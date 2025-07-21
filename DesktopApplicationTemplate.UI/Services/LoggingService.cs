using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DesktopApplicationTemplate.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly TextBox _outputBox;
        private readonly Dispatcher _dispatcher;

        public LoggingService(TextBox outputBox, Dispatcher dispatcher)
        {
            _outputBox = outputBox;
            _dispatcher = dispatcher;
        }

        public void Log(string message, LogLevel level)
        {
            string formatted = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            _dispatcher.Invoke(() =>
            {
                _outputBox.AppendText(formatted + Environment.NewLine);
                _outputBox.ScrollToEnd();
            });
        }
    }
}
