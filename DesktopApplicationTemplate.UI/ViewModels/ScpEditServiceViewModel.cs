using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing SCP service configuration.
/// </summary>
public class ScpEditServiceViewModel : ServiceEditViewModelBase<ScpServiceOptions>
{
    private ScpServiceOptions _options = new();
    private string _host = string.Empty;
    private string _port = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScpEditServiceViewModel"/> class.
    /// </summary>
    public ScpEditServiceViewModel(IServiceRule rule, ILoggingService? logger = null)
        : base(rule, logger)
    {
    }

    /// <summary>
    /// Loads the supplied options into the view model.
    /// </summary>
    public void Load(string serviceName, ScpServiceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        Host = _options.Host;
        Port = _options.Port.ToString();
        Username = _options.Username;
        Password = _options.Password;
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
        RaiseServiceSaved(_options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => RaiseEditCancelled();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => RaiseAdvancedConfigRequested(_options);
}

