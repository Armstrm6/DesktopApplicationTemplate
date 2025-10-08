using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Models;

namespace DesktopApplicationTemplate.Core.Services
{
    public interface INetworkConfigurationService
    {
        Task<NetworkConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
        Task ApplyConfigurationAsync(NetworkConfiguration configuration, CancellationToken cancellationToken = default);
        event EventHandler<NetworkConfiguration>? ConfigurationChanged;
    }
}
