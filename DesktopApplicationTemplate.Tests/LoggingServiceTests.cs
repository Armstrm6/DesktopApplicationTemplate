using DesktopApplicationTemplate.UI.Services;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using Xunit;
using System.Text.RegularExpressions;

namespace DesktopApplicationTemplate.Tests
{
    public class LoggingServiceTests
    {
        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        public void Log_AddsFormattedMessageToRichTextBox(LogLevel level)
        {
            var box = new RichTextBox();
            var service = new LoggingService(box, Dispatcher.CurrentDispatcher);

            service.Log("test", level);

            string text = new TextRange(box.Document.ContentStart, box.Document.ContentEnd).Text.Trim();
            Assert.Contains("test", text);
            Assert.Contains($"[{level}]", text);
            Assert.Matches(@"\[\d{2}:\d{2}:\d{2}\]", text);
        }
    }
}
