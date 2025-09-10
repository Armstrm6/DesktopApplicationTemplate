using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class ServiceListModel : ViewModelBase
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
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

        /// <summary>
        /// Gets the average execution time in milliseconds for operations performed by this service.
        /// </summary>
        public double? AverageExecutionTimeMs => _executionCount == 0 ? null : _totalExecutionTimeMs / _executionCount;

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

        public static Func<string, string, ServiceListModel?>? ResolveService { get; set; }

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
                        AddLog("[Service Activated]", WpfBrushes.Green);
                    else
                        AddLog("[Service Deactivated]", WpfBrushes.Red);
                    ActiveChanged?.Invoke(_isActive);
                }
            }
        }

        public ObservableCollection<LogEntry> Logs { get; set; } = new();
        public event Action<bool>? ActiveChanged;

        public event Action<ServiceListModel, LogEntry>? LogAdded;

        public void AddLog(string message, WpfBrush? color = null, LogLevel level = LogLevel.Debug, bool checkReference = true)
        {
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
            OnPropertyChanged(nameof(AverageExecutionTimeMs));
        }

        private void HandleReference(string message, WpfBrush color, LogLevel level)
        {
            var m = Regex.Match(message, @"^([^.]+)\.([^.]+)\.(.+)$");
            if (m.Success && ResolveService != null)
            {
                var type = m.Groups[1].Value;
                var name = m.Groups[2].Value;
                var msg = m.Groups[3].Value;
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

        public void SetColorsByType()
        {
            (BackgroundColor, BorderColor) = ServiceType switch
            {
                "TCP" => (WpfBrushes.LightBlue, WpfBrushes.DarkBlue),
                "HTTP" => (WpfBrushes.LightGreen, WpfBrushes.DarkGreen),
                "File Observer" => (WpfBrushes.LightSalmon, WpfBrushes.DarkSalmon),
                "HID" => (WpfBrushes.LightYellow, WpfBrushes.Goldenrod),
                "Heartbeat" => (WpfBrushes.LightPink, WpfBrushes.DeepPink),
                "SCP" => (WpfBrushes.LightCyan, WpfBrushes.CadetBlue),
                "MQTT" => (WpfBrushes.LightGoldenrodYellow, WpfBrushes.Goldenrod),
                "FTP Server" or "FTP" => (WpfBrushes.LightSteelBlue, WpfBrushes.SteelBlue),
                _ => (WpfBrushes.LightGray, WpfBrushes.Gray)
            };
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }
    }
}

