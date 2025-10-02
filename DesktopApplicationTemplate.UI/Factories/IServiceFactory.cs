using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Factories
{
    public interface IServiceFactory
    {
        ServiceType ServiceType { get; }
        ServiceListModel Create(object options);
    }
}
