using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ServicePersistenceTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
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
                    new ServiceViewModel{DisplayName="A", ServiceType="Heartbeat", IsActive=true, Order=0, ListeningAddress="localhost", Port="1000"},
                    new ServiceViewModel{DisplayName="B", ServiceType="TCP", IsActive=false, Order=1, ListeningAddress="0.0.0.0", Port="2000"}
                };
                services[0].AssociatedServices.Add("B");
                services[1].AssociatedServices.Add("A");

                ServicePersistence.Save(services);
                var loaded = ServicePersistence.Load();
                Assert.Equal(2, loaded.Count);
                Assert.Equal("A", loaded[0].DisplayName);
                Assert.Contains("B", loaded[0].AssociatedServices);
                Assert.Equal("localhost", loaded[0].ListeningAddress);
                Assert.Equal("1000", loaded[0].Port);
            }
            finally
            {
                ServicePersistence.FilePath = oldPath;
                Directory.Delete(tempDir, true);
            }

            ConsoleTestLogger.LogPass();
        }
    }
}
