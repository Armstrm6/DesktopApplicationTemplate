using DesktopApplicationTemplate.Core.Services;
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
        public void SaveAndLoad_RoundTripsServices()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var original = typeof(ServicePersistence).GetField("FilePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            string? oldPath = (string?)original!.GetValue(null);
            original.SetValue(null, Path.Combine(tempDir, "services.json"));
            try
            {
                var services = new List<ServiceViewModel>
                {
                    new ServiceViewModel{DisplayName="A", ServiceType="Heartbeat", IsActive=true, Order=0},
                    new ServiceViewModel{DisplayName="B", ServiceType="TCP", IsActive=false, Order=1}
                };

                ServicePersistence.Save(services);
                var loaded = ServicePersistence.Load();
                Assert.Equal(2, loaded.Count);
                Assert.Equal("A", loaded[0].DisplayName);
            }
            finally
            {
                original.SetValue(null, oldPath);
                Directory.Delete(tempDir, true);
            }

            ConsoleTestLogger.LogPass();
        }
    }
}
