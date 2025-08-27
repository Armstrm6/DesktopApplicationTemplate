using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for configuring a new TCP service before creation.
/// </summary>
public class TcpCreateServiceViewModel : ServiceCreateViewModelBase<TcpServiceOptions>
{
    private string _serviceName = string.Empty;
    private string _host = string.Empty;
    private int _port = 0;

    /// <summary>
    /// Current advanced options.
    /// </summary>
    public TcpServiceOptions Options { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpCreateServiceViewModel"/> class.
    /// </summary>
    public TcpCreateServiceViewModel(ILoggingService? logger = null)
        : base(logger)
    {
    }

    /// <summary>
    /// Raised when the user finishes configuring the service.
    /// </summary>
    public event Action<string, TcpServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when the user cancels configuration.
    /// </summary>
    public event Action? Cancelled;


    /// <summary>
    /// Name of the service to create.
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

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<TcpServiceOptions>? AdvancedConfigRequested;

    /// <inheritdoc />
    protected override void OnSave()
    {
        Logger?.Log("TCP create options start", LogLevel.Debug);
        Options.Host = Host;
        Options.Port = Port;
        Logger?.Log("TCP create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, Options);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        Logger?.Log("TCP create options cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Logger?.Log("Opening TCP advanced config", LogLevel.Debug);
        Options.Host = Host;
        Options.Port = Port;
        AdvancedConfigRequested?.Invoke(Options);
    }
}
