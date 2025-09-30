using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services.Common.Descriptors;

public sealed class MqttServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = ServiceDescriptorIds.Mqtt;

    public MqttServiceDescriptor(
        IServiceOptionsSerializer? optionsSerializer = null,
        IReadOnlyCollection<ServiceFactoryBinding>? factories = null)
        : base(
            DescriptorId,
            "MQTT",
            "Messaging",
            "Connect to MQTT brokers and manage topic subscriptions.",
            ServiceType.Mqtt,
            optionsSerializer,
            factories,
            new ServicePresentationMetadata(
                "📡",
                "#FFFAFAD2",
                "#FFDAA520",
                "MQTT"))
    {
    }
}
