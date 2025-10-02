using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Tcp;

/// <summary>
/// Represents the configuration used to initialise a TCP runtime instance.
/// </summary>
public sealed class TcpRuntimeContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TcpRuntimeContext"/> class.
    /// </summary>
    public TcpRuntimeContext(ServiceType serviceType, string serviceName, TcpServiceOptions options, string defaultScript)
    {
        ServiceType = serviceType;
        ServiceName = serviceName;
        Options = options;
        DefaultScript = defaultScript;
    }

    /// <summary>
    /// Gets the owning service type.
    /// </summary>
    public ServiceType ServiceType { get; }

    /// <summary>
    /// Gets the unique service name.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the TCP service options.
    /// </summary>
    public TcpServiceOptions Options { get; }

    /// <summary>
    /// Gets the default script used when no script has been configured.
    /// </summary>
    public string DefaultScript { get; }
}
