using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using System.Windows.Threading;

namespace DesktopApplicationTemplate.UI.Services
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
                WpfBrush color = level switch
                {
                    LogLevel.Debug => WpfBrushes.Black,
                    LogLevel.Warning => WpfBrushes.Orange,
                    LogLevel.Error => WpfBrushes.Red,
                    _ => WpfBrushes.Black
                };

                var paragraph = new Paragraph(new Run(formatted) { Foreground = color });
                _outputBox.Document.Blocks.Add(paragraph);
                _outputBox.ScrollToEnd();
            });
        }
    }
}
