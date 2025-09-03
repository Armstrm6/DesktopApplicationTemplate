using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

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
        : base(rule, logger)
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

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public HeartbeatServiceOptions Options { get; } = new();

    /// <inheritdoc />
    protected override void OnSave()
    {
        Logger?.Log("Heartbeat create options start", LogLevel.Debug);
        Options.BaseMessage = BaseMessage;
        Logger?.Log("Heartbeat create options finished", LogLevel.Debug);
        RaiseServiceSaved(Options);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        Logger?.Log("Heartbeat create cancelled", LogLevel.Debug);
        RaiseEditCancelled();
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Logger?.Log("Heartbeat advanced config requested", LogLevel.Debug);
        Options.BaseMessage = BaseMessage;
        RaiseAdvancedConfigRequested(Options);
    }
}
