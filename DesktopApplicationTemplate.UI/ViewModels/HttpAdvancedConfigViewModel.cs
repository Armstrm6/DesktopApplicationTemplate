using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced HTTP options.
/// </summary>
public class HttpAdvancedConfigViewModel : AdvancedConfigViewModelBase<HttpServiceOptions>
{
    private readonly HttpServiceOptions _options;
    private string? _username;
    private string? _password;
    private string? _certificatePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpAdvancedConfigViewModel"/> class.
    /// </summary>
    public HttpAdvancedConfigViewModel(HttpServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _username = options.Username;
        _password = options.Password;
        _certificatePath = options.ClientCertificatePath;
    }

    /// <summary>
    /// Username for basic authentication.
    /// </summary>
    public string? Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Password for basic authentication.
    /// </summary>
    public string? Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Path to client TLS certificate.
    /// </summary>
    public string? ClientCertificatePath
    {
        get => _certificatePath;
        set { _certificatePath = value; OnPropertyChanged(); }
    }

    protected override HttpServiceOptions OnSave()
    {
        Logger?.Log("HTTP advanced options start", LogLevel.Debug);
        _options.Username = string.IsNullOrWhiteSpace(Username) ? null : Username;
        _options.Password = string.IsNullOrWhiteSpace(Password) ? null : Password;
        _options.ClientCertificatePath = string.IsNullOrWhiteSpace(ClientCertificatePath) ? null : ClientCertificatePath;
        Logger?.Log("HTTP advanced options finished", LogLevel.Debug);
        return _options;
    }

    protected override void OnBack()
    {
        Logger?.Log("HTTP advanced options back", LogLevel.Debug);
    }
}
