using System;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Common.Descriptors;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class FileObserverServiceFactory : IServiceFactory
    {
        private readonly Func<MainView> _getMainView;

        public ServiceType ServiceType => ServiceType.FileObserver;

        public FileObserverServiceFactory(Func<MainView> getMainView)
        {
            _getMainView = getMainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<FileObserverServiceOptions>)optionsObj;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                Type = ServiceType.FileObserver,
                DescriptorId = FileObserverServiceDescriptor.DescriptorId,
                IsActive = false
            };

            svc.SetPayload(options);

            _getMainView().GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
