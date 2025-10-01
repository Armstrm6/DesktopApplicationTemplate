using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Hid.Options;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;

namespace DesktopApplicationTemplate.Services.Hid.UI.Factories
{
    public class HidServiceFactory : IServiceFactory
    {
        private readonly Func<MainView> _getMainView;
        private readonly IServiceCatalog _catalog;

        public ServiceType ServiceType => ServiceType.Hid;

        public HidServiceFactory(Func<MainView> getMainView, IServiceCatalog catalog)
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

            var options = context.GetPayload<HidServiceOptions>() ?? new HidServiceOptions();

            var svc = new ServiceListModel
            {
                Type = ServiceType.Hid,
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
