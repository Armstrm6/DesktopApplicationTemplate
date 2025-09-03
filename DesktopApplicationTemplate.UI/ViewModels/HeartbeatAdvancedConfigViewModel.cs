using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced Heartbeat configuration.
/// </summary>
public class HeartbeatAdvancedConfigViewModel : AdvancedConfigViewModelBase<HeartbeatServiceOptions>
{
    private readonly HeartbeatServiceOptions _options;
    private bool _includePing;
    private bool _includeStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeartbeatAdvancedConfigViewModel"/> class.
    /// </summary>
    public HeartbeatAdvancedConfigViewModel(HeartbeatServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _includePing = options.IncludePing;
        _includeStatus = options.IncludeStatus;
    }

    /// <summary>
    /// Whether to include a ping indicator in the heartbeat.
    /// </summary>
    public bool IncludePing
    {
        get => _includePing;
        set { _includePing = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether to include a status indicator in the heartbeat.
    /// </summary>
    public bool IncludeStatus
    {
        get => _includeStatus;
        set { _includeStatus = value; OnPropertyChanged(); }
    }

    protected override HeartbeatServiceOptions OnSave()
    {
        Logger?.Log("Heartbeat advanced options start", LogLevel.Debug);
        _options.IncludePing = IncludePing;
        _options.IncludeStatus = IncludeStatus;
        Logger?.Log("Heartbeat advanced options finished", LogLevel.Debug);
        return _options;
    }

    protected override void OnBack()
    {
        Logger?.Log("Heartbeat advanced options back", LogLevel.Debug);
    }
}
