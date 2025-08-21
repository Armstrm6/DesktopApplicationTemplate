using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public LoggingService(IRichTextLogger richTextLogger, string logFilePath = "app.log")
        {
            _richTextLogger = richTextLogger;
            _logFilePath = logFilePath;
            Reload();
        }

        public void Log(string message, LogLevel level)
        {
            var entry = new LogEntry
            {
                Message = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}",
                Color = LevelToColor(level),
                Level = level
            };

            _logEntries.Add(entry);

            if (level >= MinimumLevel)
            {
                _ = _richTextLogger.AppendAsync(entry);
            }

            LogAdded?.Invoke(entry);

            _ = WriteToFileAsync(entry.Message + Environment.NewLine);
        }

        private static System.Windows.Media.Brush LevelToColor(LogLevel level) => level switch
        {
            LogLevel.Debug => System.Windows.Media.Brushes.Black,
            LogLevel.Warning => System.Windows.Media.Brushes.Orange,
            LogLevel.Error => System.Windows.Media.Brushes.Red,
            LogLevel.Critical => System.Windows.Media.Brushes.DarkRed,
            _ => System.Windows.Media.Brushes.Black
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

        private Task WriteToFileAsync(string text)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await File.AppendAllTextAsync(_logFilePath, text).ConfigureAwait(false);
                }
                catch
                {
                    // ignore logging errors
                }
            });
        }
    }
}
