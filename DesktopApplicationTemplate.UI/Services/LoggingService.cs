using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace DesktopApplicationTemplate.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly RichTextBox _outputBox;
        private readonly Dispatcher _dispatcher;

        public LoggingService(RichTextBox outputBox, Dispatcher dispatcher)
        {
            _outputBox = outputBox;
            _dispatcher = dispatcher;
        }

        public void Log(string message, LogLevel level)
        {
            string formatted = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            _dispatcher.Invoke(() =>
            {
                Brush color = level switch
                {
                    LogLevel.Debug => Brushes.Black,
                    LogLevel.Warning => Brushes.Orange,
                    LogLevel.Error => Brushes.Red,
                    _ => Brushes.Black
                };

                var paragraph = new Paragraph(new Run(formatted) { Foreground = color });
                _outputBox.Document.Blocks.Add(paragraph);
                _outputBox.ScrollToEnd();
            });
        }
    }
}
