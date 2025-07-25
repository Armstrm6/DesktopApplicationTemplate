using DesktopApplicationTemplate.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ServiceManagerTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        public void Sync_StartsAndStopsServicesBasedOnFile()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var services = new[]
                {
                    new ServiceInfo { DisplayName = "Svc1", ServiceType = "Heartbeat", IsActive = true, Order = 0 },
                    new ServiceInfo { DisplayName = "Svc2", ServiceType = "TCP", IsActive = false, Order = 1 }
                };
                File.WriteAllText(tempFile, JsonSerializer.Serialize(services));

                IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Heartbeat:Message", "HB"},
                    {"Heartbeat:IntervalSeconds", "1"}
                }).Build();

                using var manager = new ServiceManager(NullLogger<ServiceManager>.Instance, config, tempFile);
                manager.Sync();
                Assert.Contains("Svc1", manager.ActiveServices);
                Assert.DoesNotContain("Svc2", manager.ActiveServices);

                services[0].IsActive = false;
                File.WriteAllText(tempFile, JsonSerializer.Serialize(services));
                manager.Sync();

                Assert.Empty(manager.ActiveServices);
            }
            finally
            {
                File.Delete(tempFile);
            }
            ConsoleTestLogger.LogPass();
        }
    }
}
