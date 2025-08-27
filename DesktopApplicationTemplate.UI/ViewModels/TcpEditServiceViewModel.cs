using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing TCP service configuration.
/// </summary>
public class TcpEditServiceViewModel : ServiceEditViewModelBase<TcpServiceOptions>
{
    private readonly TcpServiceOptions _options;
    private string _serviceName;
    private string _host;
    private int _port;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpEditServiceViewModel"/> class.
    /// </summary>
    public TcpEditServiceViewModel(string serviceName, TcpServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _host = options.Host;
        _port = options.Port;
    }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, TcpServiceOptions>? ServiceUpdated;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<TcpServiceOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Host name or address for the service.
    /// </summary>
    public string Host
    {
        get => _host;
        set { _host = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Port number for the service.
    /// </summary>
    public int Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(); }
    }

    /// <inheritdoc />
    protected override void OnSave()
    {
        _options.Host = Host;
        _options.Port = Port;
        ServiceUpdated?.Invoke(ServiceName, _options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => Cancelled?.Invoke();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => AdvancedConfigRequested?.Invoke(_options);
}

