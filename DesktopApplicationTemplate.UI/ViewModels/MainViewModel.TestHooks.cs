using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public partial class MainViewModel
    {
        internal Task StartServicesForTestingAsync() => StartServicesAsync();

        internal Task StopServicesForTestingAsync() => StopServicesAsync();
    }
}
