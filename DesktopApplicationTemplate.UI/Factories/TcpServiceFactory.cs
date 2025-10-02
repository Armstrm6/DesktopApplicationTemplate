using System;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class TcpServiceFactory : IServiceFactory
    {
        private readonly Func<MainView> _getMainView;

        public ServiceType ServiceType => ServiceType.Tcp;

        public TcpServiceFactory(Func<MainView> getMainView)
        {
            _getMainView = getMainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<TcpServiceOptions>)optionsObj;
            var name = ctx.Name;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                DisplayName = $"{ServiceType.Tcp.ToLegacyString()} - {name}",
                Type = ServiceType.Tcp,
                IsActive = false,
                TcpOptions = options
            };

            _getMainView().GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
