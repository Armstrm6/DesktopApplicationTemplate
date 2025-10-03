using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// View model for displaying and managing service logs.
    /// </summary>
    public class ServiceLogViewModel : ViewModelBase
    {
        private ObservableCollection<LogEntry> _logs;
        private NotifyCollectionChangedEventHandler? _logsChangedHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLogViewModel"/> class
        /// with default values.
        /// </summary>
        public ServiceLogViewModel()
            : this(ServiceType.Mqtt, new ObservableCollection<LogEntry>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLogViewModel"/> class.
        /// </summary>
        /// <param name="type">The category of service associated with these logs.</param>
        /// <param name="logs">The collection of log entries to display.</param>
        public ServiceLogViewModel(ServiceType type, ObservableCollection<LogEntry> logs)
        {
            Type = type;
            _logs = logs;
            AttachLogCollection(_logs);
            RefreshLogCommand = new RelayCommand(RefreshLogs);
            ExportLogCommand = new RelayCommand(() => ExportLogs(Path.Combine(Path.GetTempPath(), "exported_logs.txt")));
            ClearLogCommand = new RelayCommand(ClearLogs);
        }

        /// <summary>
        /// Gets the service category associated with the logs.
        /// </summary>
        public ServiceType Type { get; }

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
            if (_logs == logs)
            {
                RefreshLogs();
                return;
            }

            DetachLogCollection();
            _logs = logs;
            AttachLogCollection(_logs);
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

        private void AttachLogCollection(ObservableCollection<LogEntry> logs)
        {
            _logsChangedHandler ??= (_, __) => OnPropertyChanged(nameof(DisplayLogs));
            logs.CollectionChanged += _logsChangedHandler;
        }

        private void DetachLogCollection()
        {
            if (_logsChangedHandler is null)
            {
                return;
            }

            _logs.CollectionChanged -= _logsChangedHandler;
        }
    }
}
