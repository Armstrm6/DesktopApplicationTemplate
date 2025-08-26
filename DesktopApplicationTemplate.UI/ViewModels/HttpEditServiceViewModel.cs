using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing HTTP service configuration.
/// </summary>
public class HttpEditServiceViewModel : HttpCreateServiceViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpEditServiceViewModel"/> class.
    /// </summary>
    public HttpEditServiceViewModel(string serviceName, HttpServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        ServiceName = serviceName;
        BaseUrl = options.BaseUrl;
        Options.BaseUrl = options.BaseUrl;
        Options.Username = options.Username;
        Options.Password = options.Password;
        Options.ClientCertificatePath = options.ClientCertificatePath;
    }

    /// <summary>
    /// Command for saving the updated configuration.
    /// </summary>
    public ICommand SaveCommand => CreateCommand;

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, HttpServiceOptions>? ServiceUpdated
    {
        add => ServiceCreated += value;
        remove => ServiceCreated -= value;
    }
}
