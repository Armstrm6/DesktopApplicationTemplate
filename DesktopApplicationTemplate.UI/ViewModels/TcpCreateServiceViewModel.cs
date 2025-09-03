using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for configuring a new TCP service before creation.
/// </summary>
public class TcpCreateServiceViewModel : ServiceCreateViewModelBase<TcpServiceOptions>
{
    private readonly IServiceRule _rule;
    private string _serviceName = string.Empty;
    private string _host = string.Empty;
    private int _port;

    /// <summary>
    /// Current advanced options.
    /// </summary>
    public TcpServiceOptions Options { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpCreateServiceViewModel"/> class.
    /// </summary>
    public TcpCreateServiceViewModel(IServiceRule rule, ILoggingService? logger = null)
        : base(logger)
    {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
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
        set
        {
            _serviceName = value;
            var error = _rule.ValidateRequired(value, "Service name");
            if (error is not null)
                AddError(nameof(ServiceName), error);
            else
                ClearErrors(nameof(ServiceName));
            OnPropertyChanged();
        }
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
            var error = _rule.ValidateRequired(value, "Host");
            if (error is not null)
                AddError(nameof(Host), error);
            else
                ClearErrors(nameof(Host));
            OnPropertyChanged();
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
            var error = _rule.ValidatePort(value);
            if (error is not null)
                AddError(nameof(Port), error);
            else
                ClearErrors(nameof(Port));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<TcpServiceOptions>? AdvancedConfigRequested;

    /// <inheritdoc />
    protected override void OnSave()
    {
        if (HasErrors)
        {
            Logger?.Log("TCP create validation failed", LogLevel.Warning);
            return;
        }
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
