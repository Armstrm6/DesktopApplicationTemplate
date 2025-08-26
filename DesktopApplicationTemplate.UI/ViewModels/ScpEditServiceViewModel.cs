using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing SCP service configuration.
/// </summary>
public class ScpEditServiceViewModel : ScpCreateServiceViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScpEditServiceViewModel"/> class.
    /// </summary>
    public ScpEditServiceViewModel(string serviceName, ScpServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        ServiceName = serviceName;
        Host = options.Host;
        Port = options.Port.ToString();
        Username = options.Username;
        Password = options.Password;
        Options.Host = options.Host;
        Options.Port = options.Port;
        Options.Username = options.Username;
        Options.Password = options.Password;
        Options.LocalPath = options.LocalPath;
        Options.RemotePath = options.RemotePath;
    }

    /// <summary>
    /// Command for saving the updated configuration.
    /// </summary>
    public ICommand SaveCommand => CreateCommand;

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, ScpServiceOptions>? ServiceUpdated
    {
        add => ServiceCreated += value;
        remove => ServiceCreated -= value;
    }
}
