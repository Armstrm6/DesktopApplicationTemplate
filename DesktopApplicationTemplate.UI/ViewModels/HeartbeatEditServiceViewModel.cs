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
    private string _serviceName;
    private string _baseMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeartbeatEditServiceViewModel"/> class.
    /// </summary>
    public HeartbeatEditServiceViewModel(string serviceName, HeartbeatServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _baseMessage = options.BaseMessage;
    }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, HeartbeatServiceOptions>? ServiceUpdated;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<HeartbeatServiceOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
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
        ServiceUpdated?.Invoke(ServiceName, _options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => Cancelled?.Invoke();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => AdvancedConfigRequested?.Invoke(_options);
}

