using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ServiceManagerTests
    {
        [Fact]
        public void Sync_StartsAndStopsServicesBasedOnFile()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var services = new[]
                {
                    new ServiceInfo { DisplayName = "Svc1", ServiceType = ServiceType.Heartbeat, IsActive = true, Order = 0 },
                    new ServiceInfo { DisplayName = "Svc2", ServiceType = ServiceType.Tcp, IsActive = false, Order = 1 }
                };
                File.WriteAllText(tempFile, JsonSerializer.Serialize(services));

                IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Heartbeat:Message", "HB"},
                    {"Heartbeat:IntervalSeconds", "1"}
                }).Build();

                using var manager = new ServiceManager(NullLogger<ServiceManager>.Instance, config, tempFile);
                manager.Sync();

                var runningField = typeof(ServiceManager).GetField("_running", BindingFlags.Instance | BindingFlags.NonPublic)!;
                var running = (IDictionary<string, object>)runningField.GetValue(manager)!;
                Assert.Contains(running.Values, r =>
                    (ServiceType)r.GetType().GetProperty("ServiceType")!.GetValue(r)! == ServiceType.Heartbeat);
                Assert.DoesNotContain(running.Values, r =>
                    (ServiceType)r.GetType().GetProperty("ServiceType")!.GetValue(r)! == ServiceType.Tcp);

                services[0].IsActive = false;
                File.WriteAllText(tempFile, JsonSerializer.Serialize(services));
                manager.Sync();

                running = (IDictionary<string, object>)runningField.GetValue(manager)!;
                Assert.Empty(running);
            }
            finally
            {
                File.Delete(tempFile);
            }
            ConsoleTestLogger.LogPass();
        }

        [Theory]
        [InlineData("HB", ServiceType.Heartbeat)]
        [InlineData("Heartbeat", ServiceType.Heartbeat)]
        public void Sync_LoadsServicesFromConfiguration(string typeValue, ServiceType expected)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempDir, "services.json");

            var configValues = new Dictionary<string, string?>
            {
                {"Services:0:DisplayName", "Svc1"},
                {"Services:0:ServiceType", typeValue},
                {"Services:0:IsActive", "true"},
                {"Services:0:Order", "0"},
                {"Heartbeat:Message", "HB"},
                {"Heartbeat:IntervalSeconds", "1"}
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            using var manager = new ServiceManager(NullLogger<ServiceManager>.Instance, config, tempFile);
            manager.Sync();

            var load = typeof(ServiceManager).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var infos = (List<ServiceInfo>)load.Invoke(manager, null)!;
            var info = Assert.Single(infos);
            Assert.Equal(expected, info.ServiceType);
            Assert.True(info.IsActive);
            ConsoleTestLogger.LogPass();
        }
    }
}
