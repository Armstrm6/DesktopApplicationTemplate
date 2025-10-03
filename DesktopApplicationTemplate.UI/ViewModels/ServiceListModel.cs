using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Services;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public enum ServiceRuntimeState
    {
        Inactive,
        Activating,
        Active,
        Error
    }

    public class ServiceListModel : ViewModelBase
    {
        public string DisplayName { get; set; } = string.Empty;
        public ServiceType Type { get; set; }
        [JsonIgnore] public Page? Page { get; set; }
        public int Order { get; set; }

        private WpfBrush _backgroundColor = WpfBrushes.LightGray;
        public WpfBrush BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(); }
        }

        private WpfBrush _borderColor = WpfBrushes.Gray;
        public WpfBrush BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; OnPropertyChanged(); }
        }
        [JsonIgnore] public Page? ServicePage { get; set; }

        public ObservableCollection<string> AssociatedServices { get; } = new();

        private double _totalExecutionTimeMs;
        private int _executionCount;
        private TimeSpan _lastExecutionDuration;
        private string _lastInputMessage = string.Empty;

        /// <summary>
        /// Gets the average execution time in milliseconds for operations performed by this service.
        /// </summary>
        public double? AverageExecutionTimeMs => _executionCount == 0 ? null : _totalExecutionTimeMs / _executionCount;

        /// <summary>
        /// Gets the duration of the most recent execution for this service.
        /// </summary>
        public TimeSpan LastExecutionDuration
        {
            get => _lastExecutionDuration;
            private set
            {
                _lastExecutionDuration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExecutionTimeText));
            }
        }

        /// <summary>
        /// Gets a formatted string displaying the last execution duration and average.
        /// </summary>
        public string ExecutionTimeText => _executionCount == 0
            ? string.Empty
            : $"Last: {LastExecutionDuration.TotalMilliseconds:F0} ms (Avg: {AverageExecutionTimeMs:F0} ms)";

        /// <summary>
        /// Gets the last input message received by this service.
        /// </summary>
        public string LastInputMessage
        {
            get => _lastInputMessage;
            private set
            {
                _lastInputMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the accumulated execution time in milliseconds.
        /// </summary>
        public double TotalExecutionTimeMs
        {
            get => _totalExecutionTimeMs;
            set
            {
                _totalExecutionTimeMs = value;
                OnPropertyChanged(nameof(AverageExecutionTimeMs));
                OnPropertyChanged(nameof(ExecutionTimeText));
            }
        }

        /// <summary>
        /// Gets or sets the number of execution samples recorded for this service.
        /// </summary>
        public int ExecutionCount
        {
            get => _executionCount;
            set
            {
                _executionCount = value;
                OnPropertyChanged(nameof(AverageExecutionTimeMs));
                OnPropertyChanged(nameof(ExecutionTimeText));
            }
        }

        /// <summary>
        /// TCP-specific configuration for this service, if applicable.
        /// </summary>
        public TcpServiceOptions? TcpOptions { get; set; }

        /// <summary>
        /// FTP server-specific configuration for this service, if applicable.
        /// </summary>
        public FtpServerOptions? FtpOptions { get; set; }

        /// <summary>
        /// HTTP-specific configuration for this service, if applicable.
        /// </summary>
        public HttpServiceOptions? HttpOptions { get; set; }

        /// <summary>
        /// HID-specific configuration for this service, if applicable.
        /// </summary>
        public HidServiceOptions? HidOptions { get; set; }

        /// <summary>
        /// Heartbeat-specific configuration for this service, if applicable.
        /// </summary>
        public HeartbeatServiceOptions? HeartbeatOptions { get; set; }

        /// <summary>
        /// File Observer-specific configuration for this service, if applicable.
        /// </summary>
        public FileObserverServiceOptions? FileObserverOptions { get; set; }

        /// <summary>
        /// SCP-specific configuration for this service, if applicable.
        /// </summary>
        public ScpServiceOptions? ScpOptions { get; set; }

        /// <summary>
        /// CSV creator-specific configuration for this service, if applicable.
        /// </summary>
        public CsvServiceOptions? CsvOptions { get; set; }

        public static Func<ServiceType, string, ServiceListModel?>? ResolveService { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                    if (_isActive)
                    {
                        AddLog("[Service Activated]", WpfBrushes.Green);
                    }
                    else
                    {
                        if (RuntimeState != ServiceRuntimeState.Error)
                        {
                            RuntimeState = ServiceRuntimeState.Inactive;
                        }
                        AddLog("[Service Deactivated]", WpfBrushes.Red);
                    }
                    ActiveChanged?.Invoke(_isActive);
                }
            }
        }

        public void SetRuntimeState(ServiceRuntimeState state)
        {
            RuntimeState = state;
        }

        private ServiceRuntimeState _runtimeState = ServiceRuntimeState.Inactive;
        public ServiceRuntimeState RuntimeState
        {
            get => _runtimeState;
            private set
            {
                if (_runtimeState != value)
                {
                    _runtimeState = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<LogEntry> Logs { get; set; } = new();
        public event Action<bool>? ActiveChanged;

        public event Action<ServiceListModel, LogEntry>? LogAdded;

        public void AddLog(string message, WpfBrush? color = null, LogLevel level = LogLevel.Debug, bool checkReference = true)
        {
            LastInputMessage = message;
            var ts = DateTime.Now.ToString("MM.dd.yyyy - HH:mm:ss.fffffff");
            var entry = new LogEntry { Message = $"{ts} {message}", Color = (color ?? WpfBrushes.Black).ToString(), Level = level };
            Logs.Insert(0, entry);
            LogAdded?.Invoke(this, entry);
            if (checkReference)
            {
                HandleReference(message, color ?? WpfBrushes.Black, level);
            }
        }

        /// <summary>
        /// Records a single execution duration for this service and updates the running average.
        /// </summary>
        /// <param name="duration">The execution duration to record.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="duration"/> is negative.</exception>
        public void RecordExecutionTime(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
                throw new ArgumentException("Duration must be non-negative", nameof(duration));

            _totalExecutionTimeMs += duration.TotalMilliseconds;
            _executionCount++;
            LastExecutionDuration = duration;
            OnPropertyChanged(nameof(AverageExecutionTimeMs));
        }

        private void HandleReference(string message, WpfBrush color, LogLevel level)
        {
            var m = Regex.Match(message, @"^([^.]+)\.([^.]+)\.(.+)$");
            if (m.Success && ResolveService != null)
            {
                var typeStr = m.Groups[1].Value;
                var name = m.Groups[2].Value;
                var msg = m.Groups[3].Value;
                if (ServiceTypeExtensions.TryParse(typeStr, out var type))
                {
                    var target = ResolveService(type, name);
                    if (target != null && target != this)
                    {
                        if (!AssociatedServices.Contains(target.DisplayName))
                            AssociatedServices.Add(target.DisplayName);
                        if (!target.AssociatedServices.Contains(DisplayName))
                            target.AssociatedServices.Add(DisplayName);
                        target.AddLog(msg, color, level, false);
                    }
                }
            }
        }

        public void SetColorsByType()
        {
            (BackgroundColor, BorderColor) = Type switch
            {
                ServiceType.Tcp => (WpfBrushes.LightBlue, WpfBrushes.DarkBlue),
                ServiceType.Http => (WpfBrushes.LightGreen, WpfBrushes.DarkGreen),
                ServiceType.FileObserver => (WpfBrushes.LightSalmon, WpfBrushes.DarkSalmon),
                ServiceType.Hid => (WpfBrushes.LightYellow, WpfBrushes.Goldenrod),
                ServiceType.Heartbeat => (WpfBrushes.LightPink, WpfBrushes.DeepPink),
                ServiceType.Scp => (WpfBrushes.LightCyan, WpfBrushes.CadetBlue),
                ServiceType.Mqtt => (WpfBrushes.LightGoldenrodYellow, WpfBrushes.Goldenrod),
                ServiceType.Ftp => (WpfBrushes.LightSteelBlue, WpfBrushes.SteelBlue),
                ServiceType.Csv => (WpfBrushes.LightGray, WpfBrushes.Gray),
                _ => (WpfBrushes.LightGray, WpfBrushes.Gray)
            };
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }
    }
}

