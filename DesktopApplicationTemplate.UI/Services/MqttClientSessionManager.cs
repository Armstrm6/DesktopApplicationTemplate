using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Provides MQTT client sessions scoped to individual services.
/// </summary>
public interface IMqttClientSessionManager
{
    /// <summary>
    /// Gets or creates the MQTT client associated with the specified service.
    /// </summary>
    /// <param name="service">The owning service model.</param>
    /// <returns>An MQTT client scoped to the service.</returns>
    IMqttClientService GetClient(ServiceListModel service);

    /// <summary>
    /// Releases the MQTT client associated with the specified service.
    /// </summary>
    /// <param name="service">The owning service model.</param>
    Task ReleaseAsync(ServiceListModel service);
}

/// <summary>
/// Default implementation of <see cref="IMqttClientSessionManager"/> that caches sessions per service.
/// </summary>
public sealed class MqttClientSessionManager : IMqttClientSessionManager
{
    private readonly IMqttClientServiceFactory _clientFactory;
    private readonly ILogger<MqttClientSessionManager> _logger;
    private readonly Dictionary<ServiceListModel, IMqttClientService> _sessions = new();
    private readonly object _syncRoot = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttClientSessionManager"/> class.
    /// </summary>
    public MqttClientSessionManager(IMqttClientServiceFactory clientFactory, ILogger<MqttClientSessionManager> logger)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IMqttClientService GetClient(ServiceListModel service)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        lock (_syncRoot)
        {
            if (!_sessions.TryGetValue(service, out var client))
            {
                var options = service.GetOrCreateOptions(() => new MqttServiceOptions());
                client = _clientFactory.Create(options);
                _sessions.Add(service, client);
                _logger.LogDebug("Created MQTT client session for service {ServiceName}.", service.DisplayName);
            }

            return client;
        }
    }

    /// <inheritdoc />
    public async Task ReleaseAsync(ServiceListModel service)
    {
        if (service is null)
        {
            return;
        }

        IMqttClientService? client = null;

        lock (_syncRoot)
        {
            if (_sessions.TryGetValue(service, out client))
            {
                _sessions.Remove(service);
            }
        }

        if (client is null)
        {
            return;
        }

        try
        {
            await client.DisconnectAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to disconnect MQTT client for service {ServiceName}.", service.DisplayName);
        }
    }
}
