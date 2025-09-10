using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class FtpServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider _services;
        private readonly MainView _mainView;

        public string ServiceType => "FTP Server";

        public FtpServiceFactory(IServiceProvider services, MainView mainView)
        {
            _services = services;
            _mainView = mainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<FtpServerOptions>)optionsObj;
            var name = ctx.Name;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                DisplayName = $"FTP Server - {name}",
                ServiceType = "FTP Server",
                IsActive = false,
                FtpOptions = options
            };

            _mainView.GetOrCreateServicePage(svc);

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
