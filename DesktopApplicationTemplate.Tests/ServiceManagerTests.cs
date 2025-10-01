using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TestCommon;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ServiceManagerTests
{
    [Fact]
    public async Task Sync_StartsAndStopsServicesBasedOnConfiguration()
    {
        using var tempDirectory = new TempDirectory();
        var managerSetup = CreateManager(tempDirectory.Path, descriptorId: "service.test");
        await using var manager = managerSetup.Manager;

        await manager.SyncAsync(CancellationToken.None);

        var factory = managerSetup.Factory;
        var runtime = Assert.Single(factory.CreatedRuntimes);
        Assert.Equal(1, runtime.StartCalls);
        Assert.Equal(0, runtime.StopCalls);
        Assert.Equal("Service One", Assert.Single(factory.Contexts).DisplayName);

        managerSetup.Configuration["Services:0:IsActive"] = "false";

        await manager.SyncAsync(CancellationToken.None);

        Assert.Equal(1, runtime.StartCalls);
        Assert.Equal(1, runtime.StopCalls);
        Assert.Empty(manager.ActiveServices);

        ConsoleTestLogger.LogPass();
    }

    [Theory]
    [InlineData("HB")]
    [InlineData("Heartbeat")]
    public async Task Sync_ResolvesLegacyServiceCodes(string legacyCode)
    {
        using var tempDirectory = new TempDirectory();
        var setup = CreateManager(tempDirectory.Path, descriptorId: "service.test", legacyCode: legacyCode, includeDescriptorId: false);
        await using var manager = setup.Manager;

        await manager.SyncAsync(CancellationToken.None);

        var factory = setup.Factory;
        Assert.Single(factory.CreatedRuntimes);
        Assert.Equal("service.test", Assert.Single(factory.Contexts).DescriptorId);

        ConsoleTestLogger.LogPass();
    }

    private static ManagerSetup CreateManager(string tempDirectory, string descriptorId, string legacyCode = "HB", bool includeDescriptorId = true)
    {
        var descriptor = new TestDescriptor(descriptorId);
        var catalog = new ServiceCatalog(new[] { descriptor });

        var services = new ServiceCollection();
        services.AddSingleton<TestRuntimeFactory>();
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var values = new Dictionary<string, string?>
        {
            ["Services:0:DisplayName"] = "Service One",
            ["Services:0:IsActive"] = "true",
            ["Services:0:Order"] = "0"
        };

        if (includeDescriptorId)
        {
            values["Services:0:DescriptorId"] = descriptorId;
        }
        else
        {
            values["Services:0:ServiceType"] = legacyCode;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        var servicesFile = Path.Combine(tempDirectory, "services.json");
        var manager = new ServiceManager(
            NullLogger<ServiceManager>.Instance,
            configuration,
            catalog,
            scopeFactory,
            servicesFile);

        var factory = serviceProvider.GetRequiredService<TestRuntimeFactory>();

        return new ManagerSetup(manager, configuration, factory);
    }

    private sealed record ManagerSetup(ServiceManager Manager, IConfigurationRoot Configuration, TestRuntimeFactory Factory);

    private sealed class TestRuntimeFactory : IServiceRuntimeFactory
    {
        private readonly List<TestRuntime> _runtimes = new();
        private readonly List<ServiceRuntimeContext> _contexts = new();

        public IReadOnlyList<TestRuntime> CreatedRuntimes => _runtimes;
        public IReadOnlyList<ServiceRuntimeContext> Contexts => _contexts;

        public IServiceRuntime Create(ServiceRuntimeContext context)
        {
            var runtime = new TestRuntime();
            _runtimes.Add(runtime);
            _contexts.Add(context);
            return runtime;
        }
    }

    private sealed class TestRuntime : IServiceRuntime
    {
        public int StartCalls { get; private set; }
        public int StopCalls { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartCalls++;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            StopCalls++;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class TestDescriptor : IServiceDescriptor
    {
        private readonly IReadOnlyCollection<ServiceFactoryBinding> _factories;

        public TestDescriptor(string id)
        {
            Id = id;
            DisplayName = "Test Descriptor";
            Category = "Test";
            _factories = new[]
            {
                ServiceFactoryBinding.Create(
                    ServiceFactoryKind.Runtime,
                    typeof(IServiceRuntimeFactory),
                    sp => sp.GetRequiredService<TestRuntimeFactory>())
            };
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public string? Description => null;
        public ServiceType? LegacyType => ServiceType.Heartbeat;
        public IServiceOptionsSerializer? OptionsSerializer => null;
        public IReadOnlyCollection<ServiceFactoryBinding> Factories => _factories;
        public ServicePresentationMetadata Presentation => ServicePresentationMetadata.Empty;
        public bool HasPayloadDescription => false;
        public string? DescribePayload(object? payload) => null;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
            catch
            {
                // ignored for test cleanup
            }
        }
    }
}
