using DesktopApplicationTemplate.Persistence;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Services;
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
                var services = new List<ServiceListModel>
                {
                    new ServiceListModel{DisplayName="A", ServiceType=ServiceType.Heartbeat, IsActive=true, Order=0},
                    new ServiceListModel{DisplayName="B", ServiceType=ServiceType.Tcp, IsActive=false, Order=1}
                };
                services[0].AssociatedServices.Add("B");
                services[1].AssociatedServices.Add("A");

                ServicePersistence.Save(services);
                var loaded = ServicePersistence.Load();
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
                var a = new ServiceListModel { DisplayName = "A", ServiceType = ServiceType.Tcp };
                var b = new ServiceListModel { DisplayName = "B", ServiceType = ServiceType.Tcp };
                a.AssociatedServices.Add("B");
                b.AssociatedServices.Add("A");
                var services = new List<ServiceListModel> { a, b };

                ServicePersistence.Save(services);

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
                var opt = host.Services.GetRequiredService<IOptions<TcpServiceOptions>>().Value;

                var services = new List<ServiceListModel>
                {
                    TestHelpers.CreateService(type, name)
                    {
                        IsActive=false,
                        Order=0,
                        TcpOptions = new TcpServiceOptions
                        {
                            Host = "h",
                            Port = 42,
                            UseUdp = true,
                            Mode = TcpServiceMode.Sending,
                            InputMessage = "in",
                            Script = "return message;",
                            OutputMessage = "out",
                            LastTestMessage = "last"
                        }
                    }
                };

                ServicePersistence.Save(services);

                // mutate options to verify load restores
                opt.Host = "changed";
                opt.Port = 100;
                opt.UseUdp = false;
                opt.Mode = TcpServiceMode.Listening;
                opt.InputMessage = "changed";
                opt.Script = "changed";
                opt.OutputMessage = "changed";
                opt.LastTestMessage = "changed";

                var loaded = ServicePersistence.Load();
                var info = Assert.Single(loaded);
                Assert.NotNull(info.TcpOptions);
                Assert.Equal("h", info.TcpOptions!.Host);
                Assert.Equal(42, info.TcpOptions.Port);
                Assert.True(info.TcpOptions.UseUdp);
                Assert.Equal(TcpServiceMode.Sending, info.TcpOptions.Mode);
                Assert.Equal("in", info.TcpOptions.InputMessage);
                Assert.Equal("return message;", info.TcpOptions.Script);
                Assert.Equal("out", info.TcpOptions.OutputMessage);
                Assert.Equal("last", info.TcpOptions.LastTestMessage);

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
                var opt = host.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;

                var services = new List<ServiceListModel>
                {
                    TestHelpers.CreateService(ServiceType.Ftp, "One")
                    {
                        IsActive = false,
                        Order = 0,
                        FtpOptions = new FtpServerOptions
                        {
                            Port = 21,
                            RootPath = "/srv",
                            AllowAnonymous = true,
                            Username = "u",
                            Password = "p"
                        }
                    }
                };

                ServicePersistence.Save(services);

                // mutate options to verify load restores
                opt.Port = 100;
                opt.RootPath = "changed";
                opt.AllowAnonymous = false;
                opt.Username = null;
                opt.Password = null;

                var loaded = ServicePersistence.Load();
                var info = Assert.Single(loaded);
                Assert.NotNull(info.FtpOptions);
                Assert.Equal(21, info.FtpOptions!.Port);
                Assert.Equal("/srv", info.FtpOptions.RootPath);
                Assert.True(info.FtpOptions.AllowAnonymous);
                Assert.Equal("u", info.FtpOptions.Username);
                Assert.Equal("p", info.FtpOptions.Password);

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
        public void SaveAndLoad_PreservesLegacyFtpOptions()
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
                var opt = host.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;

                var services = new List<ServiceListModel>
                {
                    TestHelpers.CreateService(ServiceType.Ftp, "One")
                    {
                        IsActive = false,
                        Order = 0,
                        FtpOptions = new FtpServerOptions
                        {
                            Port = 21,
                            RootPath = "/srv",
                            AllowAnonymous = true,
                            Username = "u",
                            Password = "p"
                        }
                    }
                };

                ServicePersistence.Save(services);

                // mutate options to verify load restores
                opt.Port = 100;
                opt.RootPath = "changed";
                opt.AllowAnonymous = false;
                opt.Username = null;
                opt.Password = null;

                var loaded = ServicePersistence.Load();
                var info = Assert.Single(loaded);
                Assert.NotNull(info.FtpOptions);
                Assert.Equal(21, info.FtpOptions!.Port);
                Assert.Equal("/srv", info.FtpOptions.RootPath);
                Assert.True(info.FtpOptions.AllowAnonymous);
                Assert.Equal("u", info.FtpOptions.Username);
                Assert.Equal("p", info.FtpOptions.Password);

                // global options restored
                var restoredLegacy = host.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;
                Assert.Equal(21, restoredLegacy.Port);
                Assert.Equal("/srv", restoredLegacy.RootPath);
                Assert.True(restoredLegacy.AllowAnonymous);
                Assert.Equal("u", restoredLegacy.Username);
                Assert.Equal("p", restoredLegacy.Password);
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
                File.WriteAllText(ServicePersistence.FilePath, "[{'DisplayName':'Svc','ServiceType':'FTP Server'}]".Replace(''','"'));
                var logger = new ListLogger();
                var loaded = ServicePersistence.Load(logger);
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
                File.WriteAllText(ServicePersistence.FilePath, "[{'DisplayName':'Svc','ServiceType':'Unknown'}]".Replace(''','"'));
                var logger = new ListLogger();
                var loaded = ServicePersistence.Load(logger);
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

        private sealed class ListLogger : ILoggingService
        {
            public List<string> Messages { get; } = new();
            public void Log(string message, LogLevel level = LogLevel.Information) => Messages.Add(message);
        }
    }
}
