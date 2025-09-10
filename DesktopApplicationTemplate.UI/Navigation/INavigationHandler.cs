using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Navigation
{
    public interface INavigationHandler
    {
        string ServiceType { get; }
        Page CreateView(string defaultName);
    }
}
