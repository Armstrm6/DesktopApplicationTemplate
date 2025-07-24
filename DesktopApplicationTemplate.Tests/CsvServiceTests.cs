using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using System;
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

        [Fact]
        public void AppendRow_CreatesIncrementingFiles()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var vm = new CsvViewerViewModel();
                vm.Configuration.FileNamePattern = Path.Combine(tempDir, "output_{index}.csv");
                var service = new CsvService(vm);

                service.AppendRow(new[] { "a", "b" });
                service.AppendRow(new[] { "c", "d" });

                var file1 = Path.Combine(tempDir, "output_0.csv");
                var file2 = Path.Combine(tempDir, "output_1.csv");
                Assert.True(File.Exists(file1));
                Assert.True(File.Exists(file2));
                Assert.Contains("a,b", File.ReadAllText(file1));
                Assert.Contains("c,d", File.ReadAllText(file2));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }

            ConsoleTestLogger.LogPass();
        }
    }
}
