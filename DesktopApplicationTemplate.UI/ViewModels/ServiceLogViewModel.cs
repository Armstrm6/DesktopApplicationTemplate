using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// View model for displaying and managing service logs.
    /// </summary>
    public class ServiceLogViewModel : ViewModelBase
    {
        private ObservableCollection<LogEntry> _logs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLogViewModel"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="logs">The collection of log entries to display.</param>
        public ServiceLogViewModel(string serviceName, string serviceType, ObservableCollection<LogEntry> logs)
        {
            ServiceName = serviceName;
            ServiceType = serviceType;
            _logs = logs;
            RefreshLogCommand = new RelayCommand(_ => RefreshLogs());
            ExportLogCommand = new RelayCommand(_ => ExportLogs(Path.Combine(Path.GetTempPath(), "exported_logs.txt")));
            ClearLogCommand = new RelayCommand(_ => ClearLogs());
        }

        /// <summary>
        /// Gets the service name associated with the logs.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Gets the service type associated with the logs.
        /// </summary>
        public string ServiceType { get; }

        /// <summary>
        /// Gets or sets the log level filter.
        /// </summary>
        private LogLevel _logLevelFilter = LogLevel.Debug;
        public LogLevel LogLevelFilter
        {
            get => _logLevelFilter;
            set { _logLevelFilter = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayLogs)); }
        }

        /// <summary>
        /// Gets the log entries currently displayed.
        /// </summary>
        public IEnumerable<LogEntry> DisplayLogs => _logs.Where(l => l.Level >= LogLevelFilter);

        /// <summary>
        /// Gets the command that refreshes the log display.
        /// </summary>
        public ICommand RefreshLogCommand { get; }

        /// <summary>
        /// Gets the command that exports the current logs to a temporary file.
        /// </summary>
        public ICommand ExportLogCommand { get; }

        /// <summary>
        /// Gets the command that clears the current logs.
        /// </summary>
        public ICommand ClearLogCommand { get; }

        /// <summary>
        /// Replaces the backing log collection.
        /// </summary>
        /// <param name="logs">The new log collection.</param>
        public void SetLogs(ObservableCollection<LogEntry> logs)
        {
            _logs = logs;
            OnPropertyChanged(nameof(DisplayLogs));
        }

        /// <summary>
        /// Clears all logs in the underlying collection.
        /// </summary>
        public void ClearLogs()
        {
            _logs.Clear();
            OnPropertyChanged(nameof(DisplayLogs));
        }

        /// <summary>
        /// Exports the displayed logs to the specified file path.
        /// </summary>
        /// <param name="filePath">The destination file path.</param>
        public void ExportLogs(string filePath)
        {
            var lines = DisplayLogs.Select(l => l.Message).ToList();
            File.WriteAllLines(filePath, lines);
        }

        /// <summary>
        /// Forces the display to refresh.
        /// </summary>
        public void RefreshLogs() => OnPropertyChanged(nameof(DisplayLogs));
    }
}
