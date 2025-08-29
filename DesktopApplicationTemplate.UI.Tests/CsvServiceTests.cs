using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Xunit;
using Moq;

namespace DesktopApplicationTemplate.Tests
{
    public class CsvServiceTests
    {
        [WindowsFact]
        public void EnsureColumnsForService_AddsColumns()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()+".json");
            var vm = new CsvViewerViewModel(new StubFileDialogService(), configPath);
            var svc = new CsvService(vm);

            svc.EnsureColumnsForService("TestSvc");

            Assert.Contains(vm.Configuration.Columns, c => c.Name == "TestSvc");
            Assert.Contains(vm.Configuration.Columns, c => c.Name == "TestSvc Sent");
            ConsoleTestLogger.LogPass();
        }

        [WindowsFact]
        public void EnsureColumnsForService_SkipsCsvServices()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()+".json");
            var vm = new CsvViewerViewModel(new StubFileDialogService(), configPath);
            var svc = new CsvService(vm);

            svc.EnsureColumnsForService("CSV Creator");

            Assert.Empty(vm.Configuration.Columns);
            ConsoleTestLogger.LogPass();
        }

        [WindowsFact]
        public void RemoveColumnsForService_RemovesColumns()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()+".json");
            var vm = new CsvViewerViewModel(new StubFileDialogService(), configPath);
            var svc = new CsvService(vm);

            svc.EnsureColumnsForService("Svc");
            svc.RemoveColumnsForService("Svc");

            Assert.DoesNotContain(vm.Configuration.Columns, c => c.Name == "Svc");
            Assert.DoesNotContain(vm.Configuration.Columns, c => c.Name == "Svc Sent");

            ConsoleTestLogger.LogPass();
        }

        [WindowsFact]
        public void RemoveColumnsForService_StartsNewFile()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);
            try
            {
                var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()+".json");
                var vm = new CsvViewerViewModel(new StubFileDialogService(), configPath);
                vm.Configuration.FileNamePattern = Path.Combine(dir, "out_{index}.csv");
                var svc = new CsvService(vm);

                svc.RecordLog("Svc", "one");
                var existing = Directory.GetFiles(dir);

                svc.RemoveColumnsForService("Svc");
                svc.RecordLog("Svc2", "two");

                var files = Directory.GetFiles(dir);
                Assert.True(existing.All(f => File.Exists(f)));
                Assert.True(files.Length > existing.Length);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
            ConsoleTestLogger.LogPass();
        }

        [WindowsFact]
        public void RecordLog_WritesCsvRow()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()+".json");
            var vm = new CsvViewerViewModel(new StubFileDialogService(), configPath);
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()+".csv");
            vm.Configuration.FileNamePattern = path;
            var svc = new CsvService(vm);

            svc.RecordLog("Svc", "hello world");

            var lines = File.ReadAllLines(path);
            Assert.Equal("Svc,Svc Sent", lines[0]);
            Assert.Equal("hello world,", lines[1]);
            ConsoleTestLogger.LogPass();
        }

        [WindowsFact]
        public void AppendRow_CreatesIncrementingFiles()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()+".json");
                var vm = new CsvViewerViewModel(new StubFileDialogService(), configPath);

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

        [WindowsFact]
        public void AppendRow_CreatesNestedDirectory()
        {
            var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(baseDir);
            try
            {
                var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()+".json");
                var vm = new CsvViewerViewModel(new StubFileDialogService(), configPath);
                vm.Configuration.OutputDirectory = baseDir;
                vm.Configuration.FileNamePattern = Path.Combine("TCP1.message", "out_{index}.csv");
                var service = new CsvService(vm);

                service.AppendRow(new[] { "x", "y" });

                var expectedDir = Path.Combine(baseDir, "TCP1.message");
                Assert.True(Directory.Exists(expectedDir));
                var files = Directory.GetFiles(expectedDir, "out_0.csv");
                Assert.Single(files);
            }
            finally
            {
                Directory.Delete(baseDir, true);
            }

            ConsoleTestLogger.LogPass();
        }

        [WindowsFact]
        public void AppendRow_WithoutIndexPlaceholder_UsesSingleFile()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()+".json");
                var vm = new CsvViewerViewModel(new StubFileDialogService(), configPath);

                var filePath = Path.Combine(tempDir, "output.csv");
                vm.Configuration.FileNamePattern = filePath;
                var service = new CsvService(vm);

                service.AppendRow(new[] { "x", "y" });
                service.AppendRow(new[] { "z", "w" });

                Assert.True(File.Exists(filePath));
                var files = Directory.GetFiles(tempDir, "*.csv");
                Assert.Single(files);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }

            ConsoleTestLogger.LogPass();
        }

        [WindowsFact]
        public void CsvServiceView_LoadsInto_ContentFrame()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
                var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                var network = new Mock<INetworkConfigurationService>();
                var networkVm = new NetworkConfigurationViewModel(network.Object);
                var mainVm = new MainViewModel(new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath)), networkVm, network.Object, null, servicesPath);
                var view = new MainView(mainVm);

                var svc = new ServiceViewModel { DisplayName = "CSV Creator - Test", ServiceType = "CSV Creator" };
                svc.SetColorsByType();

                var method = typeof(MainView).GetMethod("OpenServiceEditor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(view, new object[] { svc });

                Assert.IsType<CsvServiceView>(view.ContentFrame.Content);
                ConsoleTestLogger.LogPass();
            });
        }
    }
}
