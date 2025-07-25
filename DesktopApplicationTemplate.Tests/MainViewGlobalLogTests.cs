using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Xunit;
using System.Threading;

namespace DesktopApplicationTemplate.Tests
{
    public class MainViewGlobalLogTests
    {
        [Fact]
        public void AddGlobalLog_AddsEntry()
        {
            var csv = new CsvService(new CsvViewerViewModel());
            var vm = new MainViewModel(csv);
            vm.AddGlobalLog("test message", LogLevel.Warning);

            Assert.Single(vm.AllLogs);
            Assert.Contains(vm.AllLogs, e => e.Message.Contains("test message") && e.Level == LogLevel.Warning);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void MainView_Constructor_DoesNotThrow()
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
                    var csv = new CsvService(new CsvViewerViewModel());
                    var vm = new MainViewModel(csv);
                    var view = new MainView(vm);
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
