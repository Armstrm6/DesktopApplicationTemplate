namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// Represents the different views for the MQTT service.
    /// </summary>
    public enum MqttServiceViewMode
    {
        /// <summary>Initial view for configuring a new service.</summary>
        CreateService,
        /// <summary>View showing tag subscriptions after creation.</summary>
        TagSubscriptions,
        /// <summary>View for editing an existing connection.</summary>
        EditConnection
    }
}
