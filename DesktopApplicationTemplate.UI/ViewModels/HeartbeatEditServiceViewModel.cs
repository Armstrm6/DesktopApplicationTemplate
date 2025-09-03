using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing Heartbeat service configuration.
/// </summary>
public class HeartbeatEditServiceViewModel : ServiceEditViewModelBase<HeartbeatServiceOptions>
{
    private readonly HeartbeatServiceOptions _options;
    private string _baseMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeartbeatEditServiceViewModel"/> class.
    /// </summary>
    public HeartbeatEditServiceViewModel(IServiceRule rule, string serviceName, HeartbeatServiceOptions options, ILoggingService? logger = null)
        : base(rule, logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _baseMessage = options.BaseMessage;
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
    protected override void OnSave()
    {
        _options.BaseMessage = BaseMessage;
        RaiseServiceSaved(_options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => RaiseEditCancelled();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => RaiseAdvancedConfigRequested(_options);
}

