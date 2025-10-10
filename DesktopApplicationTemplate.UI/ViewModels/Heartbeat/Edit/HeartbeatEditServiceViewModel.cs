using System;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;

namespace DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Edit;

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
    protected override Task OnSaveAsync()
    {
        _options.BaseMessage = BaseMessage;
        return RaiseServiceSavedAsync(_options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => RaiseEditCancelled();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => RaiseAdvancedConfigRequested(_options);
}

