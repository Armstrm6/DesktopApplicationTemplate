using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Models;

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
                _minimumLevel = value;
                Log($"Minimum level changed to {value}", LogLevel.Debug);
                UpdateLogDisplay();
            }
        }

        public event Action<LogEntry>? LogAdded;

        public LoggingService(IRichTextLogger richTextLogger, string logFilePath = "app.log")
        {
            _richTextLogger = richTextLogger;
            _logFilePath = logFilePath;
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
