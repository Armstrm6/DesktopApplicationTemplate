using System;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class ScpServiceFactory : IServiceFactory
    {
        private readonly Func<MainView> _getMainView;

        public ServiceType ServiceType => ServiceType.Scp;

        public ScpServiceFactory(Func<MainView> getMainView)
        {
            _getMainView = getMainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<ScpServiceOptions>)optionsObj;
            var name = ctx.Name;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                DisplayName = $"{ServiceType.Scp.ToLegacyString()} - {name}",
                Type = ServiceType.Scp,
                IsActive = false,
                ScpOptions = options
            };

            _getMainView().GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
