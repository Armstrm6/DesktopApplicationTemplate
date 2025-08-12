using DesktopApplicationTemplate.UI.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
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

        private class DummyPortViewModel : INotifyPropertyChanged
        {
            private string _port = string.Empty;
            public string Port
            {
                get => _port;
                set { _port = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Port))); }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void AttachPortListener_UpdatesPort()
        {
            var svc = new ServiceViewModel();
            var dummy = new DummyPortViewModel { Port = "1234" };
            svc.AttachPortListener(dummy);

            Assert.Equal("1234", svc.Port);

            dummy.Port = "5678";

            Assert.Equal("5678", svc.Port);

            ConsoleTestLogger.LogPass();
        }
    }
}
