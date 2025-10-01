using DesktopApplicationTemplate.UI.Views;
using System;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Common.Descriptors;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class FtpServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider _services;
        private readonly Func<MainView> _getMainView;

        public ServiceType ServiceType => ServiceType.Ftp;

        public FtpServiceFactory(IServiceProvider services, Func<MainView> getMainView)
        {
            _services = services;
            _getMainView = getMainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<FtpServerOptions>)optionsObj;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                Type = ServiceType.Ftp,
                DescriptorId = FtpServiceDescriptor.DescriptorId,
                IsActive = false
            };

            svc.SetPayload(options);

            _getMainView().GetOrCreateServicePage(svc);

            var opt = _services.GetRequiredService<IOptions<FtpServerOptions>>().Value;
            opt.Port = options.Port;
            opt.RootPath = options.RootPath;
            opt.AllowAnonymous = options.AllowAnonymous;
            opt.Username = options.Username;
            opt.Password = options.Password;

            return svc;
        }
    }
}
