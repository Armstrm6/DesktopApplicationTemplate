using System;
using System.Collections.Generic;
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
        public ServiceType Type { get; set; }
        public string DescriptorId { get; set; } = string.Empty;
        public object? DescriptorPayload { get; set; }
        [JsonIgnore] public Page? Page { get; set; }
        public int Order { get; set; }

        private string _descriptorLabel = string.Empty;
        public string DescriptorLabel
        {
            get => _descriptorLabel;
            private set
            {
                if (_descriptorLabel != value)
                {
                    _descriptorLabel = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DescriptorTooltip));
                }
            }
        }

        private string _descriptorDescription = string.Empty;
        public string DescriptorDescription
        {
            get => _descriptorDescription;
            private set
            {
                if (_descriptorDescription != value)
                {
                    _descriptorDescription = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DescriptorTooltip));
                }
            }
        }

        private string _descriptorCategory = string.Empty;
        public string DescriptorCategory
        {
            get => _descriptorCategory;
            private set
            {
                if (_descriptorCategory != value)
                {
                    _descriptorCategory = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _iconGlyph;
        public string? IconGlyph
        {
            get => _iconGlyph;
            private set
            {
                if (!string.Equals(_iconGlyph, value, StringComparison.Ordinal))
                {
                    _iconGlyph = value;
                    OnPropertyChanged();
                }
            }
        }

        private ServicePresentationMetadata _presentationMetadata = ServicePresentationMetadata.Empty;
        public ServicePresentationMetadata PresentationMetadata
        {
            get => _presentationMetadata;
            private set
            {
                var normalized = ServicePresentationMetadata.Normalize(value);
                if (_presentationMetadata != normalized)
                {
                    _presentationMetadata = normalized;
                    OnPropertyChanged();
                }
            }
        }

        public string DescriptorTooltip => string.IsNullOrWhiteSpace(DescriptorDescription)
            ? DescriptorLabel
            : $"{DescriptorLabel}: {DescriptorDescription}";

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
                var descriptorKey = m.Groups[1].Value;
                var name = m.Groups[2].Value;
                var msg = m.Groups[3].Value;
                var target = ResolveService(descriptorKey, name);
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

        public void ApplyDescriptor(IServiceDescriptor? descriptor, string? nameSuffix = null)
        {
            if (descriptor is not null)
            {
                DescriptorId = descriptor.Id;
                if (descriptor.LegacyType.HasValue)
                {
                    Type = descriptor.LegacyType.Value;
                }
                DescriptorCategory = descriptor.Category ?? string.Empty;
                DescriptorDescription = descriptor.Description ?? string.Empty;
                PresentationMetadata = descriptor.Presentation;
                DescriptorLabel = ResolveDisplayPrefix(descriptor);
                IconGlyph = !string.IsNullOrWhiteSpace(PresentationMetadata.IconGlyph)
                    ? PresentationMetadata.IconGlyph
                    : null;
            }
            else
            {
                DescriptorCategory = string.Empty;
                DescriptorDescription = string.Empty;
                PresentationMetadata = ServicePresentationMetadata.Empty;
                DescriptorLabel = Type.ToLegacyString();
                IconGlyph = null;
            }

            var prefix = DescriptorLabel;

            if (!string.IsNullOrWhiteSpace(nameSuffix))
            {
                DisplayName = $"{prefix} - {nameSuffix}";
            }
            else if (string.IsNullOrWhiteSpace(DisplayName))
            {
                DisplayName = prefix;
            }

            SetColorsByType(PresentationMetadata);
        }

        public void SetColorsByType(ServicePresentationMetadata metadata)
        {
            if (!TryApplyMetadataColors(metadata))
            {
                (BackgroundColor, BorderColor) = LegacyColorMap.TryGetValue(Type, out var brushes)
                    ? brushes
                    : (WpfBrushes.LightGray, WpfBrushes.Gray);
            }

            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }

        public TOptions? GetPayload<TOptions>() where TOptions : class => DescriptorPayload as TOptions;

        public void SetPayload<TOptions>(TOptions? payload) where TOptions : class => DescriptorPayload = payload;

        private static string ResolveDisplayPrefix(IServiceDescriptor descriptor)
        {
            if (!string.IsNullOrWhiteSpace(descriptor.Presentation.DisplayLabel))
            {
                return descriptor.Presentation.DisplayLabel!;
            }

            if (!string.IsNullOrWhiteSpace(descriptor.DisplayName))
            {
                return descriptor.DisplayName;
            }

            return descriptor.LegacyType?.ToLegacyString() ?? descriptor.Id;
        }

        private bool TryApplyMetadataColors(ServicePresentationMetadata metadata)
        {
            if (metadata is null)
            {
                return false;
            }

            var brushConverter = new System.Windows.Media.BrushConverter();
            if (!string.IsNullOrWhiteSpace(metadata.PrimaryAccentColor)
                && !string.IsNullOrWhiteSpace(metadata.SecondaryAccentColor))
            {
                try
                {
                    var background = (WpfBrush?)brushConverter.ConvertFromString(metadata.PrimaryAccentColor!);
                    var border = (WpfBrush?)brushConverter.ConvertFromString(metadata.SecondaryAccentColor!);
                    if (background is not null && border is not null)
                    {
                        BackgroundColor = background;
                        BorderColor = border;
                        return true;
                    }
                }
                catch
                {
                    // Ignore conversion failures and fall back to legacy colors.
                }
            }

            return false;
        }

        private static readonly IReadOnlyDictionary<ServiceType, (WpfBrush Background, WpfBrush Border)> LegacyColorMap =
            new Dictionary<ServiceType, (WpfBrush, WpfBrush)>
            {
                [ServiceType.Tcp] = (WpfBrushes.LightBlue, WpfBrushes.DarkBlue),
                [ServiceType.Http] = (WpfBrushes.LightGreen, WpfBrushes.DarkGreen),
                [ServiceType.FileObserver] = (WpfBrushes.LightSalmon, WpfBrushes.DarkSalmon),
                [ServiceType.Hid] = (WpfBrushes.LightYellow, WpfBrushes.Goldenrod),
                [ServiceType.Heartbeat] = (WpfBrushes.LightPink, WpfBrushes.DeepPink),
                [ServiceType.Scp] = (WpfBrushes.LightCyan, WpfBrushes.CadetBlue),
                [ServiceType.Mqtt] = (WpfBrushes.LightGoldenrodYellow, WpfBrushes.Goldenrod),
                [ServiceType.Ftp] = (WpfBrushes.LightSteelBlue, WpfBrushes.SteelBlue),
                [ServiceType.Csv] = (WpfBrushes.LightGray, WpfBrushes.Gray)
            };
    }
}

