using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Factories
{
    public interface IServiceFactory
    {
        string ServiceType { get; }
        ServiceListModel Create(object options);
    }
}
