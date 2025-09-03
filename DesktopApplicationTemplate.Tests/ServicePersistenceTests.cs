using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI;
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
                var services = new List<ServiceViewModel>
                {
                    new ServiceViewModel{DisplayName="A", ServiceType="Heartbeat", IsActive=true, Order=0},
                    new ServiceViewModel{DisplayName="B", ServiceType="TCP", IsActive=false, Order=1}
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
                var a = new ServiceViewModel { DisplayName = "A", ServiceType = "TCP" };
                var b = new ServiceViewModel { DisplayName = "B", ServiceType = "TCP" };
                a.AssociatedServices.Add("B");
                b.AssociatedServices.Add("A");
                var services = new List<ServiceViewModel> { a, b };

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

        [Fact]
        public void SaveAndLoad_PreservesTcpOptions()
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

                var services = new List<ServiceViewModel>
                {
                    new ServiceViewModel
                    {
                        DisplayName="TCP - One",
                        ServiceType="TCP",
                        IsActive=false,
                        Order=0,
                        TcpOptions = new TcpServiceOptions
                        {
                            Host = "h",
                            Port = 42,
                            UseUdp = true,
                            Mode = TcpServiceMode.Sending,
                            InputMessage = "in",
                            Script = "code",
                            OutputMessage = "out"
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

                var loaded = ServicePersistence.Load();
                var info = Assert.Single(loaded);
                Assert.NotNull(info.TcpOptions);
                Assert.Equal("h", info.TcpOptions!.Host);
                Assert.Equal(42, info.TcpOptions.Port);
                Assert.True(info.TcpOptions.UseUdp);
                Assert.Equal(TcpServiceMode.Sending, info.TcpOptions.Mode);
                Assert.Equal("in", info.TcpOptions.InputMessage);
                Assert.Equal("code", info.TcpOptions.Script);
                Assert.Equal("out", info.TcpOptions.OutputMessage);

                // global options restored
                var restored = host.Services.GetRequiredService<IOptions<TcpServiceOptions>>().Value;
                Assert.Equal("h", restored.Host);
                Assert.Equal(42, restored.Port);
                Assert.True(restored.UseUdp);
                Assert.Equal(TcpServiceMode.Sending, restored.Mode);
                Assert.Equal("in", restored.InputMessage);
                Assert.Equal("code", restored.Script);
                Assert.Equal("out", restored.OutputMessage);
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

                var services = new List<ServiceViewModel>
                {
                    new ServiceViewModel
                    {
                        DisplayName = "FTP Server - One",
                        ServiceType = "FTP Server",
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

                var services = new List<ServiceViewModel>
                {
                    new ServiceViewModel
                    {
                        DisplayName = "FTP - One",
                        ServiceType = "FTP",
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
    }
}
