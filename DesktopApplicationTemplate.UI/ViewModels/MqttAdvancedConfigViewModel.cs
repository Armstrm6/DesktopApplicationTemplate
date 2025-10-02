using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced MQTT configuration.
/// </summary>
public class MqttAdvancedConfigViewModel : AdvancedConfigViewModelBase<MqttServiceOptions>
{
    private readonly MqttServiceOptions _options;
    private string? _clientCertificatePath;
    private string? _willTopic;
    private string? _willPayload;
    private MqttQualityOfServiceLevel _willQualityOfService = MqttQualityOfServiceLevel.AtMostOnce;
    private bool _willRetain;
    private ushort _keepAliveSeconds = 60;
    private bool _cleanSession = true;
    private int _reconnectDelaySeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttAdvancedConfigViewModel"/> class.
    /// </summary>
    public MqttAdvancedConfigViewModel(MqttServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _willTopic = options.WillTopic;
        _willPayload = options.WillPayload;
        _willQualityOfService = options.WillQualityOfService;
        _willRetain = options.WillRetain;
        _keepAliveSeconds = options.KeepAliveSeconds;
        _cleanSession = options.CleanSession;
        _reconnectDelaySeconds = options.ReconnectDelay?.Seconds ?? 0;
        QoSLevels = Enum.GetValues(typeof(MqttQualityOfServiceLevel)).Cast<MqttQualityOfServiceLevel>().ToArray();
    }

    /// <summary>
    /// Available MQTT quality of service levels.
    /// </summary>
    public IReadOnlyList<MqttQualityOfServiceLevel> QoSLevels { get; }

    /// <summary>
    /// Path to the client certificate used for TLS authentication.
    /// </summary>
    public string? ClientCertificatePath
    {
        get => _clientCertificatePath;
        set { _clientCertificatePath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Will topic published on unexpected disconnect.
    /// </summary>
    public string? WillTopic
    {
        get => _willTopic;
        set { _willTopic = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Will message payload.
    /// </summary>
    public string? WillPayload
    {
        get => _willPayload;
        set { _willPayload = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Quality of service level for the will message.
    /// </summary>
    public MqttQualityOfServiceLevel WillQualityOfService
    {
        get => _willQualityOfService;
        set { _willQualityOfService = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether the will message should be retained.
    /// </summary>
    public bool WillRetain
    {
        get => _willRetain;
        set { _willRetain = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Keep alive interval in seconds.
    /// </summary>
    public ushort KeepAliveSeconds
    {
        get => _keepAliveSeconds;
        set { _keepAliveSeconds = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether a clean session should be used.
    /// </summary>
    public bool CleanSession
    {
        get => _cleanSession;
        set { _cleanSession = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Reconnect delay in seconds.
    /// </summary>
    public int ReconnectDelaySeconds
    {
        get => _reconnectDelaySeconds;
        set { _reconnectDelaySeconds = value; OnPropertyChanged(); }
    }

    protected override MqttServiceOptions OnSave()
    {
        Logger?.Log("MQTT advanced options start", LogLevel.Debug);
        _options.WillTopic = string.IsNullOrWhiteSpace(WillTopic) ? null : WillTopic;
        _options.WillPayload = string.IsNullOrWhiteSpace(WillPayload) ? null : WillPayload;
        _options.WillQualityOfService = WillQualityOfService;
        _options.WillRetain = WillRetain;
        _options.KeepAliveSeconds = KeepAliveSeconds;
        _options.CleanSession = CleanSession;
        _options.ReconnectDelay = ReconnectDelaySeconds > 0 ? TimeSpan.FromSeconds(ReconnectDelaySeconds) : null;
        if (!string.IsNullOrWhiteSpace(ClientCertificatePath) && File.Exists(ClientCertificatePath))
        {
            _options.ClientCertificate = File.ReadAllBytes(ClientCertificatePath);
        }
        else
        {
            _options.ClientCertificate = null;
        }
        Logger?.Log("MQTT advanced options finished", LogLevel.Debug);
        return _options;
    }

    protected override void OnBack()
    {
        Logger?.Log("MQTT advanced options back", LogLevel.Debug);
    }
}
