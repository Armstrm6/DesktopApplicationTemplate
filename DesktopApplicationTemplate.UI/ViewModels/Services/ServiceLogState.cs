using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfBrushConverter = System.Windows.Media.BrushConverter;

namespace DesktopApplicationTemplate.UI.ViewModels.Services
{
    public sealed class ServiceLogState : ViewModelBase
    {
        internal const string TimestampFormat = "MM.dd.yyyy - HH:mm:ss.fffffff";
        internal const int TimestampLength = 29;
        internal const string InputMessagePlaceholder = "---------";

        private readonly LinkedList<ServiceMessageHistoryEntry> _messageHistory = new();
        private readonly ServiceMetricsState _metrics;
        private readonly int _maxLogEntries;

        private ObservableCollection<LogEntry> _logs = new();
        private string _inputMessage = InputMessagePlaceholder;
        private string _outputMessage = string.Empty;
        private WpfBrush _lastInputBrush = WpfBrushes.Black;
        private string _lastLogMessage = string.Empty;
        private WpfBrush _lastLogBrush = WpfBrushes.Black;
        private ServiceType _serviceType;
        private string _serviceName = string.Empty;

        private static readonly WpfBrushConverter BrushConverter = new();

        public ServiceLogState(ServiceMetricsState metrics, int maxLogEntries)
        {
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            _maxLogEntries = maxLogEntries;
        }

        public ObservableCollection<LogEntry> Logs
        {
            get => _logs;
            private set
            {
                if (ReferenceEquals(_logs, value))
                {
                    return;
                }

                _logs = value;
                OnPropertyChanged();
                RefreshLogMetadata();
            }
        }

        public string InputMessage
        {
            get => _inputMessage;
            private set
            {
                if (string.Equals(_inputMessage, value, StringComparison.Ordinal))
                {
                    return;
                }

                _inputMessage = value;
                OnPropertyChanged();
            }
        }

        public string OutputMessage
        {
            get => _outputMessage;
            private set
            {
                if (string.Equals(_outputMessage, value, StringComparison.Ordinal))
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
                if (Equals(_lastInputBrush, value))
                {
                    return;
                }

                _lastInputBrush = value;
                OnPropertyChanged();
            }
        }

        public string LastLogMessage
        {
            get => _lastLogMessage;
            private set
            {
                if (string.Equals(_lastLogMessage, value, StringComparison.Ordinal))
                {
                    return;
                }

                _lastLogMessage = value;
                OnPropertyChanged();
            }
        }

        public WpfBrush LastLogBrush
        {
            get => _lastLogBrush;
            private set
            {
                if (Equals(_lastLogBrush, value))
                {
                    return;
                }

                _lastLogBrush = value;
                OnPropertyChanged();
            }
        }

        public bool HasMessageHistory => _messageHistory.Count > 0;

        public event Action<LogEntry>? LogAdded;

        public void UpdateIdentity(ServiceType type, string? displayName)
        {
            var normalizedName = displayName ?? string.Empty;
            var changed = _serviceType != type || !string.Equals(_serviceName, normalizedName, StringComparison.Ordinal);
            if (!changed)
            {
                return;
            }

            _serviceType = type;
            _serviceName = normalizedName;
            RefreshLogMetadata();
        }

        public LogEntry AddLog(string? message, WpfBrush brush, LogLevel level)
        {
            var normalizedMessage = NormalizeLatestMessage(message);
            var entryMessage = string.IsNullOrEmpty(normalizedMessage)
                ? $"[{level}]"
                : $"[{level}] {normalizedMessage}";
            var entry = new LogEntry
            {
                Message = entryMessage,
                Color = brush.ToString(),
                Level = level,
                ServiceType = _serviceType,
                ServiceName = _serviceName
            };

            LastLogMessage = entryMessage;
            LastLogBrush = brush;
            Logs.Insert(0, entry);
            while (Logs.Count > _maxLogEntries)
            {
                Logs.RemoveAt(Logs.Count - 1);
            }

            LogAdded?.Invoke(entry);
            return entry;
        }

        public void LoadPersistedLogs(IEnumerable<LogEntry> entries)
        {
            var materialized = entries?.Where(e => !string.IsNullOrWhiteSpace(e.Message)).Take(_maxLogEntries).ToList()
                ?? new List<LogEntry>();
            Logs = new ObservableCollection<LogEntry>(materialized);
            LastLogMessage = string.Empty;
            LastLogBrush = WpfBrushes.Black;

            if (Logs.FirstOrDefault() is { } latest)
            {
                var normalized = NormalizePersistedMessage(latest.Message ?? string.Empty);
                LastLogMessage = string.IsNullOrEmpty(normalized)
                    ? $"[{latest.Level}]"
                    : $"[{latest.Level}] {normalized}";
                LastLogBrush = ParseBrush(latest.Color, WpfBrushes.Black);
            }
        }

        public void LoadMessageHistory(IEnumerable<ServiceMessageHistoryEntry> entries)
        {
            _messageHistory.Clear();
            _metrics.ResetMessageCounts();
            InputMessage = InputMessagePlaceholder;
            OutputMessage = string.Empty;
            LastInputBrush = WpfBrushes.Black;

            if (entries is null)
            {
                return;
            }

            var incomingCount = 0;
            var outgoingCount = 0;

            foreach (var entry in entries
                         .Where(e => e is not null)
                         .OrderByDescending(e => e.Timestamp)
                         .Take(DesktopApplicationTemplate.UI.ViewModels.ServiceMessageTableViewModel.MaxRows))
            {
                var incoming = entry.IncomingMessage ?? string.Empty;
                var outgoing = entry.OutgoingMessage ?? string.Empty;
                if (!string.IsNullOrEmpty(incoming))
                {
                    incomingCount++;
                }

                if (!string.IsNullOrEmpty(outgoing))
                {
                    outgoingCount++;
                }

                _messageHistory.AddLast(new ServiceMessageHistoryEntry
                {
                    IncomingMessage = incoming,
                    OutgoingMessage = outgoing,
                    Destination = entry.Destination ?? string.Empty,
                    Timestamp = entry.Timestamp
                });
            }

            _metrics.SetMessageCounts(incomingCount, outgoingCount);

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

        public string UpdateInputMessage(string? message, WpfBrush? brush = null)
        {
            var normalizedMessage = NormalizeLatestMessage(message, InputMessagePlaceholder);
            InputMessage = normalizedMessage;
            LastInputBrush = brush ?? WpfBrushes.Black;
            return normalizedMessage;
        }

        public string UpdateOutputMessage(string? message)
        {
            var normalizedMessage = NormalizeLatestMessage(message);
            OutputMessage = normalizedMessage;
            return normalizedMessage;
        }

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
            while (_messageHistory.Count > DesktopApplicationTemplate.UI.ViewModels.ServiceMessageTableViewModel.MaxRows)
            {
                _messageHistory.RemoveLast();
            }

            if (!string.IsNullOrEmpty(incomingMessage))
            {
                _metrics.IncrementIncomingMessages();
                UpdateInputMessage(incomingMessage);
            }

            if (!string.IsNullOrEmpty(outgoingMessage))
            {
                _metrics.IncrementOutgoingMessages();
                UpdateOutputMessage(outgoingMessage);
            }
        }

        public void ResetLatestMessages()
        {
            InputMessage = InputMessagePlaceholder;
            OutputMessage = string.Empty;
            LastInputBrush = WpfBrushes.Black;
        }

        private void RefreshLogMetadata()
        {
            foreach (var entry in Logs)
            {
                entry.ServiceType = _serviceType;
                entry.ServiceName = _serviceName;
            }
        }

        internal static WpfBrush ParseBrush(string? color, WpfBrush fallback)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return fallback;
            }

            try
            {
                if (BrushConverter.ConvertFromString(color) is WpfBrush parsed)
                {
                    if (parsed is System.Windows.Freezable freezable && !freezable.IsFrozen && freezable.CanFreeze)
                    {
                        freezable.Freeze();
                    }
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

        private static string NormalizeLatestMessage(string? message, string? emptyPlaceholder = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return emptyPlaceholder ?? string.Empty;
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
    }
}
