using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfRichTextBox = System.Windows.Controls.RichTextBox;
using System.Windows.Documents;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using System.Windows.Threading;

namespace DesktopApplicationTemplate.UI.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly WpfRichTextBox _outputBox;
        private readonly Dispatcher _dispatcher;
        private readonly string _logFilePath;

        public LoggingService(WpfRichTextBox outputBox, Dispatcher dispatcher, string logFilePath = "app.log")
        {
            _outputBox = outputBox;
            _dispatcher = dispatcher;
            _logFilePath = logFilePath;
        }

        public void Log(string message, LogLevel level)
        {
            string formatted = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            _dispatcher.Invoke(() =>
            {
                WpfBrush color = level switch
                {
                    LogLevel.Debug => WpfBrushes.Black,
                    LogLevel.Warning => WpfBrushes.Orange,
                    LogLevel.Error => WpfBrushes.Red,
                    LogLevel.Critical => WpfBrushes.DarkRed,
                    _ => WpfBrushes.Black
                };

                var paragraph = new Paragraph(new Run(formatted) { Foreground = color });
                _outputBox.Document.Blocks.Add(paragraph);
                _outputBox.ScrollToEnd();
            });
            try
            {
                System.IO.File.AppendAllText(_logFilePath, formatted + Environment.NewLine);
            }
            catch
            {
                // ignore logging errors
            }
        }
    }
}
