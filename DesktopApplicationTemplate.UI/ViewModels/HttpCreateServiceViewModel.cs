using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for configuring a new HTTP service.
/// </summary>
public class HttpCreateServiceViewModel : ServiceCreateViewModelBase<HttpServiceOptions>
{
    private string _serviceName = string.Empty;
    private string _baseUrl = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpCreateServiceViewModel"/> class.
    /// </summary>
    public HttpCreateServiceViewModel(ILoggingService? logger = null)
        : base(logger)
    {
    }

    /// <summary>
    /// Raised when the user completes service creation.
    /// </summary>
    public event Action<string, HttpServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when the user cancels creation.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<HttpServiceOptions>? AdvancedConfigRequested;

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Command to open advanced configuration.
    /// </summary>
    public ICommand OpenAdvancedConfigCommand => AdvancedConfigCommand;

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Base URL for requests.
    /// </summary>
    public string BaseUrl
    {
        get => _baseUrl;
        set { _baseUrl = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public HttpServiceOptions Options { get; } = new();

    /// <inheritdoc />
    protected override void OnSave()
    {
        Logger?.Log("HTTP create options start", LogLevel.Debug);
        Options.BaseUrl = BaseUrl;
        Logger?.Log("HTTP create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, Options);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        Logger?.Log("HTTP create cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Logger?.Log("Opening HTTP advanced config", LogLevel.Debug);
        Options.BaseUrl = BaseUrl;
        AdvancedConfigRequested?.Invoke(Options);
    }
}
