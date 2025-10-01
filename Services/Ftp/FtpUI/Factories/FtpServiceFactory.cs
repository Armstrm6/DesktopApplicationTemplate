using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Ftp.UI.Options;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.Services.Ftp.UI.Factories
{
    public class FtpServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider _services;
        private readonly Func<MainView> _getMainView;
        private readonly IServiceCatalog _catalog;

        public ServiceType ServiceType => ServiceType.Ftp;

        public FtpServiceFactory(IServiceProvider services, Func<MainView> getMainView, IServiceCatalog catalog)
        {
            _services = services;
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

            var options = context.GetPayload<FtpServerOptions>() ?? new FtpServerOptions();

            var svc = new ServiceListModel
            {
                Type = ServiceType.Ftp,
                DescriptorId = context.DescriptorId,
                IsActive = false
            };

            svc.SetPayload(options);

            svc.ApplyDescriptor(descriptor, context.ServiceName);

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
