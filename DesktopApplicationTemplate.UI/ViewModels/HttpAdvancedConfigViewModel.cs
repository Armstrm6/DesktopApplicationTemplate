using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced HTTP options.
/// </summary>
public class HttpAdvancedConfigViewModel : ValidatableViewModelBase, ILoggingViewModel
{
    private readonly HttpServiceOptions _options;
    private string? _username;
    private string? _password;
    private string? _certificatePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpAdvancedConfigViewModel"/> class.
    /// </summary>
    public HttpAdvancedConfigViewModel(HttpServiceOptions options, ILoggingService? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _username = options.Username;
        _password = options.Password;
        _certificatePath = options.ClientCertificatePath;
        Logger = logger;
        SaveCommand = new RelayCommand(Save);
        BackCommand = new RelayCommand(Back);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Command to save the advanced options.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command to navigate back without saving.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Raised when options are saved.
    /// </summary>
    public event Action<HttpServiceOptions>? Saved;

    /// <summary>
    /// Raised when navigation back is requested.
    /// </summary>
    public event Action? BackRequested;

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

    private void Save()
    {
        Logger?.Log("HTTP advanced options start", LogLevel.Debug);
        _options.Username = string.IsNullOrWhiteSpace(Username) ? null : Username;
        _options.Password = string.IsNullOrWhiteSpace(Password) ? null : Password;
        _options.ClientCertificatePath = string.IsNullOrWhiteSpace(ClientCertificatePath) ? null : ClientCertificatePath;
        Logger?.Log("HTTP advanced options finished", LogLevel.Debug);
        Saved?.Invoke(_options);
    }

    private void Back()
    {
        Logger?.Log("HTTP advanced options back", LogLevel.Debug);
        BackRequested?.Invoke();
    }
}
