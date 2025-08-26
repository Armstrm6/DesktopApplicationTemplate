using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing MQTT service configuration.
/// </summary>
public class MqttEditServiceViewModel : MqttCreateServiceViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MqttEditServiceViewModel"/> class.
    /// </summary>
    public MqttEditServiceViewModel(string serviceName, MqttServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        ServiceName = serviceName;
        Host = options.Host;
        Port = options.Port;
        ClientId = options.ClientId;
        Username = options.Username;
        Password = options.Password;
        Options.Host = options.Host;
        Options.Port = options.Port;
        Options.ClientId = options.ClientId;
        Options.Username = options.Username;
        Options.Password = options.Password;
        Options.UseTls = options.UseTls;
        Options.ClientCertificate = options.ClientCertificate;
        Options.WillTopic = options.WillTopic;
        Options.WillPayload = options.WillPayload;
        Options.WillQualityOfService = options.WillQualityOfService;
        Options.WillRetain = options.WillRetain;
        Options.KeepAliveSeconds = options.KeepAliveSeconds;
        Options.CleanSession = options.CleanSession;
        Options.ReconnectDelay = options.ReconnectDelay;
    }

    /// <summary>
    /// Command for saving the updated configuration.
    /// </summary>
    public ICommand SaveCommand => CreateCommand;

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, MqttServiceOptions>? ServiceUpdated
    {
        add => ServiceCreated += value;
        remove => ServiceCreated -= value;
    }
}
