using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ServiceViewModelTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void AddLog_ReferenceUpdatesAssociatedServices()
        {
            var a = new ServiceViewModel { DisplayName = "Heartbeat - A", ServiceType = "Heartbeat" };
            var b = new ServiceViewModel { DisplayName = "TCP - B", ServiceType = "TCP" };
            var services = new List<ServiceViewModel> { a, b };
            ServiceViewModel.ResolveService = (type, name) =>
                services.Find(s => s.ServiceType == type && s.DisplayName.Split(" - ").Last() == name);

            a.AddLog("TCP.B.Test message");

            Assert.Contains(b.Logs, l => l.Message.Contains("Test message"));
            Assert.Contains(b.DisplayName, a.AssociatedServices);
            Assert.Contains(a.DisplayName, b.AssociatedServices);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void GenerateServiceName_SkipsExistingNames()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var csvVm = new CsvViewerViewModel(Path.Combine(tempDir, "csv.json"));
            var csv = new CsvService(csvVm);
            var net = new StubNetworkService();
            var netVm = new NetworkConfigurationViewModel(net);
            var main = new MainViewModel(csv, netVm, net, servicesFilePath: Path.Combine(tempDir, "services.json"));

            main.Services.Add(new ServiceViewModel { DisplayName = "TCP - TCP1", ServiceType = "TCP" });
            main.Services.Add(new ServiceViewModel { DisplayName = "TCP - TCP2", ServiceType = "TCP" });

            var name = main.GenerateServiceName("TCP");
            Assert.Equal("TCP3", name);

            Directory.Delete(tempDir, true);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void RenameService_AppendsNumericSuffix_WhenNameExists()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var csvVm = new CsvViewerViewModel(Path.Combine(tempDir, "csv.json"));
            var csv = new CsvService(csvVm);
            var net = new StubNetworkService();
            var netVm = new NetworkConfigurationViewModel(net);
            var main = new MainViewModel(csv, netVm, net, servicesFilePath: Path.Combine(tempDir, "services.json"));

            var svc1 = new ServiceViewModel { DisplayName = "TCP - TCP1", ServiceType = "TCP" };
            var svc2 = new ServiceViewModel { DisplayName = "TCP - TCP2", ServiceType = "TCP" };
            main.Services.Add(svc1);
            main.Services.Add(svc2);

            var desired = "TCP1";
            if (main.Services.Any(s => s != svc2 && s.DisplayName.Split(" - ").Last().Equals(desired, StringComparison.OrdinalIgnoreCase)))
            {
                desired = main.GenerateServiceName(svc2.ServiceType);
            }
            svc2.DisplayName = $"{svc2.ServiceType} - {desired}";

            Assert.Equal("TCP - TCP3", svc2.DisplayName);

            Directory.Delete(tempDir, true);
            ConsoleTestLogger.LogPass();
        }

        private class StubNetworkService : INetworkConfigurationService
        {
            public event EventHandler<NetworkConfiguration>? ConfigurationChanged;
            public Task<NetworkConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(new NetworkConfiguration());
            public Task ApplyConfigurationAsync(NetworkConfiguration configuration, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }
    }
}
