using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DesktopApplicationTemplate.UI.ViewModels
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
            private set
            {
                if (_outputMessage == value) return;
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

        public TcpServiceMessagesViewModel(ServiceMessageTableViewModel messageTable, IMessageRoutingService routing)
        {
            MessageTable = messageTable ?? throw new ArgumentNullException(nameof(messageTable));
            _routing = routing ?? throw new ArgumentNullException(nameof(routing));

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
        public void SetService(ServiceViewModel service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _options = service.TcpOptions ?? new TcpServiceOptions();
            ServiceName = service.DisplayName.Split(" - ").Last();
            Script = string.IsNullOrWhiteSpace(_options.Script)
                ? ScriptEditorViewModel.DefaultScript
                : _options.Script;
            OutputMessage = _options.OutputMessage;
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
            _options.LastTestMessage = TestMessage;
            _options.Script = Script;
            _routing.UpdateMessage(ServiceName, TestMessage);
            try
            {
                OutputMessage = await RunScriptAsync().ConfigureAwait(false);
                Logger?.Log($"Script executed successfully: {OutputMessage}", LogLevel.Information);
            }
            catch (Exception ex)
            {
                OutputMessage = ex.ToString();
                Logger?.Log($"Script execution failed: {ex}", LogLevel.Error);
            }
            _options.OutputMessage = OutputMessage;
        }

        private async Task<string> RunScriptAsync()
        {
            var globals = new ScriptGlobals { message = TestMessage };
            var code = Script + "\nProcess(message);";
            var script = CSharpScript.Create<string>(code, ScriptOptions.Default, typeof(ScriptGlobals));
            var diagnostics = script.Compile();
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                return string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString()));

            var result = await script.RunAsync(globals).ConfigureAwait(false);
            return result.ReturnValue ?? string.Empty;
        }

        public class ScriptGlobals
        {
            public string message = string.Empty;
        }

        private void InitializeTestMessage()
        {
            if (string.IsNullOrWhiteSpace(ServiceName))
                return;

            TestMessage = string.IsNullOrWhiteSpace(_options.LastTestMessage)
                ? $"{ServiceName}-PEAK-123456789"
                : _options.LastTestMessage;
            _routing.UpdateMessage(ServiceName, TestMessage);
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
