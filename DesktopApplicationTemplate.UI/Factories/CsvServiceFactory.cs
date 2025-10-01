using System;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Common.Descriptors;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class CsvServiceFactory : IServiceFactory
    {
        private readonly Func<MainView> _getMainView;

        public ServiceType ServiceType => ServiceType.Csv;

        public CsvServiceFactory(Func<MainView> getMainView)
        {
            _getMainView = getMainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<CsvServiceOptions>)optionsObj;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                Type = ServiceType.Csv,
                DescriptorId = CsvServiceDescriptor.DescriptorId,
                IsActive = false
            };

            svc.SetPayload(options);

            _getMainView().GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
