using System;
using System.Collections.Generic;
using System.Linq;
using WpfRichTextBox = System.Windows.Controls.RichTextBox;
using System.Windows.Documents;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using System.Windows.Threading;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly WpfRichTextBox _outputRichTextBox;
        private readonly Dispatcher _dispatcher;
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

        public LoggingService(WpfRichTextBox outputRichTextBox, Dispatcher dispatcher, string logFilePath = "app.log")
        {
            _outputRichTextBox = outputRichTextBox;
            _dispatcher = dispatcher;
            _logFilePath = logFilePath;
        }

        public void Log(string message, LogLevel level)
        {
            var entry = new LogEntry { Message = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}",
                                        Color = LevelToColor(level),
                                        Level = level };

            _logEntries.Add(entry);

            if (level >= MinimumLevel)
            {
                _dispatcher.Invoke(() =>
                {
                    var paragraph = new Paragraph(new Run(entry.Message) { Foreground = entry.Color });
                    _outputRichTextBox.Document.Blocks.Add(paragraph);
                    _outputRichTextBox.ScrollToEnd();
                });
            }

            LogAdded?.Invoke(entry);

            try
            {
                System.IO.File.AppendAllText(_logFilePath, entry.Message + Environment.NewLine);
            }
            catch
            {
                // ignore logging errors
            }

        }

        private static WpfBrush LevelToColor(LogLevel level) => level switch
        {
            LogLevel.Debug => WpfBrushes.Black,
            LogLevel.Warning => WpfBrushes.Orange,
            LogLevel.Error => WpfBrushes.Red,
            LogLevel.Critical => WpfBrushes.DarkRed,
            _ => WpfBrushes.Black
        };

        private void UpdateLogDisplay()
        {
            _dispatcher.Invoke(() =>
            {
                _outputRichTextBox.Document.Blocks.Clear();
                foreach (var e in _logEntries.Where(e => e.Level >= MinimumLevel))
                {
                    var paragraph = new Paragraph(new Run(e.Message) { Foreground = e.Color });
                    _outputRichTextBox.Document.Blocks.Add(paragraph);
                }
                _outputRichTextBox.ScrollToEnd();
            });
        }
    }
}
