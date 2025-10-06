using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Views;

namespace DesktopApplicationTemplate.UI.ViewModels.Tcp
{
    /// <summary>
    /// View model for displaying TCP service messages and associated logs.
    /// </summary>
    public class TcpServiceMessagesViewModel : ViewModelBase, ILoggingViewModel
    {
        private LogLevel _logLevelFilter = LogLevel.Debug;

        /// <summary>Table view model for displaying message history.</summary>
        public ServiceMessageTableViewModel MessageTable { get; }

        /// <summary>Collection of TCP message rows.</summary>
        public ObservableCollection<TcpMessageRow> Messages { get; } = new();

        /// <summary>Incoming data extracted from <see cref="Messages"/>.</summary>
        public IEnumerable<string> IncomingData => Messages.Select(m => $"{m.IncomingIp}: {m.IncomingMessage}");

        /// <summary>Outgoing results extracted from <see cref="Messages"/>.</summary>
        public IEnumerable<string> OutgoingResults => Messages.Select(m => $"{m.ConnectedService}: {m.Result}");

        /// <summary>Collection of log entries.</summary>
        public ObservableCollection<LogEntry> Logs { get; } = new();

        /// <inheritdoc />
        private ILoggingService? _logger;
        public ILoggingService? Logger
        {
            get => _logger;
            set
            {
                if (_logger == value) return;
                if (_logger is not null)
                    _logger.LogAdded -= OnLogAdded;
                _logger = value;
                if (_logger is not null)
                    _logger.LogAdded += OnLogAdded;
            }
        }

        /// <summary>Gets or sets the minimum log level to display.</summary>
        public LogLevel LogLevelFilter
        {
            get => _logLevelFilter;
            set
            {
                if (_logLevelFilter == value) return;
                _logLevelFilter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayLogs));
            }
        }

        /// <summary>Logs matching the current <see cref="LogLevelFilter"/>.</summary>
        public IEnumerable<LogEntry> DisplayLogs => Logs.Where(l => l.Level >= LogLevelFilter);

        /// <summary>Command to clear displayed logs.</summary>
        public ICommand ClearLogCommand { get; }

        /// <summary>Command to export displayed logs to a file.</summary>
        public ICommand ExportLogCommand { get; }

        /// <summary>Command to refresh log display.</summary>
        public ICommand RefreshLogCommand { get; }

        /// <summary>Command to open advanced TCP settings.</summary>
        public ICommand OpenAdvancedSettingsCommand { get; }

        /// <summary>Command to open the script editor window.</summary>
        public ICommand OpenScriptEditorCommand { get; }

        /// <summary>Raised when the advanced settings view should open.</summary>
        public event EventHandler? AdvancedSettingsRequested;

        private readonly IMessageRoutingService _routing;
        private TcpServiceOptions _options = new();
        private TcpRuntimeContext? _runtimeContext;
        private const int MaxTcpMessageRows = 50;
        private ServiceListModel? _service;
        private CancellationTokenSource? _networkLoopCancellation;
        private Task? _networkLoopTask;
        private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(2);
        private readonly List<Task> _activeClientTasks = new();
        private readonly object _clientTasksLock = new();

        /// <summary>Type of the service associated with these messages.</summary>
        public ServiceType ServiceType { get; private set; } = ServiceType.Tcp;

        private string _serviceName = string.Empty;

        /// <summary>Name of the service associated with these messages.</summary>
        public string ServiceName
        {
            get => _serviceName;
            set
            {
                if (_serviceName == value) return;
                _serviceName = value ?? string.Empty;
                OnPropertyChanged();
                InitializeTestMessage();
            }
        }

        private string _testMessage = string.Empty;

        private string _script = string.Empty;

        private string _outputMessage = string.Empty;

        /// <summary>Message used for testing communication.</summary>
        public string TestMessage
        {
            get => _testMessage;
            set
            {
                if (_testMessage == value) return;
                _testMessage = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        /// <summary>Script applied to incoming messages before routing.</summary>
        public string Script
        {
            get => _script;
            set
            {
                if (_script == value) return;
                _script = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        /// <summary>Result of executing the test message with the current script.</summary>
        public string OutputMessage
        {
            get => _outputMessage;
            internal set
            {
                _outputMessage = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        /// <summary>Computer IP for incoming connections.</summary>
        public string ComputerIp { get; private set; } = string.Empty;

        /// <summary>Listening port for the server.</summary>
        public string ListeningPort { get; private set; } = string.Empty;

        /// <summary>Destination server IP.</summary>
        public string ServerIp { get; private set; } = string.Empty;

        /// <summary>Gateway for the destination server.</summary>
        public string ServerGateway { get; private set; } = string.Empty;

        /// <summary>Destination server port.</summary>
        public string ServerPort { get; private set; } = string.Empty;

        /// <summary>Whether UDP mode is enabled.</summary>
        public bool IsUdp { get; private set; }

        private readonly ITcpRuntime _tcpRuntime;

        public TcpServiceMessagesViewModel(ServiceMessageTableViewModel messageTable, IMessageRoutingService routing, ITcpRuntime tcpRuntime)
        {
            MessageTable = messageTable ?? throw new ArgumentNullException(nameof(messageTable));
            _routing = routing ?? throw new ArgumentNullException(nameof(routing));
            _tcpRuntime = tcpRuntime ?? throw new ArgumentNullException(nameof(tcpRuntime));
            Messages.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IncomingData));
                OnPropertyChanged(nameof(OutgoingResults));
            };

            ClearLogCommand = new RelayCommand(ClearLogs);
            ExportLogCommand = new RelayCommand(ExportLogs);
            RefreshLogCommand = new RelayCommand(() => OnPropertyChanged(nameof(DisplayLogs)));
            OpenAdvancedSettingsCommand = new RelayCommand(() => AdvancedSettingsRequested?.Invoke(this, EventArgs.Empty));
            OpenScriptEditorCommand = new AsyncRelayCommand(OpenScriptEditorAsync);
        }

        /// <summary>Associates the view model with a service and its TCP options.</summary>
        /// <param name="service">The service context.</param>
        public void SetService(ServiceListModel service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            StopNetwork();
            if (_service is not null)
            {
                _service.ActiveChanged -= OnServiceActiveChanged;
            }

            _service = service;
            _service.ActiveChanged += OnServiceActiveChanged;
            _options = service.TcpOptions ?? new TcpServiceOptions();
            ServiceType = service.Type;
            ServiceName = service.DisplayName;
            Script = string.IsNullOrWhiteSpace(_options.Script)
                ? ScriptEditorViewModel.DefaultScript
                : _options.Script;
            OutputMessage = _options.OutputMessage;
            _runtimeContext = new TcpRuntimeContext(ServiceType, ServiceName, _options, ScriptEditorViewModel.DefaultScript);
            Messages.Clear();
            MessageTable.Messages.Clear();
            OnPropertyChanged(nameof(IncomingData));
            OnPropertyChanged(nameof(OutgoingResults));
            _ = InitializeRuntimeAsync();
            if (_service.IsActive)
            {
                _ = StartNetworkAsync();
            }
        }

        private async Task InitializeRuntimeAsync()
        {
            if (_runtimeContext is null)
            {
                return;
            }

            try
            {
                var state = await _tcpRuntime.InitializeAsync(_runtimeContext).ConfigureAwait(false);
                Script = state.Script;
                TestMessage = state.TestMessage;
                OutputMessage = state.OutputMessage;
                _options.OutputMessage = state.OutputMessage;
                Logger?.Log($"Script executed successfully: {OutputMessage}", LogLevel.Information);
            }
            catch (Exception ex)
            {
                OutputMessage = ex.ToString();
                Logger?.Log($"Script execution failed: {ex}", LogLevel.Error);
            }
        }

        /// <summary>Updates network and scripting settings.</summary>
        public void UpdateNetworkSettings(string computerIp, string listeningPort, string serverIp, string serverGateway, string serverPort, bool isUdp)
        {
            ComputerIp = computerIp ?? string.Empty;
            ListeningPort = listeningPort ?? string.Empty;
            ServerIp = serverIp ?? string.Empty;
            ServerGateway = serverGateway ?? string.Empty;
            ServerPort = serverPort ?? string.Empty;
            IsUdp = isUdp;
            OnPropertyChanged(nameof(ComputerIp));
            OnPropertyChanged(nameof(ListeningPort));
            OnPropertyChanged(nameof(ServerIp));
            OnPropertyChanged(nameof(ServerGateway));
            OnPropertyChanged(nameof(ServerPort));
            OnPropertyChanged(nameof(IsUdp));
        }

        private void OnServiceActiveChanged(bool isActive)
        {
            if (isActive)
            {
                _ = StartNetworkAsync();
            }
            else
            {
                StopNetwork();
            }
        }

        private async Task StartNetworkAsync()
        {
            StopNetwork();

            if (_options.Port <= 0)
            {
                Logger?.Log("TCP port is not configured; skipping network startup.", LogLevel.Warning);
                return;
            }

            _networkLoopCancellation = new CancellationTokenSource();
            var token = _networkLoopCancellation.Token;
            var protocol = _options.UseUdp ? "UDP" : "TCP";
            Logger?.Log($"Starting TCP {_options.ConnectionRole} on {_options.Host}:{_options.Port} ({protocol})", LogLevel.Information);

            _networkLoopTask = _options.ConnectionRole == TcpConnectionRole.Server
                ? RunServerLoopAsync(token)
                : RunClientLoopAsync(token);

            if (_options.ConnectionRole == TcpConnectionRole.Server && !string.IsNullOrWhiteSpace(_options.Host))
            {
                await PingRemoteAsync().ConfigureAwait(false);
            }
        }

        private void StopNetwork()
        {
            var cts = _networkLoopCancellation;
            var task = _networkLoopTask;
            if (cts == null && task == null)
            {
                return;
            }

            if (cts != null)
            {
                _networkLoopCancellation = null;
                try
                {
                    cts.Cancel();
                }
                catch
                {
                    // ignore cancellation errors
                }
                finally
                {
                    cts.Dispose();
                }
            }

            if (task != null)
            {
                _networkLoopTask = null;
                _ = task.ContinueWith(t =>
                {
                    if (t.IsFaulted && Logger is not null)
                    {
                        Logger.Log($"TCP network loop faulted: {t.Exception?.GetBaseException().Message}", LogLevel.Error);
                    }
                }, TaskScheduler.Default);
            }

            Logger?.Log("TCP network loop stopped", LogLevel.Debug);
        }

        private async Task RunServerLoopAsync(CancellationToken cancellationToken)
        {
            TcpListener? listener = null;
            try
            {
                var listenAddress = ResolveListenAddress(_options.Host);
                if (!string.IsNullOrWhiteSpace(_options.Host) &&
                    !IPAddress.TryParse(_options.Host, out _) &&
                    !string.Equals(_options.Host, "0.0.0.0", StringComparison.Ordinal) &&
                    listenAddress.Equals(IPAddress.Any))
                {
                    LogConnectionIssues("TCP listener address resolution", null, LogLevel.Warning);
                }

                listener = new TcpListener(listenAddress, _options.Port);
                listener.Start();
                Logger?.Log($"Listening on {listenAddress}:{_options.Port}", LogLevel.Information);

                while (!cancellationToken.IsCancellationRequested)
                {
                    TcpClient client;
                    try
                    {
                        client = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    var clientTask = HandleClientAsync(client, cancellationToken);
                    TrackClientTask(clientTask);
                }
            }
            catch (Exception ex)
            {
                LogConnectionIssues("TCP listener", ex);
            }
            finally
            {
                try
                {
                    listener?.Stop();
                }
                catch
                {
                    // ignore errors during shutdown
                }
                Logger?.Log("TCP listener stopped", LogLevel.Debug);
            }
        }

        private async Task RunClientLoopAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
            {
                Logger?.Log("TCP client host is not configured.", LogLevel.Warning);
                LogConnectionIssues("TCP client configuration", null, LogLevel.Warning);
                return;
            }

            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(_options.Host, _options.Port, cancellationToken).ConfigureAwait(false);
                Logger?.Log($"Connected to {_options.Host}:{_options.Port}", LogLevel.Information);

                using var stream = client.GetStream();
                var message = _options.InputMessage ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    var payload = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(payload.AsMemory(0, payload.Length), cancellationToken).ConfigureAwait(false);
                    Logger?.Log($"Sent message to {_options.Host}:{_options.Port}: {message}", LogLevel.Information);
                }

                var buffer = new byte[4096];
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
                var endpoint = $"{_options.Host}:{_options.Port}";
                if (bytesRead > 0)
                {
                    var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Logger?.Log($"Received response from {endpoint}: {response}", LogLevel.Information);
                    await AppendMessageAsync(message, response, endpoint).ConfigureAwait(false);
                }
                else
                {
                    await AppendMessageAsync(message, string.Empty, endpoint).ConfigureAwait(false);
                    Logger?.Log($"No response received from {endpoint}", LogLevel.Warning);
                }
            }
            catch (OperationCanceledException)
            {
                // graceful cancellation
            }
            catch (Exception ex)
            {
                LogConnectionIssues("TCP client connection", ex);
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (client)
            {
                var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
                Logger?.Log($"Client connected from {endpoint}", LogLevel.Information);

                try
                {
                    using var stream = client.GetStream();
                    var buffer = new byte[4096];

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        var incoming = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Logger?.Log($"Received message from {endpoint}: {incoming}", LogLevel.Information);
                        await AppendMessageAsync(incoming, _options.OutputMessage, endpoint).ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(_options.OutputMessage))
                        {
                            var response = Encoding.UTF8.GetBytes(_options.OutputMessage);
                            await stream.WriteAsync(response.AsMemory(0, response.Length), cancellationToken).ConfigureAwait(false);
                            Logger?.Log($"Sent response to {endpoint}: {_options.OutputMessage}", LogLevel.Debug);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // graceful cancellation
                }
                catch (Exception ex)
                {
                    Logger?.Log($"Error handling client {endpoint}: {ex.Message}", LogLevel.Error);
                }
                finally
                {
                    Logger?.Log($"Client disconnected: {endpoint}", LogLevel.Information);
                }
            }
        }

        private async Task PingRemoteAsync()
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
            {
                return;
            }

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(_options.Host, (int)PingTimeout.TotalMilliseconds);
                if (reply.Status == IPStatus.Success)
                {
                    Logger?.Log($"Ping to {_options.Host} succeeded in {reply.RoundtripTime} ms", LogLevel.Information);
                }
                else
                {
                    Logger?.Log($"Ping to {_options.Host} failed with status {reply.Status}", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                LogConnectionIssues("TCP ping", ex);
            }
        }

        private Task AppendMessageAsync(string? incoming, string? outgoing, string? endpoint)
        {
            var incomingMessage = incoming ?? string.Empty;
            var outgoingMessage = outgoing ?? string.Empty;
            var destination = endpoint ?? string.Empty;

            return RunOnUiThreadAsync(() =>
            {
                Messages.Insert(0, new TcpMessageRow
                {
                    IncomingMessage = incomingMessage,
                    IncomingIp = destination,
                    OutgoingMessage = outgoingMessage,
                    ConnectedService = destination,
                    Result = string.IsNullOrWhiteSpace(outgoingMessage) ? string.Empty : outgoingMessage
                });

                while (Messages.Count > MaxTcpMessageRows)
                {
                    Messages.RemoveAt(Messages.Count - 1);
                }

                MessageTable.AddMessage(incomingMessage, outgoingMessage, destination);

                OnPropertyChanged(nameof(IncomingData));
                OnPropertyChanged(nameof(OutgoingResults));
            });
        }

        private void LogConnectionIssues(string stage, Exception? exception, LogLevel? summaryLevel = null)
        {
            if (Logger is null)
            {
                return;
            }

            if (exception is not null)
            {
                Logger.Log($"{stage} failed: {exception.Message}", LogLevel.Error);
                if (exception is SocketException socketException)
                {
                    Logger.Log($"Socket error: {socketException.SocketErrorCode}", LogLevel.Error);
                }
            }
            else if (summaryLevel is LogLevel level)
            {
                Logger.Log($"{stage} has configuration issues.", level);
            }

            foreach (var detail in BuildConnectionDiagnostics())
            {
                Logger.Log(detail, LogLevel.Warning);
            }
        }

        private IEnumerable<string> BuildConnectionDiagnostics()
        {
            yield return BuildHostDiagnostic();
            yield return BuildPortDiagnostic();
            yield return BuildSubnetDiagnostic();
            yield return BuildDnsDiagnostic(_options.PrimaryDns, "Primary DNS");
            yield return BuildDnsDiagnostic(_options.AlternateDns, "Alternate DNS");
        }

        private string BuildHostDiagnostic()
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
            {
                return "Host: not configured";
            }

            if (IPAddress.TryParse(_options.Host, out _))
            {
                return $"Host: {_options.Host}";
            }

            try
            {
                var addresses = Dns.GetHostAddresses(_options.Host)
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.ToString())
                    .ToArray();

                if (addresses.Length == 0)
                {
                    return $"Host: DNS lookup returned no IPv4 addresses for '{_options.Host}'";
                }

                var preview = string.Join(", ", addresses.Take(2));
                if (addresses.Length > 2)
                {
                    preview += ", ...";
                }

                return $"Host: resolved to {preview}";
            }
            catch (SocketException ex)
            {
                return $"Host: DNS resolution failed ({ex.SocketErrorCode})";
            }
            catch (Exception ex)
            {
                return $"Host: resolution failed ({ex.Message})";
            }
        }

        private string BuildPortDiagnostic()
        {
            return _options.Port is >= 1 and <= 65535
                ? $"Port: {_options.Port}"
                : $"Port: invalid ({_options.Port})";
        }

        private string BuildSubnetDiagnostic()
        {
            if (string.IsNullOrWhiteSpace(_options.SubnetMask))
            {
                return "Subnet: not configured";
            }

            return IPAddress.TryParse(_options.SubnetMask, out _)
                ? $"Subnet: {_options.SubnetMask}"
                : $"Subnet: invalid ({_options.SubnetMask})";
        }

        private static string BuildDnsDiagnostic(string value, string label)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return $"{label}: not configured";
            }

            return IPAddress.TryParse(value, out _)
                ? $"{label}: {value}"
                : $"{label}: invalid ({value})";
        }

        private static Task RunOnUiThreadAsync(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (App.UiThreadTaskFactory is null)
            {
                action();
                return Task.CompletedTask;
            }

            return App.UiThreadTaskFactory.RunAsync(async () =>
            {
                await App.UiThreadTaskFactory.SwitchToMainThreadAsync();
                action();
            }).Task;
        }

        private void TrackClientTask(Task clientTask)
        {
            if (clientTask is null)
            {
                throw new ArgumentNullException(nameof(clientTask));
            }

            lock (_clientTasksLock)
            {
                _activeClientTasks.Add(clientTask);
            }

            _ = clientTask.ContinueWith(t =>
            {
                lock (_clientTasksLock)
                {
                    _activeClientTasks.Remove(t);
                }

                if (t.IsFaulted && Logger is not null)
                {
                    var baseException = t.Exception?.GetBaseException();
                    if (baseException is not null)
                    {
                        Logger.Log($"TCP client handler faulted: {baseException.Message}", LogLevel.Error);
                    }
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private static IPAddress ResolveListenAddress(string host)
        {
            if (string.IsNullOrWhiteSpace(host) || string.Equals(host, "0.0.0.0", StringComparison.Ordinal))
            {
                return IPAddress.Any;
            }

            if (IPAddress.TryParse(host, out var address))
            {
                return address;
            }

            try
            {
                return Dns.GetHostAddresses(host).FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) ?? IPAddress.Any;
            }
            catch
            {
                return IPAddress.Any;
            }
        }

        private void ClearLogs()
        {
            Logs.Clear();
            OnPropertyChanged(nameof(DisplayLogs));
            Logger?.Log("TCP logs cleared", LogLevel.Debug);
        }

        private void ExportLogs()
        {
            var path = Path.Combine(Path.GetTempPath(), "tcp_logs.txt");
            File.WriteAllLines(path, DisplayLogs.Select(l => l.Message));
            Logger?.Log($"TCP logs exported to {path}", LogLevel.Debug);
        }

        private void OnLogAdded(LogEntry entry) => Logs.Insert(0, entry);

        /// <summary>Saves the current test message to options and routing.</summary>
        public async Task SaveAsync()
        {
            if (_runtimeContext is null)
            {
                return;
            }

            try
            {
                var result = await _tcpRuntime.ExecuteAsync(new TcpRuntimeExecutionRequest(_runtimeContext, Script, TestMessage)).ConfigureAwait(false);
                Script = result.Script;
                TestMessage = result.TestMessage;
                OutputMessage = result.OutputMessage;
                _options.OutputMessage = result.OutputMessage;
                Logger?.Log($"Script executed successfully: {OutputMessage}", LogLevel.Information);
            }
            catch (Exception ex)
            {
                OutputMessage = ex.ToString();
                Logger?.Log($"Script execution failed: {ex}", LogLevel.Error);
            }
        }

        private void InitializeTestMessage()
        {
            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                return;
            }

            _runtimeContext = new TcpRuntimeContext(ServiceType, ServiceName, _options, ScriptEditorViewModel.DefaultScript);
            _ = InitializeRuntimeAsync();
        }

        private async Task OpenScriptEditorAsync()
        {
            var editor = new ScriptEditorWindow();
            if (editor.DataContext is not ScriptEditorViewModel svm)
                return;

            if (!string.IsNullOrWhiteSpace(Script))
                svm.ScriptText = Script;
            svm.TestMessage = TestMessage;

            void OnOutputGenerated(string output)
            {
                OutputMessage = _options.OutputMessage = output;
                TestMessage = svm.TestMessage;
                _options.LastTestMessage = svm.TestMessage;
                _routing.UpdateMessage(ServiceType, ServiceName, svm.TestMessage);
            }

            void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(ScriptEditorViewModel.TestMessage))
                    TestMessage = svm.TestMessage;
            }

            svm.OutputGenerated += OnOutputGenerated;
            svm.PropertyChanged += OnPropertyChanged;

            var result = editor.ShowDialog() == true;

            svm.OutputGenerated -= OnOutputGenerated;
            svm.PropertyChanged -= OnPropertyChanged;

            if (result)
            {
                Script = editor.ScriptText;
                TestMessage = editor.LastTestMessage;
                await SaveAsync().ConfigureAwait(false);
            }
        }
    }
}
