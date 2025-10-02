using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;

namespace DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Create;

/// <summary>
/// View model for creating a new Heartbeat service.
/// </summary>
public class HeartbeatCreateServiceViewModel : ServiceCreateViewModelBase<HeartbeatServiceOptions>
{
    private string _baseMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeartbeatCreateServiceViewModel"/> class.
    /// </summary>
    public HeartbeatCreateServiceViewModel(IServiceRule rule, ILoggingService? logger = null)
        : base(rule, logger: logger)
    {
    }

    /// <summary>
    /// Base message for the heartbeat.
    /// </summary>
    public string BaseMessage
    {
        get => _baseMessage;
        set { _baseMessage = value; OnPropertyChanged(); }
    }

    /// <inheritdoc />
    protected override void ApplyOptions(HeartbeatServiceOptions options)
    {
        options.BaseMessage = BaseMessage;
    }
}
