using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Csv;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ServiceListModelTests
    {
        [Fact]
        public void AddLog_ReferenceUpdatesAssociatedServices()
        {
            var a = TestHelpers.CreateService(ServiceType.Heartbeat, "A");
            var b = TestHelpers.CreateService(ServiceType.Tcp, "B");
            var services = new List<ServiceListModel> { a, b };
            ServiceListModel.ResolveService = (descriptorKey, name) =>
            {
                var key = descriptorKey;
                if (ServiceTypeExtensions.TryParse(descriptorKey, out var parsed))
                {
                    key = parsed.ToDescriptorId();
                }

                return services.Find(s =>
                    string.Equals(s.DescriptorId, key, StringComparison.OrdinalIgnoreCase) &&
                    s.DisplayName.Split(" - ").Last().Equals(name, StringComparison.OrdinalIgnoreCase));
            };

            a.AddLog("TCP.B.Test message");

            Assert.Contains(b.Logs, l => l.Message.Contains("Test message"));
            Assert.Contains(b.DisplayName, a.AssociatedServices);
            Assert.Contains(a.DisplayName, b.AssociatedServices);

            ConsoleTestLogger.LogPass();
        }

        [Theory]
        [InlineData(ServiceType.Tcp, "TCP1")]
        public void GenerateServiceName_SkipsExistingNames(ServiceType type, string baseName)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var csvVm = new CsvViewerViewModel(new StubFileDialogService(), Path.Combine(tempDir, "csv.json"));
            var csv = new CsvService(csvVm);
            var net = new StubNetworkService();
            var netVm = new NetworkConfigurationViewModel(net);
            var main = new MainViewModel(csv, netVm, net, new Dictionary<ServiceType, IEditServiceHandler>(), servicesFilePath: Path.Combine(tempDir, "services.json"));

            main.Services.Add(TestHelpers.CreateService(type, baseName));
            var secondName = baseName[..^1] + "2";
            main.Services.Add(TestHelpers.CreateService(type, secondName));

            var name = main.GenerateServiceName(type);
            var expected = baseName[..^1] + "3";
            Assert.Equal(expected, name);

            Directory.Delete(tempDir, true);
            ConsoleTestLogger.LogPass();
        }

        [Theory]
        [InlineData(ServiceType.Tcp, "TCP1")]
        public void RenameService_AppendsNumericSuffix_WhenNameExists(ServiceType type, string baseName)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var csvVm = new CsvViewerViewModel(new StubFileDialogService(), Path.Combine(tempDir, "csv.json"));
            var csv = new CsvService(csvVm);
            var net = new StubNetworkService();
            var netVm = new NetworkConfigurationViewModel(net);
            var main = new MainViewModel(csv, netVm, net, new Dictionary<ServiceType, IEditServiceHandler>(), servicesFilePath: Path.Combine(tempDir, "services.json"));

            var svc1 = TestHelpers.CreateService(type, baseName);
            var svc2 = TestHelpers.CreateService(type, baseName[..^1] + "2");
            main.Services.Add(svc1);
            main.Services.Add(svc2);

            var desired = baseName;
            if (main.Services.Any(s => s != svc2 && s.DisplayName.Split(" - ").Last().Equals(desired, StringComparison.OrdinalIgnoreCase)))
            {
                desired = main.GenerateServiceName(svc2.Type);
            }
            svc2.DisplayName = $"{svc2.Type.ToLegacyString()} - {desired}";

            var expected = $"{type.ToLegacyString()} - {baseName[..^1] + "3"}";
            Assert.Equal(expected, svc2.DisplayName);

            Directory.Delete(tempDir, true);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void RecordExecutionTime_ComputesAverageAndTracksLastExecution()
        {
            var vm = new ServiceListModel { Type = ServiceType.Tcp };
            vm.RecordExecutionTime(TimeSpan.FromMilliseconds(100));
            vm.RecordExecutionTime(TimeSpan.FromMilliseconds(50));

            Assert.Equal(75, vm.AverageExecutionTimeMs);
            Assert.Equal(TimeSpan.FromMilliseconds(50), vm.LastExecutionDuration);
            Assert.Equal("Last: 50 ms (Avg: 75 ms)", vm.ExecutionTimeText);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void RecordExecutionTime_Throws_When_Negative()
        {
            var vm = new ServiceListModel { Type = ServiceType.Tcp };
            Assert.Throws<ArgumentException>(() => vm.RecordExecutionTime(TimeSpan.FromMilliseconds(-1)));
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void AddLog_UpdatesLastInputMessage()
        {
            var vm = new ServiceListModel { Type = ServiceType.Tcp };
            vm.AddLog("hello world");

            Assert.Equal("hello world", vm.LastInputMessage);
            ConsoleTestLogger.LogPass();
        }

        private class StubNetworkService : INetworkConfigurationService
        {
            public event EventHandler<NetworkConfiguration>? ConfigurationChanged
            {
                add { }
                remove { }
            }
            public Task<NetworkConfiguration> GetConfigurationAsync(CancellationToken ct = default)
                => Task.FromResult(new NetworkConfiguration());
            public Task ApplyConfigurationAsync(NetworkConfiguration configuration, CancellationToken ct = default)
                => Task.CompletedTask;
        }
    }
}
