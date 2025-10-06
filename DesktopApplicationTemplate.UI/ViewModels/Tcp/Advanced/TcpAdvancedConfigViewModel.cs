using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;

namespace DesktopApplicationTemplate.UI.ViewModels.Tcp.Advanced;

/// <summary>
/// View model for editing advanced TCP configuration values.
/// </summary>
public class TcpAdvancedConfigViewModel : AdvancedConfigViewModelBase<TcpServiceOptions>
{
    private readonly TcpServiceOptions _options;
    private string _computerIp;
    private string _listeningPort;
    private string _serverIp;
    private string _serverGateway;
    private string _serverPort;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpAdvancedConfigViewModel"/> class.
    /// </summary>
    public TcpAdvancedConfigViewModel(TcpServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _computerIp = options.ComputerIp;
        _listeningPort = options.ListeningPort;
        _serverIp = options.ServerIp;
        _serverGateway = options.ServerGateway;
        _serverPort = options.ServerPort;
    }

    /// <summary>
    /// Gets or sets the computer IP displayed in the TCP message view.
    /// </summary>
    public string ComputerIp
    {
        get => _computerIp;
        set
        {
            if (_computerIp == value)
            {
                return;
            }

            _computerIp = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the listening port shown in the TCP message view.
    /// </summary>
    public string ListeningPort
    {
        get => _listeningPort;
        set
        {
            if (_listeningPort == value)
            {
                return;
            }

            _listeningPort = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the destination server IP.
    /// </summary>
    public string ServerIp
    {
        get => _serverIp;
        set
        {
            if (_serverIp == value)
            {
                return;
            }

            _serverIp = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the destination gateway.
    /// </summary>
    public string ServerGateway
    {
        get => _serverGateway;
        set
        {
            if (_serverGateway == value)
            {
                return;
            }

            _serverGateway = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the destination server port.
    /// </summary>
    public string ServerPort
    {
        get => _serverPort;
        set
        {
            if (_serverPort == value)
            {
                return;
            }

            _serverPort = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    protected override TcpServiceOptions OnSave()
    {
        Logger?.Log("TCP advanced options start", LogLevel.Debug);
        _options.ComputerIp = ComputerIp;
        _options.ListeningPort = ListeningPort;
        _options.ServerIp = ServerIp;
        _options.ServerGateway = ServerGateway;
        _options.ServerPort = ServerPort;
        Logger?.Log("TCP advanced options finished", LogLevel.Debug);
        return _options;
    }
}
