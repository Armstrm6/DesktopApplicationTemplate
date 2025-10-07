using System;
using System.Net;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;

namespace DesktopApplicationTemplate.UI.ViewModels.Tcp.Create;

/// <summary>
/// View model for configuring a new TCP service before creation.
/// </summary>
public class TcpCreateServiceViewModel : ServiceCreateViewModelBase<TcpServiceOptions>
{
    private string _host = string.Empty;
    private int _port;
    private bool _useUdp;
    private TcpServiceMode _mode;
    private TcpConnectionRole _connectionRole = TcpConnectionRole.Server;
    private string _serverHost = NetworkUtilities.GetLocalIpAddress();
    private string _clientHost = string.Empty;
    private string _subnetMask = string.Empty;
    private string _primaryDns = string.Empty;
    private string _alternateDns = string.Empty;
    private string _destinationHost = string.Empty;
    private int _destinationPort;
    private string _destinationGateway = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpCreateServiceViewModel"/> class.
    /// </summary>
    public TcpCreateServiceViewModel(IServiceRule rule, ILoggingService? logger = null)
        : base(rule, logger: logger)
    {
        var configuration = NetworkUtilities.GetLocalNetworkConfiguration();
        _serverHost = string.IsNullOrWhiteSpace(configuration.IpAddress)
            ? NetworkUtilities.GetLocalIpAddress()
            : configuration.IpAddress;
        Host = _serverHost;
        SubnetMask = configuration.SubnetMask;
        PrimaryDns = configuration.DnsPrimary;
        AlternateDns = configuration.DnsSecondary;
    }

    /// <summary>
    /// Host name or address for the service.
    /// </summary>
    public string Host
    {
        get => _host;
        set
        {
            _host = value;
            var error = Rule.ValidateRequired(value, "Host");
            if (error is not null)
                AddError(nameof(Host), error);
            else
                ClearErrors(nameof(Host));
            OnPropertyChanged();
            if (_connectionRole == TcpConnectionRole.Server)
            {
                _serverHost = value;
            }
            else
            {
                _clientHost = value;
            }
        }
    }

    /// <summary>
    /// Port number for the service.
    /// </summary>
    public int Port
    {
        get => _port;
        set
        {
            _port = value;
            var error = Rule.ValidatePort(value);
            if (error is not null)
                AddError(nameof(Port), error);
            else
                ClearErrors(nameof(Port));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Subnet mask associated with the connection.
    /// </summary>
    public string SubnetMask
    {
        get => _subnetMask;
        set
        {
            _subnetMask = value ?? string.Empty;
            ValidateOptionalIpAddress(_subnetMask, nameof(SubnetMask), "Subnet");
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Primary DNS server used for resolving host names.
    /// </summary>
    public string PrimaryDns
    {
        get => _primaryDns;
        set
        {
            _primaryDns = value ?? string.Empty;
            ValidateOptionalIpAddress(_primaryDns, nameof(PrimaryDns), "Primary DNS");
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Alternate DNS server used for resolving host names.
    /// </summary>
    public string AlternateDns
    {
        get => _alternateDns;
        set
        {
            _alternateDns = value ?? string.Empty;
            ValidateOptionalIpAddress(_alternateDns, nameof(AlternateDns), "Alternate DNS");
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether UDP should be used instead of TCP.
    /// </summary>
    public bool UseUdp
    {
        get => _useUdp;
        set { _useUdp = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Operating mode for the service.
    /// </summary>
    public TcpServiceMode Mode
    {
        get => _mode;
        set
        {
            if (_mode == value)
            {
                return;
            }

            _mode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSendingEnabled));
            if (!IsSendingEnabled)
            {
                ClearErrors(nameof(DestinationHost));
                ClearErrors(nameof(DestinationPort));
            }
        }
    }

    /// <summary>
    /// Available service modes.
    /// </summary>
    public TcpServiceMode[] Modes { get; } = (TcpServiceMode[])Enum.GetValues(typeof(TcpServiceMode));

    /// <summary>
    /// Indicates whether the service should expose sending configuration fields.
    /// </summary>
    public bool IsSendingEnabled => Mode != TcpServiceMode.Listening;

    /// <summary>
    /// Available TCP connection roles.
    /// </summary>
    public TcpConnectionRole[] ConnectionRoles { get; } = (TcpConnectionRole[])Enum.GetValues(typeof(TcpConnectionRole));

    /// <summary>
    /// Remote host used when sending messages.
    /// </summary>
    public string DestinationHost
    {
        get => _destinationHost;
        set
        {
            _destinationHost = value ?? string.Empty;
            if (IsSendingEnabled)
            {
                var error = Rule.ValidateRequired(_destinationHost, "Destination Host");
                if (error is not null)
                {
                    AddError(nameof(DestinationHost), error);
                }
                else
                {
                    ClearErrors(nameof(DestinationHost));
                }
            }
            else
            {
                ClearErrors(nameof(DestinationHost));
            }

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Remote port used when sending messages.
    /// </summary>
    public int DestinationPort
    {
        get => _destinationPort;
        set
        {
            _destinationPort = value;
            if (IsSendingEnabled)
            {
                var error = Rule.ValidatePort(value);
                if (error is not null)
                {
                    AddError(nameof(DestinationPort), error);
                }
                else
                {
                    ClearErrors(nameof(DestinationPort));
                }
            }
            else
            {
                ClearErrors(nameof(DestinationPort));
            }

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gateway associated with the remote destination.
    /// </summary>
    public string DestinationGateway
    {
        get => _destinationGateway;
        set
        {
            _destinationGateway = value ?? string.Empty;
            ValidateOptionalIpAddress(_destinationGateway, nameof(DestinationGateway), "Destination Gateway");
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Selected TCP connection role.
    /// </summary>
    public TcpConnectionRole ConnectionRole
    {
        get => _connectionRole;
        set
        {
            if (_connectionRole == value)
            {
                return;
            }

            _connectionRole = value;
            OnPropertyChanged();
            if (_connectionRole == TcpConnectionRole.Server)
            {
                _clientHost = _host;
                if (string.IsNullOrWhiteSpace(_serverHost))
                {
                    _serverHost = NetworkUtilities.GetLocalIpAddress();
                }
                Host = _serverHost;
            }
            else
            {
                _serverHost = _host;
                Host = _clientHost;
            }
            Logger?.Log($"TCP connection role set to {_connectionRole}", LogLevel.Debug);
        }
    }

    /// <inheritdoc />
    protected override void ApplyOptions(TcpServiceOptions options)
    {
        options.Host = Host;
        options.Port = Port;
        options.UseUdp = UseUdp;
        options.SubnetMask = SubnetMask;
        options.PrimaryDns = PrimaryDns;
        options.AlternateDns = AlternateDns;
        options.Mode = Mode;
        options.ConnectionRole = ConnectionRole;
        options.DestinationHost = DestinationHost;
        options.DestinationPort = DestinationPort;
        options.DestinationGateway = DestinationGateway;
    }

    private void ValidateOptionalIpAddress(string value, string propertyName, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ClearErrors(propertyName);
            return;
        }

        if (IPAddress.TryParse(value, out _))
        {
            ClearErrors(propertyName);
            return;
        }

        var label = displayName ?? propertyName;
        AddError(propertyName, $"{label} must be a valid IPv4 address");
    }
}
