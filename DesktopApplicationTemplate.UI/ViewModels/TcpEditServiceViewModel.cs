using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing TCP service configuration.
/// </summary>
public class TcpEditServiceViewModel : ServiceEditViewModelBase<TcpServiceOptions>
{
    private readonly IServiceRule _rule;
    private TcpServiceOptions _options = new();
    private string _serviceName = string.Empty;
    private string _host = string.Empty;
    private int _port;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpEditServiceViewModel"/> class.
    /// </summary>
    public TcpEditServiceViewModel(IServiceRule rule, ILoggingService? logger = null)
        : base(logger)
    {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
    }

    /// <summary>
    /// Loads the provided options into the view model.
    /// </summary>
    public void Load(string serviceName, TcpServiceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        Host = _options.Host;
        Port = _options.Port;
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

    /// <inheritdoc />
    protected override void OnSave()
    {
        if (HasErrors)
        {
            Logger?.Log("TCP edit validation failed", LogLevel.Warning);
            return;
        }
        _options.Host = Host;
        _options.Port = Port;
        ServiceUpdated?.Invoke(ServiceName, _options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => Cancelled?.Invoke();

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        _options.Host = Host;
        _options.Port = Port;
        AdvancedConfigRequested?.Invoke(_options);
    }
}

