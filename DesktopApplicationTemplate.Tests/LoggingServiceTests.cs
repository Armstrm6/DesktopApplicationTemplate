using DesktopApplicationTemplate.UI.Services;
using WpfControls = System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.IO;
using Xunit;
using System.Text.RegularExpressions;
using System;

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
        public void Log_AddsFormattedMessageToRichTextBox(LogLevel level)
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var box = new System.Windows.Controls.RichTextBox();
                    var service = new LoggingService();
                    service.Initialize(box, Dispatcher.CurrentDispatcher);

                    service.Log("test", level);

                    string text = new TextRange(box.Document.ContentStart, box.Document.ContentEnd).Text.Trim();
                    Assert.Contains("test", text);
                    Assert.Contains($"[{level}]", text);
                    Assert.Matches(@"\[\d{2}:\d{2}:\d{2}\]", text);
                }
                catch (Exception e)
                {
                    ex = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void Log_WritesMessageToFile()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            Exception? ex = null;
            var path = Path.GetTempFileName();
            var thread = new Thread(() =>
            {
                try
                {
                    var box = new System.Windows.Controls.RichTextBox();
                    var service = new LoggingService(path);
                    service.Initialize(box, Dispatcher.CurrentDispatcher);
                    service.Log("file-test", LogLevel.Debug);
                    var content = File.ReadAllText(path);
                    Assert.Contains("file-test", content);
                }
                catch (Exception e)
                {
                    ex = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }
            if (ex != null) throw ex;

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void MinimumLevel_Change_FiltersExistingLogs()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var box = new System.Windows.Controls.RichTextBox();
                    var service = new LoggingService();
                    service.Initialize(box, Dispatcher.CurrentDispatcher);
                    service.Log("debug", LogLevel.Debug);
                    service.Log("error", LogLevel.Error);
                    service.MinimumLevel = LogLevel.Error;
                    string text = new TextRange(box.Document.ContentStart, box.Document.ContentEnd).Text;
                    Assert.DoesNotContain("debug", text);
                    Assert.Contains("error", text);
                }
                catch (Exception e)
                {
                    ex = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;

            ConsoleTestLogger.LogPass();
        }
    }
}
