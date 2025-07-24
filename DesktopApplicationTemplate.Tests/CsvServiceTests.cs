using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using System.IO;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class CsvServiceTests
    {
        [Fact]
        public void EnsureColumnsForService_AddsColumns()
        {
            var vm = new CsvViewerViewModel();
            var svc = new CsvService(vm);

            svc.EnsureColumnsForService("TestSvc");

            Assert.Contains(vm.Configuration.Columns, c => c.Name == "TestSvc");
            Assert.Contains(vm.Configuration.Columns, c => c.Name == "TestSvc Sent");
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void RecordLog_WritesCsvRow()
        {
            var vm = new CsvViewerViewModel();
            string path = Path.GetTempFileName();
            vm.Configuration.FileNamePattern = path;
            var svc = new CsvService(vm);

            svc.RecordLog("Svc", "hello world");

            var lines = File.ReadAllLines(path);
            Assert.Equal("Svc,Svc Sent", lines[0]);
            Assert.Equal("hello world,", lines[1]);
            ConsoleTestLogger.LogPass();
        }
    }
}
