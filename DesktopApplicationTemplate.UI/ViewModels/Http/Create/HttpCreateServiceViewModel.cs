using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;

namespace DesktopApplicationTemplate.UI.ViewModels.Http.Create;

/// <summary>
/// View model for configuring a new HTTP service.
/// </summary>
public class HttpCreateServiceViewModel : ServiceCreateViewModelBase<HttpServiceOptions>
{
    private string _baseUrl = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpCreateServiceViewModel"/> class.
    /// </summary>
    public HttpCreateServiceViewModel(IServiceRule rule, ILoggingService? logger = null)
        : base(rule, logger: logger)
    {
    }

    /// <summary>
    /// Base URL for requests.
    /// </summary>
    public string BaseUrl
    {
        get => _baseUrl;
        set { _baseUrl = value; OnPropertyChanged(); }
    }

    /// <inheritdoc />
    protected override void ApplyOptions(HttpServiceOptions options)
    {
        options.BaseUrl = BaseUrl;
    }
}
