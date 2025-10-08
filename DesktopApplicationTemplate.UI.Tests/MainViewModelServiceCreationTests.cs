using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Csv;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.UI.Tests
{
    public class MainViewModelServiceCreationTests : IDisposable
    {
        private readonly TestLoggingService logger = new();
        private readonly MainViewModelTestEnvironment environment;

        public MainViewModelServiceCreationTests()
        {
            environment = new MainViewModelTestEnvironment(logger);
        }

        public void Dispose()
        {
            environment.Dispose();
        }

        [Fact]
        public async Task StartServicesAsync_DoesNotActivateServicesDuringCreationAsync()
        {
            var viewModel = environment.ViewModel;
            var service = CreateService("CreationGuard");
            viewModel.Services.Add(service);

            using (viewModel.BeginServiceCreationScope())
            {
                await viewModel.StartServicesForTestingAsync();
            }

            service.IsActive.Should().BeFalse();
            service.Logs.Should().BeEmpty();
            viewModel.ActivatingServicesCount.Should().Be(0);
            logger.Messages.Should().Contain(message => message.Contains("Skipping service activation while creation is in progress."));
        }

        [Fact]
        public async Task StopServicesAsync_DoesNotDeactivateServicesDuringCreationAsync()
        {
            var viewModel = environment.ViewModel;
            var service = CreateService("ActiveService");
            service.InitializeActivationState(true);
            viewModel.Services.Add(service);

            using (viewModel.BeginServiceCreationScope())
            {
                await viewModel.StopServicesForTestingAsync();
            }

            service.IsActive.Should().BeTrue();
            viewModel.ActivatingServicesCount.Should().Be(0);
            logger.Messages.Should().Contain(message => message.Contains("Skipping service deactivation while creation is in progress."));
        }

        [Fact]
        public async Task StartServicesAsync_ActivatesAfterCreationScopeDisposesAsync()
        {
            var viewModel = environment.ViewModel;
            var service = CreateService("DeferredStart");
            viewModel.Services.Add(service);

            using (viewModel.BeginServiceCreationScope())
            {
                // Intentionally empty to exit scope immediately.
            }

            await viewModel.StartServicesForTestingAsync();

            service.IsActive.Should().BeTrue();
            viewModel.ActivatingServicesCount.Should().BeGreaterThan(0);
            logger.Messages.Should().NotContain(message => message.Contains("Skipping service activation while creation is in progress."));
        }

        [Fact]
        public void InitializeActivationState_DoesNotLogWhenSuppressed()
        {
            var service = CreateService("Initializer");
            service.Logs.Should().BeEmpty();

            service.InitializeActivationState(true);
            service.IsActive.Should().BeTrue();
            service.Logs.Should().BeEmpty();

            service.InitializeActivationState(false);
            service.IsActive.Should().BeFalse();
            service.Logs.Should().BeEmpty();
        }

        private static ServiceListModel CreateService(string name)
        {
            return new ServiceListModel
            {
                DisplayName = name,
                Type = ServiceType.Http
            };
        }

        private sealed class MainViewModelTestEnvironment : IDisposable
        {
            private readonly string servicesFilePath;
            public MainViewModel ViewModel { get; }

            public MainViewModelTestEnvironment(TestLoggingService logger)
            {
                var basePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(basePath);
                servicesFilePath = Path.Combine(basePath, "services.json");

                var fileDialog = new TestFileDialogService();
                var csvViewModel = new CsvViewerViewModel(fileDialog, Path.Combine(basePath, "csv_config.json"));
                var csvService = new TestCsvService();
                var csvOutput = new TestCsvOutput();
                var csvAdapter = new CsvServiceAdapter(csvViewModel, csvService, csvOutput, logger);
                var networkService = new TestNetworkConfigurationService();
                var networkConfig = new NetworkConfigurationViewModel(networkService, logger);
                var serviceCatalog = new TestServiceCatalog();
                var startupPreferences = new TestStartupPreferencesService();

                ViewModel = new MainViewModel(
                    csvAdapter,
                    networkConfig,
                    networkService,
                    serviceCatalog,
                    new Dictionary<ServiceType, IEditServiceHandler>(),
                    startupPreferences,
                    logger,
                    servicesFilePath);
            }

            public void Dispose()
            {
                if (File.Exists(servicesFilePath))
                {
                    File.Delete(servicesFilePath);
                }

                var directory = Path.GetDirectoryName(servicesFilePath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
        }

        private sealed class TestLoggingService : ILoggingService
        {
            public List<string> Messages { get; } = new();

            public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

            public event Action<LogEntry>? LogAdded;

            public void Log(string message, LogLevel level)
            {
                Messages.Add(message);
                LogAdded?.Invoke(new LogEntry
                {
                    Message = message,
                    Level = level
                });
            }

            public void Reload()
            {
            }
        }

        private sealed class TestNetworkConfigurationService : INetworkConfigurationService
        {
            private NetworkConfiguration currentConfiguration = new();

            public event EventHandler<NetworkConfiguration>? ConfigurationChanged;

            public Task ApplyConfigurationAsync(
                NetworkConfiguration configuration,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                currentConfiguration = configuration ?? new NetworkConfiguration();
                ConfigurationChanged?.Invoke(this, currentConfiguration);
                return Task.CompletedTask;
            }

            public Task<NetworkConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(currentConfiguration);
            }
        }

        private sealed class TestFileDialogService : IFileDialogService
        {
            public string? OpenFile() => null;

            public string? SelectFolder() => null;

            public string? SaveFile(string suggestedName, string filter) => null;
        }

        private sealed class TestCsvService : DesktopApplicationTemplate.Core.Services.Protocols.Csv.ICsvService
        {
            public bool EnsureColumnsForService(CsvConfiguration configuration, string serviceName) => false;

            public bool RemoveColumnsForService(CsvConfiguration configuration, CsvServiceState state, string serviceName) => false;

            public void RecordLog(CsvConfiguration configuration, CsvServiceState state, ICsvOutput output, string serviceName, string message)
            {
            }

            public void AppendRow(CsvConfiguration configuration, CsvServiceState state, ICsvOutput output, IEnumerable<string?> values)
            {
            }
        }

        private sealed class TestCsvOutput : ICsvOutput
        {
            public bool FileExists(string filePath) => false;

            public long GetFileLength(string filePath) => 0;

            public void AppendLine(string filePath, string content)
            {
            }

            public void EnsureDirectoryForFile(string filePath)
            {
            }

            public void DeleteFile(string filePath)
            {
            }
        }

        private sealed class TestServiceCatalog : IServiceCatalog
        {
            private readonly List<IServiceDescriptor> descriptors = new();

            public IReadOnlyCollection<IServiceDescriptor> Descriptors => descriptors;

            public event EventHandler? DescriptorsChanged;

            public IEnumerable<IServiceDescriptor> GetAll() => Descriptors;

            public void UpdateDescriptors(IEnumerable<IServiceDescriptor> newDescriptors)
            {
                descriptors.Clear();

                foreach (var descriptor in newDescriptors ?? Enumerable.Empty<IServiceDescriptor>())
                {
                    descriptors.Add(descriptor);
                }

                DescriptorsChanged?.Invoke(this, EventArgs.Empty);
            }

            public bool TryGetById(string descriptorId, out IServiceDescriptor descriptor)
            {
                var match = descriptors.FirstOrDefault(
                    d => string.Equals(d.Id, descriptorId, StringComparison.Ordinal));

                if (match is not null)
                {
                    descriptor = match;
                    return true;
                }

                descriptor = null!;
                return false;
            }
        }

        private sealed class TestStartupPreferencesService : IStartupPreferencesService
        {
            public StartupPreferencesResult ShowDialog(bool runServicesOnStartup, bool runUiOnStartup)
                => new(true, runServicesOnStartup, runUiOnStartup);
        }
    }
}
