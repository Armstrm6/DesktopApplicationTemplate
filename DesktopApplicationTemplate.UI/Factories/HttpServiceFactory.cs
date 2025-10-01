using System;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Common.Descriptors;

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
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                Type = ServiceType.Http,
                DescriptorId = HttpServiceDescriptor.DescriptorId,
                IsActive = false
            };

            svc.SetPayload(options);

            _getMainView().GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
