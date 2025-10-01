using DesktopApplicationTemplate.Persistence;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Serialization;
using DesktopApplicationTemplate.Services.Common.Descriptors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ServicePersistenceTests
    {
        [Fact]
        public void SaveAndLoad_RoundTripsServices()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            string oldPath = ServicePersistence.FilePath;
            ServicePersistence.FilePath = Path.Combine(tempDir, "services.json");
            try
            {
                var catalog = CreateCatalog();
                var serviceA = TestHelpers.CreateService(ServiceType.Heartbeat, "A");
                serviceA.IsActive = true;
                serviceA.Order = 0;
                var serviceB = TestHelpers.CreateService(ServiceType.Tcp, "B");
                serviceB.IsActive = false;
                serviceB.Order = 1;
                var services = new List<ServiceListModel> { serviceA, serviceB };
                services[0].AssociatedServices.Add("B");
                services[1].AssociatedServices.Add("A");

                ServicePersistence.Save(services, catalog);
                var loaded = ServicePersistence.Load(catalog);
                Assert.Equal(2, loaded.Count);
                Assert.Equal("A", loaded[0].DisplayName);
                Assert.Contains("B", loaded[0].AssociatedServices);
                Assert.Equal(loaded.Count, loaded.Select(s => s.DisplayName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
            }
            finally
            {
                ServicePersistence.FilePath = oldPath;
                Directory.Delete(tempDir, true);
            }

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void Save_WithCyclicalReferences_DoesNotOverflow()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            string oldPath = ServicePersistence.FilePath;
            ServicePersistence.FilePath = Path.Combine(tempDir, "services.json");
            try
            {
                var catalog = CreateCatalog();
                var a = new ServiceListModel { DisplayName = "A", Type = ServiceType.Tcp, DescriptorId = ServiceType.Tcp.ToDescriptorId() };
                var b = new ServiceListModel { DisplayName = "B", Type = ServiceType.Tcp, DescriptorId = ServiceType.Tcp.ToDescriptorId() };
                a.AssociatedServices.Add("B");
                b.AssociatedServices.Add("A");
                var services = new List<ServiceListModel> { a, b };

                ServicePersistence.Save(services, catalog);

                Assert.True(File.Exists(ServicePersistence.FilePath));
            }
            finally
            {
                ServicePersistence.FilePath = oldPath;
                Directory.Delete(tempDir, true);
            }

            ConsoleTestLogger.LogPass();
        }

        [Theory]
        [InlineData(ServiceType.Tcp, "TCP1")]
        public void SaveAndLoad_PreservesTcpOptions(ServiceType type, string name)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(s => s.Configure<TcpServiceOptions>(_ => { }))
                .Build();
            var setter = typeof(App).GetProperty("AppHost", BindingFlags.Static | BindingFlags.Public)!
                .GetSetMethod(true)!;
            setter.Invoke(null, new object[] { host });

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            string oldPath = ServicePersistence.FilePath;
            ServicePersistence.FilePath = Path.Combine(tempDir, "services.json");

            try
            {
                var catalog = CreateCatalog();
                var opt = host.Services.GetRequiredService<IOptions<TcpServiceOptions>>().Value;

                var serviceInfo = TestHelpers.CreateService(type, name);
                serviceInfo.IsActive = false;
                serviceInfo.Order = 0;
                serviceInfo.SetPayload(new TcpServiceOptions
                {
                    Host = "h",
                    Port = 42,
                    UseUdp = true,
                    Mode = TcpServiceMode.Sending,
                    InputMessage = "in",
                    Script = "return message;",
                    OutputMessage = "out",
                    LastTestMessage = "last"
                });

                var services = new List<ServiceListModel> { serviceInfo };

                ServicePersistence.Save(services, catalog);

                // mutate options to verify load restores
                opt.Host = "changed";
                opt.Port = 100;
                opt.UseUdp = false;
                opt.Mode = TcpServiceMode.Listening;
                opt.InputMessage = "changed";
                opt.Script = "changed";
                opt.OutputMessage = "changed";
                opt.LastTestMessage = "changed";

                var loaded = ServicePersistence.Load(catalog);
                var info = Assert.Single(loaded);
                var payload = Assert.IsType<TcpServiceOptions>(info.Payload);
                Assert.Equal("h", payload.Host);
                Assert.Equal(42, payload.Port);
                Assert.True(payload.UseUdp);
                Assert.Equal(TcpServiceMode.Sending, payload.Mode);
                Assert.Equal("in", payload.InputMessage);
                Assert.Equal("return message;", payload.Script);
                Assert.Equal("out", payload.OutputMessage);
                Assert.Equal("last", payload.LastTestMessage);

                // global options restored
                var restored = host.Services.GetRequiredService<IOptions<TcpServiceOptions>>().Value;
                Assert.Equal("h", restored.Host);
                Assert.Equal(42, restored.Port);
                Assert.True(restored.UseUdp);
                Assert.Equal(TcpServiceMode.Sending, restored.Mode);
                Assert.Equal("in", restored.InputMessage);
                Assert.Equal("return message;", restored.Script);
                Assert.Equal("out", restored.OutputMessage);
                Assert.Equal("last", restored.LastTestMessage);
            }
            finally
            {
                ServicePersistence.FilePath = oldPath;
                Directory.Delete(tempDir, true);
                setter.Invoke(null, new object[] { null! });
                host.Dispose();
            }

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void SaveAndLoad_PreservesFtpServerOptions()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(s => s.Configure<FtpServerOptions>(_ => { }))
                .Build();
            var setter = typeof(App).GetProperty("AppHost", BindingFlags.Static | BindingFlags.Public)!
                .GetSetMethod(true)!;
            setter.Invoke(null, new object[] { host });

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            string oldPath = ServicePersistence.FilePath;
            ServicePersistence.FilePath = Path.Combine(tempDir, "services.json");

            try
            {
                var catalog = CreateCatalog();
                var opt = host.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;

                var serviceInfo = TestHelpers.CreateService(ServiceType.Ftp, "One");
                serviceInfo.IsActive = false;
                serviceInfo.Order = 0;
                serviceInfo.SetPayload(new FtpServerOptions
                {
                    Port = 21,
                    RootPath = "/srv",
                    AllowAnonymous = true,
                    Username = "u",
                    Password = "p"
                });
                var services = new List<ServiceListModel> { serviceInfo };

                ServicePersistence.Save(services, catalog);

                // mutate options to verify load restores
                opt.Port = 100;
                opt.RootPath = "changed";
                opt.AllowAnonymous = false;
                opt.Username = null;
                opt.Password = null;

                var loaded = ServicePersistence.Load(catalog);
                var info = Assert.Single(loaded);
                var payload = Assert.IsType<FtpServerOptions>(info.Payload);
                Assert.Equal(21, payload.Port);
                Assert.Equal("/srv", payload.RootPath);
                Assert.True(payload.AllowAnonymous);
                Assert.Equal("u", payload.Username);
                Assert.Equal("p", payload.Password);

                // global options restored
                var restoredFtp = host.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;
                Assert.Equal(21, restoredFtp.Port);
                Assert.Equal("/srv", restoredFtp.RootPath);
                Assert.True(restoredFtp.AllowAnonymous);
                Assert.Equal("u", restoredFtp.Username);
                Assert.Equal("p", restoredFtp.Password);
            }
            finally
            {
                ServicePersistence.FilePath = oldPath;
                Directory.Delete(tempDir, true);
                setter.Invoke(null, new object[] { null! });
                host.Dispose();
            }

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void Load_PreservesLegacyFtpOptions()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(s => s.Configure<FtpServerOptions>(_ => { }))
                .Build();
            var setter = typeof(App).GetProperty("AppHost", BindingFlags.Static | BindingFlags.Public)!
                .GetSetMethod(true)!;
            setter.Invoke(null, new object[] { host });

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            string oldPath = ServicePersistence.FilePath;
            ServicePersistence.FilePath = Path.Combine(tempDir, "services.json");

            try
            {
                var catalog = CreateCatalog();
                var options = host.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;

                var legacyJson = @"[
    {
        ""DisplayName"": ""Legacy Ftp"",
        ""ServiceType"": ""FTP"",
        ""IsActive"": true,
        ""Created"": ""2024-01-01T00:00:00"",
        ""Order"": 4,
        ""AssociatedServices"": [""Tcp1""],
        ""TotalExecutionTimeMs"": 123.0,
        ""ExecutionCount"": 10,
        ""FtpOptions"": {
            ""Port"": 21,
            ""RootPath"": ""/srv"",
            ""AllowAnonymous"": true,
            ""Username"": ""legacy-user"",
            ""Password"": ""legacy-pass""
        }
    }
]";

                File.WriteAllText(ServicePersistence.FilePath, legacyJson);

                // mutate options to verify load restores
                options.Port = 100;
                options.RootPath = "changed";
                options.AllowAnonymous = false;
                options.Username = null;
                options.Password = null;

                var loaded = ServicePersistence.Load(catalog);
                var info = Assert.Single(loaded);
                Assert.Equal(ServiceDescriptorIds.Ftp, info.DescriptorId);
                Assert.Equal(ServiceType.Ftp, info.ServiceType);
                Assert.True(info.IsActive);
                Assert.Equal(4, info.Order);
                Assert.Contains("Tcp1", info.AssociatedServices);
                Assert.Equal(123.0, info.TotalExecutionTimeMs);
                Assert.Equal(10, info.ExecutionCount);

                var payload = Assert.IsType<FtpServerOptions>(info.Payload);
                Assert.Equal(21, payload.Port);
                Assert.Equal("/srv", payload.RootPath);
                Assert.True(payload.AllowAnonymous);
                Assert.Equal("legacy-user", payload.Username);
                Assert.Equal("legacy-pass", payload.Password);

                // global options restored
                var restored = host.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;
                Assert.Equal(21, restored.Port);
                Assert.Equal("/srv", restored.RootPath);
                Assert.True(restored.AllowAnonymous);
                Assert.Equal("legacy-user", restored.Username);
                Assert.Equal("legacy-pass", restored.Password);
            }
            finally
            {
                ServicePersistence.FilePath = oldPath;
                Directory.Delete(tempDir, true);
                setter.Invoke(null, new object[] { null! });
                host.Dispose();
            }

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void Load_ParsesLegacyServiceTypeNames()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            string oldPath = ServicePersistence.FilePath;
            ServicePersistence.FilePath = Path.Combine(tempDir, "services.json");
            try
            {
                File.WriteAllText(ServicePersistence.FilePath, "[{'DisplayName':'Svc','ServiceType':'FTP Server'}]".Replace("'", "\""));
                var logger = new ListLogger();
                var loaded = ServicePersistence.Load(CreateCatalog(), logger);
                var info = Assert.Single(loaded);
                Assert.Equal(ServiceType.Ftp, info.ServiceType);
            }
            finally
            {
                ServicePersistence.FilePath = oldPath;
                Directory.Delete(tempDir, true);
            }

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void Load_LogsUnmappedServiceTypes()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            string oldPath = ServicePersistence.FilePath;
            ServicePersistence.FilePath = Path.Combine(tempDir, "services.json");
            try
            {
                File.WriteAllText(ServicePersistence.FilePath, "[{'DisplayName':'Svc','ServiceType':'Unknown'}]".Replace("'", "\""));
                var logger = new ListLogger();
                var loaded = ServicePersistence.Load(CreateCatalog(), logger);
                Assert.Empty(loaded);
                Assert.Contains(logger.Messages, m => m.Contains("Unknown"));
            }
            finally
            {
                ServicePersistence.FilePath = oldPath;
                Directory.Delete(tempDir, true);
            }

            ConsoleTestLogger.LogPass();
        }

        private static IServiceCatalog CreateCatalog()
        {
            var descriptors = new IServiceDescriptor[]
            {
                new TcpServiceDescriptor(new JsonServiceOptionsSerializer<TcpServiceOptions>()),
                new FtpServiceDescriptor(new JsonServiceOptionsSerializer<FtpServerOptions>()),
                new HttpServiceDescriptor(new JsonServiceOptionsSerializer<HttpServiceOptions>()),
                new CsvServiceDescriptor(new JsonServiceOptionsSerializer<CsvServiceOptions>()),
                new FileObserverServiceDescriptor(new JsonServiceOptionsSerializer<FileObserverServiceOptions>()),
                new ScpServiceDescriptor(new JsonServiceOptionsSerializer<ScpServiceOptions>()),
                new HidServiceDescriptor(new JsonServiceOptionsSerializer<HidServiceOptions>()),
                new HeartbeatServiceDescriptor(new JsonServiceOptionsSerializer<HeartbeatServiceOptions>()),
                new MqttServiceDescriptor(new JsonServiceOptionsSerializer<MqttServiceOptions>())
            };

            return new ServiceCatalog(descriptors);
        }

        private sealed class ListLogger : ILoggingService
        {
            public List<string> Messages { get; } = new();
            public LogLevel MinimumLevel { get; set; }
            public event Action<LogEntry> LogAdded
            {
                add { }
                remove { }
            }
            public void Log(string message, LogLevel level)
            {
                Messages.Add(message);
            }
            public void Reload()
            {
            }
        }
    }
}
