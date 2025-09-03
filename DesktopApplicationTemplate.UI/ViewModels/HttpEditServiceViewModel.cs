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
    private string _baseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpEditServiceViewModel"/> class.
    /// </summary>
    public HttpEditServiceViewModel(IServiceRule rule, string serviceName, HttpServiceOptions options, ILoggingService? logger = null)
        : base(rule, logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _baseUrl = options.BaseUrl;
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
        RaiseServiceSaved(_options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => RaiseEditCancelled();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => RaiseAdvancedConfigRequested(_options);
}

