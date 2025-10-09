using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly IRichTextLogger _richTextLogger;
        private readonly string _logFilePath;
        private readonly List<LogEntry> _logEntries = new();
        private readonly SemaphoreSlim _fileWriteLock = new(1, 1);

        private LogLevel _minimumLevel = LogLevel.Debug;
        public LogLevel MinimumLevel
        {
            get => _minimumLevel;
            set
            {
                if (_minimumLevel == value) return;
                _minimumLevel = value;
                UpdateLogDisplay();
            }
        }

        public event Action<LogEntry>? LogAdded;

        public LoggingService(IRichTextLogger richTextLogger, string? logFilePath = null)
        {
            _richTextLogger = richTextLogger;

            var resolvedLogFilePath = logFilePath ?? GetDefaultLogFilePath();
            EnsureDirectoryExists(resolvedLogFilePath);

            _logFilePath = resolvedLogFilePath;
            Reload();
        }

        private static string GetDefaultLogFilePath()
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DesktopApplicationTemplate");

            return Path.Combine(logDirectory, "app.log");
        }

        private static void EnsureDirectoryExists(string resolvedLogFilePath)
        {
            var directoryPath = Path.GetDirectoryName(resolvedLogFilePath);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public void Log(string message, LogLevel level)
        {
            var now = DateTime.Now;
            var normalizedMessage = message ?? string.Empty;
            var hasServiceContext = TryExtractServiceContext(
                normalizedMessage,
                out var contextType,
                out var contextName,
                out var contextMessage);

            if (hasServiceContext)
            {
                normalizedMessage = contextMessage;
            }

            var contextPrefix = hasServiceContext
                ? $"[{contextType}.{contextName}] "
                : string.Empty;

            var entry = new LogEntry
            {
                Message = $"[{now:HH:mm:ss}] [{level}] {contextPrefix}{normalizedMessage}",
                Color = LevelToColor(level),
                Level = level,
                ServiceType = hasServiceContext ? contextType : null,
                ServiceName = hasServiceContext ? contextName : string.Empty
            };

            _logEntries.Add(entry);

            if (level >= MinimumLevel)
            {
                _ = _richTextLogger.AppendAsync(entry);
            }

            LogAdded?.Invoke(entry);

            _ = AppendToLogFileAsync(entry.Message + Environment.NewLine);
        }

        private static bool TryExtractServiceContext(
            string message,
            out ServiceType serviceType,
            out string serviceName,
            out string content)
        {
            serviceType = default;
            serviceName = string.Empty;
            content = message ?? string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            var firstDot = message.IndexOf('.');
            if (firstDot <= 0)
            {
                return false;
            }

            var secondDot = message.IndexOf('.', firstDot + 1);
            if (secondDot <= firstDot + 1)
            {
                return false;
            }

            var typeSegment = message[..firstDot];
            if (!Enum.TryParse(typeSegment, ignoreCase: true, out serviceType))
            {
                return false;
            }

            var nameSegment = message[(firstDot + 1)..secondDot].Trim();
            if (string.IsNullOrWhiteSpace(nameSegment))
            {
                return false;
            }

            serviceName = nameSegment;
            content = message[(secondDot + 1)..].TrimStart();
            return true;
        }

        private static string LevelToColor(LogLevel level) => level switch
        {
            LogLevel.Debug => "#000000",
            LogLevel.Information => "#0000FF",
            LogLevel.Warning => "#FFA500",
            LogLevel.Error => "#FF0000",
            LogLevel.Critical => "#8B0000",
            _ => "#000000"
        };

        public void Reload()
        {
            _logEntries.Clear();
            try
            {
                if (File.Exists(_logFilePath))
                {
                    foreach (var line in File.ReadLines(_logFilePath))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        var entry = ParseLine(line);
                        _logEntries.Add(entry);
                    }
                }
            }
            catch
            {
                // ignore loading errors
            }

            UpdateLogDisplay();
            foreach (var entry in _logEntries.Where(e => e.Level >= MinimumLevel))
            {
                LogAdded?.Invoke(entry);
            }
        }

        private static LogEntry ParseLine(string line)
        {
            var level = LogLevel.Debug;
            try
            {
                var firstClose = line.IndexOf(']');
                var secondOpen = line.IndexOf('[', firstClose + 1);
                var secondClose = line.IndexOf(']', secondOpen + 1);
                if (secondOpen >= 0 && secondClose > secondOpen)
                {
                    var levelText = line.Substring(secondOpen + 1, secondClose - secondOpen - 1);
                    Enum.TryParse(levelText, out level);
                }
            }
            catch
            {
                // ignore parsing errors
            }

            return new LogEntry
            {
                Message = line,
                Level = level,
                Color = LevelToColor(level)
            };
        }

        private void UpdateLogDisplay()
        {
            _ = _richTextLogger.SetEntriesAsync(_logEntries.Where(e => e.Level >= MinimumLevel));
        }

        private async Task AppendToLogFileAsync(string text)
        {
            try
            {
                await _fileWriteLock.WaitAsync().ConfigureAwait(false);
                await File.AppendAllTextAsync(_logFilePath, text).ConfigureAwait(false);
            }
            catch
            {
                // ignore logging errors
            }
            finally
            {
                _fileWriteLock.Release();
            }
        }
    }
}
