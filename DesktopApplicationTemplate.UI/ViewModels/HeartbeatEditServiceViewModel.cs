using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing Heartbeat service configuration.
/// </summary>
public class HeartbeatEditServiceViewModel : HeartbeatCreateServiceViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeartbeatEditServiceViewModel"/> class.
    /// </summary>
    public HeartbeatEditServiceViewModel(string serviceName, HeartbeatServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        ServiceName = serviceName;
        BaseMessage = options.BaseMessage;
        Options.BaseMessage = options.BaseMessage;
        Options.IncludePing = options.IncludePing;
        Options.IncludeStatus = options.IncludeStatus;
    }

    /// <summary>
    /// Command for saving the updated configuration.
    /// </summary>
    public ICommand SaveCommand => CreateCommand;

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, HeartbeatServiceOptions>? ServiceUpdated
    {
        add => ServiceCreated += value;
        remove => ServiceCreated -= value;
    }
}
