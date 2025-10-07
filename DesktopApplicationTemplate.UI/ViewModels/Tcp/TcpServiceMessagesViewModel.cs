using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Core.Models;

namespace DesktopApplicationTemplate.UI.ViewModels.Tcp
{
    /// <summary>
    /// View model for displaying TCP service messages and associated logs.
    /// </summary>
    public class TcpServiceMessagesViewModel : ViewModelBase, ILoggingViewModel, INetworkAwareViewModel
    {
        private LogLevel _logLevelFilter = LogLevel.Debug;

        /// <summary>Table view model for displaying message history.</summary>
        public ServiceMessageTableViewModel MessageTable { get; }

        /// <summary>Collection of TCP message rows.</summary>
        public ObservableCollection<TcpMessageRow> Messages { get; } = new();

        /// <summary>Incoming data extracted from <see cref="Messages"/>.</summary>
        public IEnumerable<string> IncomingData => Messages
            .Select(m => m.IncomingMessage)
            .Where(message => !string.IsNullOrWhiteSpace(message));

        /// <summary>Outgoing results extracted from <see cref="Messages"/>.</summary>
        public IEnumerable<string> OutgoingResults => Messages
            .Select(m => m.Result)
            .Where(result => !string.IsNullOrWhiteSpace(result));

        /// <summary>Collection of log entries.</summary>
        public ObservableCollection<LogEntry> Logs { get; } = new();

        /// <summary>Latest incoming message text for the associated service.</summary>
        public string LastInputMessage => _service?.LastInputMessage ?? string.Empty;

        /// <summary>Latest outgoing message text for the associated service.</summary>
        public string LastOutputMessage => _service?.LastOutputMessage ?? string.Empty;

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
        private bool _isApplicationExitHooked;
        private NetworkConfiguration _networkConfiguration = new();
        private static readonly TimeSpan ClientReconnectDelay = TimeSpan.FromSeconds(2);

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

            EnsureApplicationExitHooked();
        }

        /// <summary>Associates the view model with a service and its TCP options.</summary>
        /// <param name="service">The service context.</param>
        public void SetService(ServiceListModel service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            StopNetwork();
            EnsureApplicationExitHooked();
            if (_service is not null)
            {
                _service.ActiveChanged -= OnServiceActiveChanged;
                _service.PropertyChanged -= OnServicePropertyChanged;
            }

            _service = service;
            _service.ActiveChanged += OnServiceActiveChanged;
            _service.PropertyChanged += OnServicePropertyChanged;
            _options = service.TcpOptions ?? new TcpServiceOptions();
            ServiceType = service.Type;
            ServiceName = service.DisplayName;
            Script = string.IsNullOrWhiteSpace(_options.Script)
                ? ScriptEditorViewModel.DefaultScript
                : _options.Script;
            OutputMessage = _options.OutputMessage;
            _runtimeContext = new TcpRuntimeContext(ServiceType, ServiceName, _options, ScriptEditorViewModel.DefaultScript);
            ApplyNetworkConfiguration(restartIfActive: false);
            var history = service.GetMessageHistorySnapshot();
            MessageTable.LoadMessages(ServiceType, ServiceName, history);
            Messages.Clear();
            foreach (var entry in history.AsEnumerable().Reverse())
            {
                var incomingDisplay = MessageDisplayFormatter.FormatForService(entry.IncomingMessage, true, ServiceType, ServiceName);
                var outgoingDisplay = MessageDisplayFormatter.FormatForService(entry.OutgoingMessage, false, ServiceType, ServiceName);
                Messages.Insert(0, new TcpMessageRow
                {
                    IncomingMessage = incomingDisplay,
                    IncomingIp = string.Empty,
                    OutgoingMessage = outgoingDisplay,
                    ConnectedService = entry.Destination ?? string.Empty,
                    Result = string.IsNullOrEmpty(outgoingDisplay) ? string.Empty : outgoingDisplay
                });
            }
            MessageTable.SetActiveService(ServiceType, ServiceName);
            OnPropertyChanged(nameof(IncomingData));
            OnPropertyChanged(nameof(OutgoingResults));
            OnPropertyChanged(nameof(LastInputMessage));
            OnPropertyChanged(nameof(LastOutputMessage));
            _ = InitializeRuntimeAsync();
            if (_service.IsActive)
            {
                _ = StartNetworkAsync();
            }
        }

        private void OnServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ServiceListModel.LastInputMessage) || string.IsNullOrEmpty(e.PropertyName))
            {
                OnPropertyChanged(nameof(LastInputMessage));
            }

            if (e.PropertyName == nameof(ServiceListModel.LastOutputMessage) || string.IsNullOrEmpty(e.PropertyName))
            {
                OnPropertyChanged(nameof(LastOutputMessage));
            }
        }

        /// <inheritdoc />
        public void UpdateNetworkConfiguration(NetworkConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _networkConfiguration = configuration;
            ApplyNetworkConfiguration(restartIfActive: true);
        }

        private void ApplyNetworkConfiguration(bool restartIfActive)
        {
            if (_options is null)
            {
                return;
            }

            var config = _networkConfiguration ?? new NetworkConfiguration();

            if (_options.ConnectionRole == TcpConnectionRole.Server && !string.IsNullOrWhiteSpace(config.IpAddress))
            {
                _options.Host = config.IpAddress;
            }

            _options.SubnetMask = config.SubnetMask ?? string.Empty;
            _options.PrimaryDns = config.DnsPrimary ?? string.Empty;
            _options.AlternateDns = config.DnsSecondary ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_options.DestinationGateway))
            {
                _options.DestinationGateway = config.Gateway ?? string.Empty;
            }

            UpdateNetworkSettings(
                _options.Host,
                _options.Port > 0 ? _options.Port.ToString(CultureInfo.InvariantCulture) : string.Empty,
                _options.DestinationHost,
                _options.DestinationGateway,
                _options.DestinationPort > 0 ? _options.DestinationPort.ToString(CultureInfo.InvariantCulture) : string.Empty,
                _options.UseUdp);

            if (restartIfActive && _service?.IsActive == true)
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
                _routing.UpdateMessage(ServiceType, ServiceName, state.TestMessage, MessageRoutingDirection.Input);
                _routing.UpdateMessage(ServiceType, ServiceName, state.OutputMessage, MessageRoutingDirection.Output);
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

            var hasListener = ShouldStartServerListener();
            if (hasListener && _options.Port <= 0)
            {
                Logger?.Log("TCP port is not configured; skipping listener startup.", LogLevel.Warning);
                hasListener = false;
            }

            var hasClientReceiver = TryGetClientReceiveEndpoint(out var clientReceiveEndpoint);
            var hasClientSender = TryGetClientSendEndpoint(out var clientSendEndpoint);

            if (!hasListener && !hasClientReceiver && !hasClientSender)
            {
                Logger?.Log("TCP service is not configured to listen or send; network startup skipped.", LogLevel.Warning);
                return;
            }

            var pingSucceeded = await EnsureRemoteHostsReachableAsync(
                hasClientReceiver ? clientReceiveEndpoint : null,
                hasClientSender ? clientSendEndpoint : null).ConfigureAwait(false);

            if (!pingSucceeded)
            {
                return;
            }

            _networkLoopCancellation = new CancellationTokenSource();
            var token = _networkLoopCancellation.Token;
            var protocol = _options.UseUdp ? "UDP" : "TCP";

            var tasks = new List<Task>();
            if (hasListener)
            {
                Logger?.Log($"Starting TCP listener on {_options.Host}:{_options.Port} ({protocol})", LogLevel.Information);
                tasks.Add(RunServerLoopAsync(token));
            }

            if (hasClientReceiver && clientReceiveEndpoint is TcpEndpoint receiveEndpoint)
            {
                Logger?.Log($"Starting TCP client to {receiveEndpoint.Host}:{receiveEndpoint.Port} ({protocol}) for receiving", LogLevel.Information);
                tasks.Add(RunClientLoopAsync(receiveEndpoint, TcpClientOperation.Receive, token));
            }

            if (hasClientSender && clientSendEndpoint is TcpEndpoint sendEndpoint)
            {
                Logger?.Log($"Starting TCP client to {sendEndpoint.Host}:{sendEndpoint.Port} ({protocol}) for sending", LogLevel.Information);
                tasks.Add(RunClientLoopAsync(sendEndpoint, TcpClientOperation.Send, token));
            }

            _networkLoopTask = Task.WhenAll(tasks);
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

            List<Task> clientTasks;
            lock (_clientTasksLock)
            {
                clientTasks = _activeClientTasks.ToList();
            }

            if (clientTasks.Count > 0)
            {
                _ = Task.WhenAll(clientTasks).ContinueWith(t =>
                {
                    if (t.IsFaulted && Logger is not null)
                    {
                        Logger.Log($"One or more TCP client handlers faulted during shutdown: {t.Exception?.GetBaseException().Message}", LogLevel.Error);
                    }
                }, TaskScheduler.Default);
            }

            Logger?.Log("TCP network loop stopped", LogLevel.Debug);
        }

        private bool ShouldStartServerListener()
        {
            return _options.ConnectionRole == TcpConnectionRole.Server &&
                   _options.Mode is TcpServiceMode.Listening or TcpServiceMode.ReceiveAndSend;
        }

        private bool TryGetClientReceiveEndpoint(out TcpEndpoint? endpoint)
        {
            endpoint = null;
            if (_options.ConnectionRole != TcpConnectionRole.Client)
            {
                return false;
            }

            if (_options.Mode is not (TcpServiceMode.Listening or TcpServiceMode.ReceiveAndSend))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(_options.Host) || _options.Port <= 0)
            {
                Logger?.Log("TCP client listening host or port is not configured; skipping client startup.", LogLevel.Warning);
                LogConnectionIssues("TCP client configuration", null, LogLevel.Warning);
                return false;
            }

            endpoint = new TcpEndpoint(_options.Host, _options.Port);
            return true;
        }

        private bool TryGetClientSendEndpoint(out TcpEndpoint? endpoint)
        {
            endpoint = null;
            if (_options.ConnectionRole != TcpConnectionRole.Client)
            {
                return false;
            }

            if (_options.Mode is not (TcpServiceMode.Sending or TcpServiceMode.ReceiveAndSend))
            {
                return false;
            }

            var host = ResolveDestinationHost();
            var port = ResolveDestinationPort();
            if (string.IsNullOrWhiteSpace(host) || port is null)
            {
                Logger?.Log("TCP client destination is not configured; skipping client startup.", LogLevel.Warning);
                LogConnectionIssues("TCP client configuration", null, LogLevel.Warning);
                return false;
            }

            endpoint = new TcpEndpoint(host, port.Value);
            return true;
        }

        private async Task<bool> EnsureRemoteHostsReachableAsync(TcpEndpoint? receiveEndpoint, TcpEndpoint? sendEndpoint)
        {
            var hostsToPing = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (receiveEndpoint is { } receive)
            {
                hostsToPing.TryAdd(receive.Host, "listening client");
            }

            if (sendEndpoint is { } send)
            {
                hostsToPing.TryAdd(send.Host, "client");
            }

            foreach (var pair in hostsToPing)
            {
                if (await PingHostAsync(pair.Key).ConfigureAwait(false))
                {
                    continue;
                }

                Logger?.Log($"Cannot start TCP {pair.Value} because {pair.Key} did not respond to ping.", LogLevel.Error);
                return false;
            }

            return true;
        }

        private async Task<bool> PingHostAsync(string host)
        {
            if (string.IsNullOrWhiteSpace(host) || string.Equals(host, "0.0.0.0", StringComparison.Ordinal))
            {
                Logger?.Log("Ping skipped because no remote host is configured.", LogLevel.Debug);
                return true;
            }

            if (IsLocalHost(host))
            {
                Logger?.Log($"Ping skipped for local host {host}.", LogLevel.Debug);
                return true;
            }

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, (int)PingTimeout.TotalMilliseconds).ConfigureAwait(false);
                if (reply.Status == IPStatus.Success)
                {
                    Logger?.Log($"Ping to {host} succeeded in {reply.RoundtripTime} ms", LogLevel.Information);
                    return true;
                }

                Logger?.Log($"Ping to {host} failed with status {reply.Status}", LogLevel.Error);
                LogConnectionIssues($"TCP ping to {host}", null, LogLevel.Error);
                return false;
            }
            catch (Exception ex)
            {
                LogConnectionIssues($"TCP ping to {host}", ex);
                return false;
            }
        }

        private enum TcpClientOperation
        {
            Receive,
            Send
        }

        private readonly record struct TcpEndpoint(string Host, int Port);

        private async Task RunServerLoopAsync(CancellationToken cancellationToken)
        {
            TcpListener? listener = null;
            try
            {
                var listenAddress = ResolveListenAddress(_options.Host, out var fallbackToAny);
                if (fallbackToAny && !string.IsNullOrWhiteSpace(_options.Host))
                {
                    Logger?.Log($"Requested listen address '{_options.Host}' is not assigned to this machine; listening on all interfaces instead.", LogLevel.Warning);
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

        private async Task RunClientLoopAsync(TcpEndpoint endpoint, TcpClientOperation operation, CancellationToken cancellationToken)
        {
            var operationLabel = operation == TcpClientOperation.Receive ? "listening endpoint" : "destination";
            var endpointDisplay = $"{endpoint.Host}:{endpoint.Port}";

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync(endpoint.Host, endpoint.Port, cancellationToken).ConfigureAwait(false);
                    Logger?.Log($"Connected to {operationLabel} {endpointDisplay}", LogLevel.Information);

                    using var stream = client.GetStream();
                    var message = _options.InputMessage ?? string.Empty;
                    var shouldSendMessage = operation == TcpClientOperation.Send && !string.IsNullOrWhiteSpace(message);
                    var pendingOutgoing = string.Empty;

                    if (shouldSendMessage)
                    {
                        var payload = Encoding.UTF8.GetBytes(message);
                        await stream.WriteAsync(payload.AsMemory(0, payload.Length), cancellationToken).ConfigureAwait(false);
                        Logger?.Log($"Sent message to {operationLabel} {endpointDisplay}: {message}", LogLevel.Information);
                        _routing.UpdateMessage(ServiceType, ServiceName, message, MessageRoutingDirection.Output);
                        pendingOutgoing = message;
                    }

                    var buffer = new byte[4096];

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        int bytesRead;
                        try
                        {
                            bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }

                        if (bytesRead == 0)
                        {
                            Logger?.Log($"Connection closed by {endpointDisplay}", LogLevel.Information);
                            break;
                        }

                        var payload = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        if (operation == TcpClientOperation.Receive)
                        {
                            Logger?.Log($"Received message from {endpointDisplay}: {payload}", LogLevel.Information);
                            _routing.UpdateMessage(ServiceType, ServiceName, payload, MessageRoutingDirection.Input);
                            await AppendMessageAsync(payload, string.Empty, endpointDisplay).ConfigureAwait(false);
                        }
                        else
                        {
                            Logger?.Log($"Received response from {endpointDisplay}: {payload}", LogLevel.Information);
                            _routing.UpdateMessage(ServiceType, ServiceName, payload, MessageRoutingDirection.Input);
                            await AppendMessageAsync(payload, pendingOutgoing, endpointDisplay).ConfigureAwait(false);
                            pendingOutgoing = string.Empty;
                        }
                    }

                    if (!string.IsNullOrEmpty(pendingOutgoing))
                    {
                        await AppendMessageAsync(string.Empty, pendingOutgoing, endpointDisplay).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    // graceful cancellation
                    break;
                }
                catch (Exception ex)
                {
                    LogConnectionIssues($"TCP client connection to {endpoint.Host}:{endpoint.Port}", ex);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(ClientReconnectDelay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
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
                        var formattedIncoming = MessageDisplayFormatter.FormatControlCharacters(incoming);
                        Logger?.Log($"Incoming message from {endpoint}: {formattedIncoming}", LogLevel.Information);
                        _routing.UpdateMessage(ServiceType, ServiceName, incoming, MessageRoutingDirection.Input);
                        await AppendMessageAsync(incoming, _options.OutputMessage, endpoint).ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(_options.OutputMessage))
                        {
                            var response = Encoding.UTF8.GetBytes(_options.OutputMessage);
                            await stream.WriteAsync(response.AsMemory(0, response.Length), cancellationToken).ConfigureAwait(false);
                            var formattedOutgoing = MessageDisplayFormatter.FormatControlCharacters(_options.OutputMessage);
                            Logger?.Log($"Outgoing message to {endpoint}: {formattedOutgoing}", LogLevel.Debug);
                            _routing.UpdateMessage(ServiceType, ServiceName, _options.OutputMessage, MessageRoutingDirection.Output);
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

        private Task AppendMessageAsync(string? incoming, string? outgoing, string? endpoint)
        {
            var incomingMessage = incoming ?? string.Empty;
            var outgoingMessage = outgoing ?? string.Empty;
            var incomingEndpoint = endpoint ?? string.Empty;
            var referencingServices = _routing.GetReferencingServices(ServiceName);
            var destination = string.Empty;
            if (!string.IsNullOrWhiteSpace(outgoingMessage))
            {
                destination = incomingEndpoint;
            }
            else if (referencingServices.Count > 0)
            {
                destination = string.Join(", ", referencingServices);
            }
            var incomingDisplay = MessageDisplayFormatter.FormatForService(incomingMessage, true, ServiceType, ServiceName);
            var outgoingDisplay = MessageDisplayFormatter.FormatForService(outgoingMessage, false, ServiceType, ServiceName);

            return RunOnUiThreadAsync(() =>
            {
                Messages.Insert(0, new TcpMessageRow
                {
                    IncomingMessage = incomingDisplay,
                    IncomingIp = incomingEndpoint,
                    OutgoingMessage = outgoingDisplay,
                    ConnectedService = destination,
                    Result = string.IsNullOrEmpty(outgoingDisplay) ? string.Empty : outgoingDisplay
                });

                while (Messages.Count > MaxTcpMessageRows)
                {
                    Messages.RemoveAt(Messages.Count - 1);
                }

                MessageTable.AddMessage(ServiceType, ServiceName, incomingMessage, outgoingMessage, destination);

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
            foreach (var destinationDetail in BuildDestinationDiagnostics())
            {
                yield return destinationDetail;
            }
        }

        private string BuildHostDiagnostic()
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
            {
                return "Host: not configured";
            }

            if (IPAddress.TryParse(_options.Host, out var parsedAddress))
            {
                var locality = IsLocalAddress(parsedAddress) ? "local" : "remote";
                return $"Host: {_options.Host} ({locality})";
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

        private IEnumerable<string> BuildDestinationDiagnostics()
        {
            if (_options.ConnectionRole == TcpConnectionRole.Client &&
                _options.Mode is TcpServiceMode.Sending or TcpServiceMode.ReceiveAndSend)
            {
                var destinationHost = ResolveDestinationHost();
                var destinationPort = ResolveDestinationPort();
                if (string.IsNullOrWhiteSpace(destinationHost) || destinationPort is null)
                {
                    yield return "Destination: not configured";
                }
                else
                {
                    yield return $"Destination: {destinationHost}:{destinationPort}";
                }
                yield break;
            }

            var referencing = _routing.GetReferencingServices(ServiceName);
            yield return referencing.Count > 0
                ? $"Destination references: {string.Join(", ", referencing)}"
                : "Destination references: none";
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

        private void EnsureApplicationExitHooked()
        {
            if (_isApplicationExitHooked || Application.Current is null)
            {
                return;
            }

            Application.Current.Exit += OnApplicationExit;
            _isApplicationExitHooked = true;
        }

        private void OnApplicationExit(object? sender, ExitEventArgs e)
        {
            StopNetwork();
        }

        private static IPAddress ResolveListenAddress(string host, out bool fallbackToAny)
        {
            fallbackToAny = false;

            if (string.IsNullOrWhiteSpace(host) || string.Equals(host, "0.0.0.0", StringComparison.Ordinal))
            {
                return IPAddress.Any;
            }

            if (IPAddress.TryParse(host, out var address))
            {
                if (IsLocalAddress(address))
                {
                    return address;
                }

                fallbackToAny = true;
                return IPAddress.Any;
            }

            try
            {
                var resolved = Dns.GetHostAddresses(host)
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                    .FirstOrDefault(IsLocalAddress);

                if (resolved is not null)
                {
                    return resolved;
                }
            }
            catch
            {
                // ignore resolution failures and fall back to any address
            }

            fallbackToAny = true;
            return IPAddress.Any;
        }

        private static bool IsLocalAddress(IPAddress address)
        {
            if (address.Equals(IPAddress.Any) || IPAddress.IsLoopback(address))
            {
                return true;
            }

            try
            {
                foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    {
                        continue;
                    }

                    foreach (var unicast in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (unicast.Address.AddressFamily != AddressFamily.InterNetwork)
                        {
                            continue;
                        }

                        if (address.Equals(unicast.Address))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // ignore adapter enumeration failures
            }

            return false;
        }

        private static bool IsLocalHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (IPAddress.TryParse(host, out var address))
            {
                return IsLocalAddress(address);
            }

            try
            {
                return Dns.GetHostAddresses(host)
                    .Any(a => a.AddressFamily == AddressFamily.InterNetwork && IsLocalAddress(a));
            }
            catch
            {
                return false;
            }
        }

        private string ResolveDestinationHost()
        {
            return string.IsNullOrWhiteSpace(_options.DestinationHost)
                ? string.Empty
                : _options.DestinationHost;
        }

        private int? ResolveDestinationPort()
        {
            return _options.DestinationPort > 0 ? _options.DestinationPort : null;
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
                _routing.UpdateMessage(ServiceType, ServiceName, result.TestMessage, MessageRoutingDirection.Input);
                _routing.UpdateMessage(ServiceType, ServiceName, result.OutputMessage, MessageRoutingDirection.Output);
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
            svm.RoutingService = _routing;
            svm.RoutingServiceName = ServiceName;

            void OnOutputGenerated(string output)
            {
                OutputMessage = _options.OutputMessage = output;
                TestMessage = svm.TestMessage;
                _options.LastTestMessage = svm.TestMessage;
                _routing.UpdateMessage(ServiceType, ServiceName, svm.TestMessage, MessageRoutingDirection.Input);
                _routing.UpdateMessage(ServiceType, ServiceName, output, MessageRoutingDirection.Output);
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
