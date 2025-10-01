using System;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Factories
{
    public sealed class ServiceFactoryContext
    {
        public ServiceFactoryContext(string descriptorId, string serviceName, object? payload, IServiceDescriptor? descriptor = null)
        {
            DescriptorId = string.IsNullOrWhiteSpace(descriptorId)
                ? throw new ArgumentException("Descriptor ID cannot be null or whitespace", nameof(descriptorId))
                : descriptorId;
            ServiceName = serviceName ?? string.Empty;
            Payload = payload;
            Descriptor = descriptor;
        }

        public string DescriptorId { get; }

        public string ServiceName { get; }

        public object? Payload { get; }

        public IServiceDescriptor? Descriptor { get; }

        public TPayload? GetPayload<TPayload>() where TPayload : class => Payload as TPayload;
    }
}
