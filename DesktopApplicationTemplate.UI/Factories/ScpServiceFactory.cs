using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class ScpServiceFactory : IServiceFactory
    {
        private readonly MainView _mainView;

        public ServiceType ServiceType => ServiceType.Scp;

        public ScpServiceFactory(MainView mainView)
        {
            _mainView = mainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<ScpServiceOptions>)optionsObj;
            var name = ctx.Name;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                DisplayName = $"{ServiceType.Scp.ToLegacyString()} - {name}",
                ServiceType = ServiceType.Scp,
                IsActive = false,
                ScpOptions = options
            };

            _mainView.GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
