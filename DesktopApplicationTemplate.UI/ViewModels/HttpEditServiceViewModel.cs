using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing HTTP service configuration.
/// </summary>
public class HttpEditServiceViewModel : ServiceEditViewModelBase<HttpServiceOptions>
{
    private readonly HttpServiceOptions _options;
    private string _serviceName;
    private string _baseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpEditServiceViewModel"/> class.
    /// </summary>
    public HttpEditServiceViewModel(string serviceName, HttpServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _baseUrl = options.BaseUrl;
    }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, HttpServiceOptions>? ServiceUpdated;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<HttpServiceOptions>? AdvancedConfigRequested;

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
    /// Current options being edited.
    /// </summary>
    public HttpServiceOptions Options => _options;

    /// <inheritdoc />
    protected override void OnSave()
    {
        _options.BaseUrl = BaseUrl;
        ServiceUpdated?.Invoke(ServiceName, _options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => Cancelled?.Invoke();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => AdvancedConfigRequested?.Invoke(_options);
}

