using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services.Protocols.Hid;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfBrushConverter = System.Windows.Media.BrushConverter;

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
        internal const int MaxLogEntries = 200;

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (string.Equals(_displayName, value, StringComparison.Ordinal))
                {
                    return;
                }

                _displayName = value ?? string.Empty;
                OnPropertyChanged();
                RefreshLogMetadata();
            }
        }

        private ServiceType _type;
        public ServiceType Type
        {
            get => _type;
            set
            {
                if (_type == value)
                {
                    return;
                }

                _type = value;
                OnPropertyChanged();
                RefreshLogMetadata();
            }
        }
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

        private string? _iconGlyph;
        public string? IconGlyph
        {
            get => _iconGlyph;
            private set { _iconGlyph = value; OnPropertyChanged(); }
        }

        private string? _descriptorLabel;
        public string? DescriptorLabel
        {
            get => _descriptorLabel;
            private set { _descriptorLabel = value; OnPropertyChanged(); }
        }
        [JsonIgnore] public Page? ServicePage { get; set; }

        public ObservableCollection<string> AssociatedServices { get; } = new();

        private readonly LinkedList<ServiceMessageHistoryEntry> _messageHistory = new();
        private double _totalExecutionTimeMs;
        private int _executionCount;
        private TimeSpan _lastExecutionDuration;
        private string _inputMessage = string.Empty;
        private string _outputMessage = string.Empty;
        private WpfBrush _lastInputBrush = WpfBrushes.Black;
        private int _incomingMessageCount;
        private int _outgoingMessageCount;
        private static readonly WpfBrushConverter BrushConverter = new();
        private const string TimestampFormat = "MM.dd.yyyy - HH:mm:ss.fffffff";
        private const int TimestampLength = 29;

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

        public int IncomingMessageCount
        {
            get => _incomingMessageCount;
            private set
            {
                if (_incomingMessageCount == value)
                {
                    return;
                }

                _incomingMessageCount = value;
                OnPropertyChanged();
            }
        }

        public int OutgoingMessageCount
        {
            get => _outgoingMessageCount;
            private set
            {
                if (_outgoingMessageCount == value)
                {
                    return;
                }

                _outgoingMessageCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Applies persisted message counters to the current instance.
        /// </summary>
        /// <param name="incoming">The number of incoming messages previously recorded.</param>
        /// <param name="outgoing">The number of outgoing messages previously recorded.</param>
        public void InitializeMessageCounts(int incoming, int outgoing)
        {
            IncomingMessageCount = Math.Max(0, incoming);
            OutgoingMessageCount = Math.Max(0, outgoing);
        }

        /// <summary>
        /// Resets the incoming and outgoing message counters.
        /// </summary>
        public void ResetMessageCounts()
        {
            IncomingMessageCount = 0;
            OutgoingMessageCount = 0;
        }

        /// <summary>
        /// Gets the most recent input message received by this service.
        /// </summary>
        public string InputMessage
        {
            get => _inputMessage;
            private set
            {
                if (_inputMessage == value)
                {
                    return;
                }

                _inputMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the most recent outgoing message produced by this service.
        /// </summary>
        public string OutputMessage
        {
            get => _outputMessage;
            private set
            {
                if (_outputMessage == value)
                {
                    return;
                }

                _outputMessage = value;
                OnPropertyChanged();
            }
        }

        public WpfBrush LastInputBrush
        {
            get => _lastInputBrush;
            private set
            {
                if (!Equals(_lastInputBrush, value))
                {
                    _lastInputBrush = value;
                    OnPropertyChanged();
                }
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

        private void RefreshLogMetadata()
        {
            if (Logs is null)
            {
                return;
            }

            foreach (var entry in Logs)
            {
                entry.ServiceType = Type;
                entry.ServiceName = DisplayName;
            }
        }
        public event Action<bool>? ActiveChanged;

        public event Action<ServiceListModel, LogEntry>? LogAdded;

        public void AddLog(string message, WpfBrush? color = null, LogLevel level = LogLevel.Debug, bool checkReference = true)
        {
            var brush = color ?? WpfBrushes.Black;
            var normalizedMessage = NormalizeLatestMessage(message);
            var entryMessage = string.IsNullOrEmpty(normalizedMessage)
                ? $"[{level}]"
                : $"[{level}] {normalizedMessage}";
            var entry = new LogEntry
            {
                Message = entryMessage,
                Color = brush.ToString(),
                Level = level,
                ServiceType = Type,
                ServiceName = DisplayName
            };
            Logs.Insert(0, entry);
            if (Logs.Count > MaxLogEntries)
            {
                Logs.RemoveAt(Logs.Count - 1);
            }
            LogAdded?.Invoke(this, entry);
            if (checkReference)
            {
                UpdateMessageCounters(message);
            }
            if (checkReference)
            {
                HandleReference(message, color ?? WpfBrushes.Black, level);
            }
        }

        public void LoadPersistedLogs(IEnumerable<LogEntry> entries)
        {
            var materialized = entries?.Where(e => !string.IsNullOrWhiteSpace(e.Message)).Take(MaxLogEntries).ToList() ?? new List<LogEntry>();
            Logs = new ObservableCollection<LogEntry>(materialized);
            OnPropertyChanged(nameof(Logs));
            RefreshLogMetadata();
            if (Logs.FirstOrDefault() is { } latest)
            {
                InputMessage = NormalizePersistedMessage(latest.Message);
                LastInputBrush = ParseBrush(latest.Color, WpfBrushes.Black);
            }
        }

        /// <summary>
        /// Rehydrates persisted message history so the service restores its last known exchanges.
        /// </summary>
        /// <param name="entries">The persisted message entries.</param>
        public void LoadMessageHistory(IEnumerable<ServiceMessageHistoryEntry> entries)
        {
            _messageHistory.Clear();
            InputMessage = string.Empty;
            OutputMessage = string.Empty;
            LastInputBrush = WpfBrushes.Black;
            if (entries is null)
            {
                return;
            }

            foreach (var entry in entries
                         .Where(e => e is not null)
                         .OrderByDescending(e => e.Timestamp)
                         .Take(ServiceMessageTableViewModel.MaxRows))
            {
                _messageHistory.AddLast(new ServiceMessageHistoryEntry
                {
                    IncomingMessage = entry.IncomingMessage ?? string.Empty,
                    OutgoingMessage = entry.OutgoingMessage ?? string.Empty,
                    Destination = entry.Destination ?? string.Empty,
                    Timestamp = entry.Timestamp
                });
            }

            foreach (var historyEntry in _messageHistory)
            {
                if (!string.IsNullOrEmpty(historyEntry.IncomingMessage))
                {
                    UpdateInputMessage(historyEntry.IncomingMessage);
                    break;
                }
            }

            foreach (var historyEntry in _messageHistory)
            {
                if (!string.IsNullOrEmpty(historyEntry.OutgoingMessage))
                {
                    UpdateOutputMessage(historyEntry.OutgoingMessage);
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the last input message tracked for this service.
        /// </summary>
        /// <param name="message">The raw message text.</param>
        /// <param name="brush">The brush used to display the message in the UI.</param>
        /// <returns>The normalized message stored on the model.</returns>
        public string UpdateInputMessage(string? message, WpfBrush? brush = null)
        {
            var normalizedMessage = NormalizeLatestMessage(message);
            InputMessage = normalizedMessage;
            LastInputBrush = brush ?? WpfBrushes.Black;
            return normalizedMessage;
        }

        /// <summary>
        /// Updates the last output message tracked for this service.
        /// </summary>
        /// <param name="message">The raw message text.</param>
        /// <returns>The normalized message stored on the model.</returns>
        public string UpdateOutputMessage(string? message)
        {
            var normalizedMessage = NormalizeLatestMessage(message);
            OutputMessage = normalizedMessage;
            return normalizedMessage;
        }

        /// <summary>
        /// Records a message exchange for persistence and downstream bindings.
        /// </summary>
        public void RecordMessageHistory(string? incomingMessage, string? outgoingMessage, string? destination, DateTime timestamp)
        {
            var entry = new ServiceMessageHistoryEntry
            {
                IncomingMessage = incomingMessage ?? string.Empty,
                OutgoingMessage = outgoingMessage ?? string.Empty,
                Destination = destination ?? string.Empty,
                Timestamp = timestamp
            };

            _messageHistory.AddFirst(entry);
            while (_messageHistory.Count > ServiceMessageTableViewModel.MaxRows)
            {
                _messageHistory.RemoveLast();
            }

            if (!string.IsNullOrEmpty(incomingMessage))
            {
                UpdateInputMessage(incomingMessage);
            }

            if (!string.IsNullOrEmpty(outgoingMessage))
            {
                UpdateOutputMessage(outgoingMessage);
            }
        }

        /// <summary>
        /// Creates a snapshot of the current message history for persistence.
        /// </summary>
        public IReadOnlyList<ServiceMessageHistoryEntry> GetMessageHistorySnapshot()
        {
            return _messageHistory
                .Select(entry => new ServiceMessageHistoryEntry
                {
                    IncomingMessage = entry.IncomingMessage,
                    OutgoingMessage = entry.OutgoingMessage,
                    Destination = entry.Destination,
                    Timestamp = entry.Timestamp
                })
                .ToList();
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

        private void UpdateMessageCounters(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (ContainsKeyword(message, "incoming", "received"))
            {
                IncomingMessageCount++;
            }

            if (ContainsKeyword(message, "outgoing", "sent", "sending"))
            {
                OutgoingMessageCount++;
            }
        }

        private static bool ContainsKeyword(string message, params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeLatestMessage(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            var trimmed = StripBracketedTimestamp(message.Trim());
            trimmed = StripLogLevel(trimmed);

            return MessageDisplayFormatter.FormatControlCharacters(trimmed);
        }

        private static string NormalizePersistedMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            var trimmed = StripPersistedTimestamp(message.Trim());
            trimmed = StripBracketedTimestamp(trimmed);
            trimmed = StripLogLevel(trimmed);

            return MessageDisplayFormatter.FormatControlCharacters(trimmed);
        }

        private static string StripPersistedTimestamp(string message)
        {
            if (message.Length > TimestampLength && message[TimestampLength] == ' ')
            {
                var timestampCandidate = message.Substring(0, TimestampLength);
                if (DateTime.TryParseExact(timestampCandidate, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    return message[(TimestampLength + 1)..].TrimStart();
                }
            }

            return message;
        }

        private static string StripBracketedTimestamp(string message)
        {
            if (message.Length == 0 || message[0] != '[')
            {
                return message;
            }

            var endIndex = message.IndexOf(']');
            if (endIndex <= 1)
            {
                return message;
            }

            var candidate = message.Substring(1, endIndex - 1);
            if (TimeSpan.TryParseExact(candidate, "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out _))
            {
                return message[(endIndex + 1)..].TrimStart();
            }

            return message;
        }

        private static string StripLogLevel(string message)
        {
            if (!message.StartsWith("[", StringComparison.Ordinal))
            {
                return message;
            }

            var levelEnd = message.IndexOf(']');
            if (levelEnd <= 0)
            {
                return message;
            }

            var candidate = message.Substring(1, levelEnd - 1);
            if (Enum.TryParse(candidate, out LogLevel _))
            {
                return message[(levelEnd + 1)..].TrimStart();
            }

            return message;
        }

        private static WpfBrush ParseBrush(string? color, WpfBrush fallback)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return fallback;
            }

            try
            {
                if (BrushConverter.ConvertFromString(color) is WpfBrush parsed)
                {
                    return parsed;
                }
            }
            catch (FormatException)
            {
            }
            catch (NotSupportedException)
            {
            }

            return fallback;
        }

        public void ApplyPresentation(ServicePresentationMetadata metadata)
        {
            var normalized = ServicePresentationMetadata.Normalize(metadata);
            BackgroundColor = ParseBrush(normalized.PrimaryAccentColor, WpfBrushes.LightGray);
            BorderColor = ParseBrush(normalized.SecondaryAccentColor, WpfBrushes.Gray);
            IconGlyph = normalized.IconGlyph;
            DescriptorLabel = normalized.DisplayLabel;
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }
    }
}

