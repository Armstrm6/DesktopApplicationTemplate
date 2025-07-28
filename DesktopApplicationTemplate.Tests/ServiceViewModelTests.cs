using DesktopApplicationTemplate.UI.ViewModels;
using System.Collections.Generic;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ServiceViewModelTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void AddLog_ReferenceUpdatesAssociatedServices()
        {
            var a = new ServiceViewModel { DisplayName = "Heartbeat - A", ServiceType = "Heartbeat" };
            var b = new ServiceViewModel { DisplayName = "TCP - B", ServiceType = "TCP" };
            var services = new List<ServiceViewModel> { a, b };
            ServiceViewModel.ResolveService = (type, name) =>
                services.Find(s => s.ServiceType == type && s.DisplayName.Split(" - ").Last() == name);

            a.AddLog("TCP.B.Test message");

            Assert.Contains(b.Logs, l => l.Message.Contains("Test message"));
            Assert.Contains(b.DisplayName, a.AssociatedServices);
            Assert.Contains(a.DisplayName, b.AssociatedServices);

            ConsoleTestLogger.LogPass();
        }
    }
}
