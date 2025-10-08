using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using CoreLogLevel = DesktopApplicationTemplate.Core.Services.LogLevel;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// View model for displaying and managing service logs.
    /// </summary>
    public class ServiceLogViewModel : ViewModelBase
    {
        private readonly ILogger<ServiceLogViewModel> _logger;
        private ObservableCollection<LogEntry> _logs;
        private NotifyCollectionChangedEventHandler? _logsChangedHandler;

        /// <summary>
        /// Gets the collection of service filters applied in aggregated mode.
        /// </summary>
        public ObservableCollection<ServiceLogFilterOption> ServiceFilters { get; } = new();

        private bool _isAggregated;
        public bool IsAggregated
        {
            get => _isAggregated;
            private set
            {
                if (_isAggregated == value)
                {
                    return;
                }

                _isAggregated = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayLogs));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLogViewModel"/> class
        /// with default values.
        /// </summary>
        public ServiceLogViewModel()
            : this(ServiceType.Mqtt, new ObservableCollection<LogEntry>(), null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLogViewModel"/> class.
        /// </summary>
        /// <param name="type">The category of service associated with these logs.</param>
        /// <param name="logs">The collection of log entries to display.</param>
        public ServiceLogViewModel(ServiceType type, ObservableCollection<LogEntry> logs, ILogger<ServiceLogViewModel>? logger = null, bool isAggregated = false)
        {
            Type = type;
            _logs = logs;
            _logger = logger ?? NullLogger<ServiceLogViewModel>.Instance;
            IsAggregated = isAggregated;
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
        private CoreLogLevel _logLevelFilter = CoreLogLevel.Debug;
        public CoreLogLevel LogLevelFilter
        {
            get => _logLevelFilter;
            set { _logLevelFilter = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayLogs)); }
        }

        /// <summary>
        /// Gets the log entries currently displayed.
        /// </summary>
        public IEnumerable<LogEntry> DisplayLogs
        {
            get
            {
                var filteredByLevel = _logs.Where(l => l.Level >= LogLevelFilter);

                if (!IsAggregated)
                {
                    return filteredByLevel;
                }

                var selectedServices = ServiceFilters
                    .Where(filter => filter.IsSelected)
                    .Select(filter => filter.ServiceName)
                    .ToList();

                if (selectedServices.Count == 0)
                {
                    return Array.Empty<LogEntry>();
                }

                var selectedSet = new HashSet<string>(selectedServices, StringComparer.OrdinalIgnoreCase);
                return filteredByLevel.Where(entry =>
                    string.IsNullOrWhiteSpace(entry.ServiceName) || selectedSet.Contains(entry.ServiceName));
            }
        }

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
        /// <param name="isAggregated">Indicates whether the view is displaying aggregated logs.</param>
        public void SetLogs(ObservableCollection<LogEntry> logs, bool isAggregated = false)
        {
            if (logs is null)
            {
                return;
            }

            if (_logs == logs)
            {
                IsAggregated = isAggregated;
                RefreshLogs();
                return;
            }

            DetachLogCollection();
            _logs = logs;
            AttachLogCollection(_logs);
            IsAggregated = isAggregated;
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
        /// Updates the available service filters displayed in aggregated mode.
        /// </summary>
        /// <param name="serviceNames">The service display names to expose in the filter list.</param>
        public void UpdateServiceFilters(IEnumerable<string> serviceNames)
        {
            var normalizedNames = (serviceNames ?? Enumerable.Empty<string>())
                .Select(name => name?.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (var i = ServiceFilters.Count - 1; i >= 0; i--)
            {
                var filter = ServiceFilters[i];
                if (!normalizedNames.Contains(filter.ServiceName, StringComparer.OrdinalIgnoreCase))
                {
                    filter.PropertyChanged -= OnServiceFilterOptionPropertyChanged;
                    ServiceFilters.RemoveAt(i);
                }
            }

            foreach (var name in normalizedNames)
            {
                if (ServiceFilters.Any(f => f.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var filter = new ServiceLogFilterOption(name)
                {
                    IsSelected = true
                };
                filter.PropertyChanged += OnServiceFilterOptionPropertyChanged;
                ServiceFilters.Add(filter);
            }

            OnPropertyChanged(nameof(DisplayLogs));
        }

        /// <summary>
        /// Exports the displayed logs to the specified file path.
        /// </summary>
        /// <param name="filePath">The destination file path.</param>
        public void ExportLogs(string filePath)
        {
            try
            {
                var lines = DisplayLogs.Select(l => l.Message).ToList();
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Failed to export logs to {FilePath}.", filePath);
                MessageBox.Show(
                    $"Failed to export logs to '{filePath}': {ex.Message}",
                    "Export Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Forces the display to refresh.
        /// </summary>
        public void RefreshLogs() => OnPropertyChanged(nameof(DisplayLogs));

        private void OnServiceFilterOptionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.Equals(e.PropertyName, nameof(ServiceLogFilterOption.IsSelected), StringComparison.Ordinal))
            {
                return;
            }

            OnPropertyChanged(nameof(DisplayLogs));
        }

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

    /// <summary>
    /// Represents a selectable service filter used by aggregated log views.
    /// </summary>
    public class ServiceLogFilterOption : ViewModelBase
    {
        public ServiceLogFilterOption(string serviceName)
        {
            ServiceName = serviceName ?? string.Empty;
        }

        /// <summary>
        /// Gets the display name for the service associated with this filter.
        /// </summary>
        public string ServiceName { get; }

        private bool _isSelected = true;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}
