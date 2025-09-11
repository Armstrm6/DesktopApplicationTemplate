using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class HidServiceFactory : IServiceFactory
    {
        private readonly MainView _mainView;

        public ServiceType ServiceType => ServiceType.Hid;

        public HidServiceFactory(MainView mainView)
        {
            _mainView = mainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<HidServiceOptions>)optionsObj;
            var name = ctx.Name;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                DisplayName = $"{ServiceType.Hid.ToLegacyString()} - {name}",
                ServiceType = ServiceType.Hid,
                IsActive = false,
                HidOptions = options
            };

            _mainView.GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
