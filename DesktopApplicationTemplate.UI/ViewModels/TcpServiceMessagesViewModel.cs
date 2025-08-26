using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// View model for displaying TCP service messages and associated logs.
    /// </summary>
    public class TcpServiceMessagesViewModel : ViewModelBase, ILoggingViewModel
    {
        private LogLevel _logLevelFilter = LogLevel.Debug;

        /// <summary>Collection of TCP message rows.</summary>
        public ObservableCollection<TcpMessageRow> Messages { get; } = new();

        /// <summary>Incoming data extracted from <see cref="Messages"/>.</summary>
        public IEnumerable<string> IncomingData => Messages.Select(m => $"{m.IncomingIp}: {m.IncomingMessage}");

        /// <summary>Script modifications extracted from <see cref="Messages"/>.</summary>
        public IEnumerable<string> ScriptModifications => Messages.Select(m => m.OutgoingMessage);

        /// <summary>Outgoing results extracted from <see cref="Messages"/>.</summary>
        public IEnumerable<string> OutgoingResults => Messages.Select(m => $"{m.ConnectedService}: {m.Result}");

        /// <summary>Collection of log entries.</summary>
        public ObservableCollection<LogEntry> Logs { get; } = new();

        /// <inheritdoc />
        public ILoggingService? Logger { get; set; }

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

        /// <summary>Raised when the advanced settings view should open.</summary>
        public event EventHandler? AdvancedSettingsRequested;

        /// <summary>Current script content.</summary>
        public string ScriptContent { get; private set; } = string.Empty;

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

        public TcpServiceMessagesViewModel()
        {
            Messages.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IncomingData));
                OnPropertyChanged(nameof(ScriptModifications));
                OnPropertyChanged(nameof(OutgoingResults));
            };

            ClearLogCommand = new RelayCommand(ClearLogs);
            ExportLogCommand = new RelayCommand(ExportLogs);
            RefreshLogCommand = new RelayCommand(() => OnPropertyChanged(nameof(DisplayLogs)));
            OpenAdvancedSettingsCommand = new RelayCommand(() => AdvancedSettingsRequested?.Invoke(this, EventArgs.Empty));
        }

        /// <summary>Updates the stored script content.</summary>
        /// <param name="script">New script text.</param>
        public void UpdateScript(string script)
        {
            ScriptContent = script ?? string.Empty;
            OnPropertyChanged(nameof(ScriptContent));
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
    }
}
