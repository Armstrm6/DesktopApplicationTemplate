using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
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
    }
}
