using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing SCP service configuration.
/// </summary>
public class ScpEditServiceViewModel : ServiceEditViewModelBase<ScpServiceOptions>
{
    private readonly ScpServiceOptions _options;
    private string _serviceName;
    private string _host;
    private string _port;
    private string _username;
    private string _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScpEditServiceViewModel"/> class.
    /// </summary>
    public ScpEditServiceViewModel(string serviceName, ScpServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _host = options.Host;
        _port = options.Port.ToString();
        _username = options.Username;
        _password = options.Password;
    }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, ScpServiceOptions>? ServiceUpdated;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<ScpServiceOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// SCP host name.
    /// </summary>
    public string Host
    {
        get => _host;
        set { _host = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// SCP port number as string.
    /// </summary>
    public string Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Username for authentication.
    /// </summary>
    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Password for authentication.
    /// </summary>
    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    /// <inheritdoc />
    protected override void OnSave()
    {
        _options.Host = Host;
        if (int.TryParse(Port, out var port))
            _options.Port = port;
        _options.Username = Username;
        _options.Password = Password;
        ServiceUpdated?.Invoke(ServiceName, _options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => Cancelled?.Invoke();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => AdvancedConfigRequested?.Invoke(_options);
}

