using System;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class HttpServiceFactory : IServiceFactory
    {
        private readonly Func<MainView> _getMainView;

        public ServiceType ServiceType => ServiceType.Http;

        public HttpServiceFactory(Func<MainView> getMainView)
        {
            _getMainView = getMainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<HttpServiceOptions>)optionsObj;
            var name = ctx.Name;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                DisplayName = $"{ServiceType.Http.ToLegacyString()} - {name}",
                ServiceType = ServiceType.Http,
                IsActive = false,
                HttpOptions = options
            };

            _getMainView().GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
