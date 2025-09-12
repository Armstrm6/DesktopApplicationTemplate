using DesktopApplicationTemplate.Persistence;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt;
using DesktopApplicationTemplate.UI.ViewModels.Csv;
using DesktopApplicationTemplate.UI.Helpers;
using Moq;
using System.IO;
using System.Linq;
using DesktopApplicationTemplate.Models;
using Xunit;
using MQTTnet.Client;
using Microsoft.Extensions.Options;
using MQTTnet;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public void GenerateServiceName_IncrementsBasedOnExisting()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            vm.Services.Add(TestHelpers.CreateService(ServiceType.Http, "HTTP1")
            {
                IsActive = false,
                Order = 0
            });
            vm.Services.Add(TestHelpers.CreateService(ServiceType.Http, "HTTP3")
            {
                IsActive = false,
                Order = 1
            });

            string next = vm.GenerateServiceName(ServiceType.Http);

            Assert.Equal("HTTP4", next);
            ConsoleTestLogger.LogPass();
        }

        [Theory]
        [InlineData(ServiceType.Tcp, "TCP1")]
        public async Task RemoveServiceCommand_LogsLifecycle(ServiceType type, string name)
        {
            var logger = new Mock<ILoggingService>();
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);

            var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
            Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
            var oldPath = ServicePersistence.FilePath;
            try
            {
                var vm = new MainViewModel(csv, networkVm, network.Object, logger.Object, servicesPath);
                var service = TestHelpers.CreateService(type, name);
                vm.Services.Add(service);
                vm.SelectedService = service;

                await ((AsyncRelayCommand)vm.RemoveServiceCommand).ExecuteAsync(null);

                logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("Removing service")), It.IsAny<LogLevel>()), Times.Once);
                logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("Service removed")), It.IsAny<LogLevel>()), Times.Once);
            }
            finally
            {
                ServicePersistence.FilePath = oldPath;
                var dir = Path.GetDirectoryName(servicesPath)!;
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }

            ConsoleTestLogger.LogPass();
        }

        [Theory]
        [InlineData(ServiceType.Tcp, "TCP1")]
        public void ClearLogs_RemovesEntries(ServiceType type, string name)
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            var svc = TestHelpers.CreateService(type, name);
            svc.Logs.Add(new LogEntry { Message = "test" });
            vm.Services.Add(svc);
            vm.SelectedService = svc;

            vm.ClearLogs();

            Assert.Empty(svc.Logs);
            ConsoleTestLogger.LogPass();
        }

        [Theory]
        [InlineData(ServiceType.Tcp, "TCP1")]
        public void ExportDisplayedLogs_WritesFile(ServiceType type, string name)
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            var svc = TestHelpers.CreateService(type, name);
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

        [Theory]
        [InlineData(ServiceType.Tcp, "TCP1")]
        public void RefreshLogs_RaisesPropertyChanged(ServiceType type, string name)
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            bool raised = false;
            vm.PropertyChanged += (s, e) => { if (e.PropertyName == "DisplayLogs") raised = true; };
            var svc = TestHelpers.CreateService(type, name);
            vm.Services.Add(svc);
            vm.SelectedService = svc;

            vm.RefreshLogs();

            Assert.True(raised);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
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
            var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            var svc = TestHelpers.CreateService(ServiceType.Mqtt, "Test");
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

            public void AddServiceForTest(ServiceListModel svc)
            {
                Services.Add(svc);
                OnPropertyChanged(nameof(ServicesCreated));
                OnPropertyChanged(nameof(CurrentActiveServices));
            }
        }

        [Theory]
        [InlineData(ServiceType.Tcp, "TCP1")]
        public async Task ServiceCounts_Update_OnAddRemoveActivation(ServiceType type, string name)
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var oldPath = ServicePersistence.FilePath;
            ServicePersistence.FilePath = Path.Combine(tempDir, "services.json");
            try
            {
                var vm = new TestMainViewModel(csv, networkVm, network.Object);
                int createdChanges = 0;
                int activeChanges = 0;
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.ServicesCreated)) createdChanges++;
                    if (e.PropertyName == nameof(MainViewModel.CurrentActiveServices)) activeChanges++;
                };
                var svc = TestHelpers.CreateService(type, name);
                svc.ActiveChanged += vm.OnServiceActiveChanged;
                vm.AddServiceForTest(svc);

                Assert.Equal(1, vm.ServicesCreated);
                Assert.Equal(0, vm.CurrentActiveServices);

                svc.IsActive = true;

                Assert.Equal(1, vm.CurrentActiveServices);

                vm.SelectedService = svc;
                await ((AsyncRelayCommand)vm.RemoveServiceCommand).ExecuteAsync(null);

                Assert.Equal(0, vm.ServicesCreated);
                Assert.Equal(0, vm.CurrentActiveServices);

                Assert.Equal(3, createdChanges);
                Assert.Equal(3, activeChanges);
            }
            finally
            {
                ServicePersistence.FilePath = oldPath;
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void AddServiceCommand_RaisesAddServiceRequested()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);

            bool raised = false;
            vm.AddServiceRequested += () => raised = true;

            vm.AddServiceCommand.Execute(null);

            Assert.True(raised);
            ConsoleTestLogger.LogPass();
        }
    }
}
