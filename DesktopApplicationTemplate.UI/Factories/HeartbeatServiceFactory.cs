using System;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class HeartbeatServiceFactory : IServiceFactory
    {
        private readonly Func<MainView> _getMainView;

        public ServiceType ServiceType => ServiceType.Heartbeat;

        public HeartbeatServiceFactory(Func<MainView> getMainView)
        {
            _getMainView = getMainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<HeartbeatServiceOptions>)optionsObj;
            var name = ctx.Name;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                DisplayName = $"{ServiceType.Heartbeat.ToLegacyString()} - {name}",
                ServiceType = ServiceType.Heartbeat,
                IsActive = false,
                HeartbeatOptions = options
            };

            _getMainView().GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
