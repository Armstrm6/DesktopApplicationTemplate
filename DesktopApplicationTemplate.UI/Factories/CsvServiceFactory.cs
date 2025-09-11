using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class CsvServiceFactory : IServiceFactory
    {
        private readonly MainView _mainView;

        public ServiceType ServiceType => ServiceType.Csv;

        public CsvServiceFactory(MainView mainView)
        {
            _mainView = mainView;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<CsvServiceOptions>)optionsObj;
            var name = ctx.Name;
            var options = ctx.Options;

            var svc = new ServiceListModel
            {
                DisplayName = $"{ServiceType.Csv.ToLegacyString()} - {name}",
                ServiceType = ServiceType.Csv,
                IsActive = false,
                CsvOptions = options
            };

            _mainView.GetOrCreateServicePage(svc);

            return svc;
        }
    }
}
