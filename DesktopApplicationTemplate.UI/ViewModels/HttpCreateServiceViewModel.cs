using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

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
        : base(rule, logger)
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
        RaiseServiceSaved(Options);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        Logger?.Log("HTTP create cancelled", LogLevel.Debug);
        RaiseEditCancelled();
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Logger?.Log("Opening HTTP advanced config", LogLevel.Debug);
        Options.BaseUrl = BaseUrl;
        RaiseAdvancedConfigRequested(Options);
    }
}
