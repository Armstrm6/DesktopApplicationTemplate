using System.Windows.Controls;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Navigation
{
    public interface INavigationHandler
    {
        ServiceType ServiceType { get; }
        Page CreateView(string defaultName);
    }
}
