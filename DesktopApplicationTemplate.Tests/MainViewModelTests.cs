using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using System.IO;
using System.Linq;
using DesktopApplicationTemplate.Models;
using Xunit;
using MQTTnet.Client;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace DesktopApplicationTemplate.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void GenerateServiceName_IncrementsBasedOnExisting()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            vm.Services.Add(new ServiceViewModel
            {
                DisplayName = "HTTP - HTTP1",
                ServiceType = "HTTP",
                IsActive = false,
                Order = 0
            });
            vm.Services.Add(new ServiceViewModel
            {
                DisplayName = "HTTP - HTTP3",
                ServiceType = "HTTP",
                IsActive = false,
                Order = 1
            });

            string next = vm.GenerateServiceName("HTTP");

            Assert.Equal("HTTP4", next);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void RemoveServiceCommand_LogsLifecycle()
        {
            var logger = new Mock<ILoggingService>();
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);

            var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
            Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
            var vm = new MainViewModel(csv, networkVm, network.Object, logger.Object, servicesPath);
            var service = new ServiceViewModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP" };
            vm.Services.Add(service);
            vm.SelectedService = service;

            vm.RemoveServiceCommand.Execute(null);

            if (File.Exists(servicesPath))
                File.Delete(servicesPath);

            logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("Removing service")), It.IsAny<LogLevel>()), Times.Once);
            logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("Service removed")), It.IsAny<LogLevel>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void ClearLogs_RemovesEntries()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            var svc = new ServiceViewModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP" };
            svc.Logs.Add(new LogEntry { Message = "test" });
            vm.Services.Add(svc);
            vm.SelectedService = svc;

            vm.ClearLogs();

            Assert.Empty(svc.Logs);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void ExportDisplayedLogs_WritesFile()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            var svc = new ServiceViewModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP" };
            svc.Logs.Add(new LogEntry { Message = "first" });
            vm.Services.Add(svc);
            vm.SelectedService = svc;

            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            vm.ExportDisplayedLogs(path);
            var lines = File.ReadAllLines(path);

            Assert.Single(lines);
            File.Delete(path);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void RefreshLogs_RaisesPropertyChanged()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            bool raised = false;
            vm.PropertyChanged += (s, e) => { if (e.PropertyName == "DisplayLogs") raised = true; };

            vm.RefreshLogs();

            Assert.True(raised);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void EditServiceCommand_LoadsCurrentMqttOptions()
        {
            var options = Options.Create(new MqttServiceOptions
            {
                Host = "existing",
                Port = 1883,
                ClientId = "client"
            });
            var client = new Mock<IMqttClient>();
            client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientConnectResult());
            var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());

            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            var svc = new ServiceViewModel { DisplayName = "MQTT - Test", ServiceType = "MQTT" };
            vm.Services.Add(svc);

            MqttEditConnectionViewModel? captured = null;
            vm.EditRequested += _ => captured = new MqttEditConnectionViewModel(service, options);

            vm.EditServiceCommand.Execute(svc);

            Assert.NotNull(captured);
            Assert.Equal("existing", captured!.Host);
            Assert.Equal(1883, captured.Port);
            Assert.Equal("client", captured.ClientId);
            ConsoleTestLogger.LogPass();
        }

        private class TestMainViewModel : MainViewModel
        {
            public TestMainViewModel(CsvService csv, NetworkConfigurationViewModel networkConfig, INetworkConfigurationService networkService)
                : base(csv, networkConfig, networkService)
            {
            }

            public void AddServiceForTest(ServiceViewModel svc)
            {
                Services.Add(svc);
                OnPropertyChanged(nameof(ServicesCreated));
                OnPropertyChanged(nameof(CurrentActiveServices));
            }
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void ServiceCounts_Update_OnAddRemoveActivation()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new TestMainViewModel(csv, networkVm, network.Object);
            int createdChanges = 0;
            int activeChanges = 0;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.ServicesCreated)) createdChanges++;
                if (e.PropertyName == nameof(MainViewModel.CurrentActiveServices)) activeChanges++;
            };
            var svc = new ServiceViewModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP" };
            svc.ActiveChanged += vm.OnServiceActiveChanged;
            vm.AddServiceForTest(svc);

            Assert.Equal(1, vm.ServicesCreated);
            Assert.Equal(0, vm.CurrentActiveServices);

            svc.IsActive = true;

            Assert.Equal(1, vm.CurrentActiveServices);

            vm.SelectedService = svc;
            vm.RemoveServiceCommand.Execute(null);

            Assert.Equal(0, vm.ServicesCreated);
            Assert.Equal(0, vm.CurrentActiveServices);

            Assert.Equal(3, createdChanges);
            Assert.Equal(3, activeChanges);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void AddServiceCommand_RaisesNavigationEvent()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);

            string? requestedName = null;
            vm.AddMqttServiceRequested += name => requestedName = name;

            vm.AddServiceCommand.Execute(null);

            Assert.Equal("MQTT1", requestedName);
            ConsoleTestLogger.LogPass();
        }
    }
}
