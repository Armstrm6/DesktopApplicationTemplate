using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using WpfControls = System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class LoggingServiceTests
    {
        [Theory]
        [TestCategory("WindowsSafe")]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        public void Log_AddsFormattedMessageToRichTextLogger(LogLevel level)
        {
            var uiLogger = new Mock<IRichTextLogger>();
            var service = new LoggingService(uiLogger.Object);

            service.Log("test", level);

            uiLogger.Verify(logger => logger.AppendAsync(
                It.Is<LogEntry>(e =>
                    e.Message.Contains("test") &&
                    e.Message.Contains($"[{level}]") &&
                    Regex.IsMatch(e.Message, "\\[\\d{2}:\\d{2}:\\d{2}\\]")
                ),
                It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public async Task Log_WritesMessageToFile()
        {
            var uiLogger = new Mock<IRichTextLogger>();
            var path = Path.GetTempFileName();
            var service = new LoggingService(uiLogger.Object, path);

            service.Log("file-test", LogLevel.Debug);
            await Task.Delay(50);

            var content = await File.ReadAllTextAsync(path);
            Assert.Contains("file-test", content);

            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void MinimumLevel_Change_FiltersExistingLogs()
        {
            var uiLogger = new Mock<IRichTextLogger>();
            var service = new LoggingService(uiLogger.Object);

            service.Log("debug", LogLevel.Debug);
            service.Log("error", LogLevel.Error);
            uiLogger.Invocations.Clear();

            service.MinimumLevel = LogLevel.Error;

            uiLogger.Verify(logger => logger.SetEntriesAsync(
                It.Is<IEnumerable<LogEntry>>(entries =>
                    entries.Any(e => e.Message.Contains("error")) &&
                    entries.All(e => !e.Message.Contains("debug"))
                ),
                It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
