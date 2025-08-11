using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.UI.Models;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface INetworkConfigurationService
    {
        Task<NetworkConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
        Task ApplyConfigurationAsync(NetworkConfiguration configuration, CancellationToken cancellationToken = default);
        event EventHandler<NetworkConfiguration>? ConfigurationChanged;
    }
}
