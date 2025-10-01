using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class HttpServiceFactory : IServiceFactory
    {
        private readonly Func<MainView> _getMainView;
        private readonly IServiceCatalog _catalog;

        public ServiceType ServiceType => ServiceType.Http;

        public HttpServiceFactory(Func<MainView> getMainView, IServiceCatalog catalog)
        {
            _getMainView = getMainView;
            _catalog = catalog;
        }

        public ServiceListModel Create(ServiceFactoryContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var descriptor = _catalog.TryGetById(context.DescriptorId, out var resolved)
                ? resolved
                : context.Descriptor;

            var options = context.GetPayload<HttpServiceOptions>() ?? new HttpServiceOptions();

            var svc = new ServiceListModel
            {
                Type = ServiceType.Http,
                DescriptorId = context.DescriptorId,
                IsActive = false
            };

            svc.SetPayload(options);

            svc.ApplyDescriptor(descriptor, context.ServiceName);

            _getMainView().GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
