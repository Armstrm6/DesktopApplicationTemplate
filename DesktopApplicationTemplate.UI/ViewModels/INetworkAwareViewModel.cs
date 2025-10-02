using DesktopApplicationTemplate.Core.Models;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public interface INetworkAwareViewModel
    {
        void UpdateNetworkConfiguration(NetworkConfiguration configuration);
    }
}
