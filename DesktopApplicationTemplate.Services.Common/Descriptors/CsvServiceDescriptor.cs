using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services.Common.Descriptors;

public sealed class CsvServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = ServiceDescriptorIds.Csv;

    public CsvServiceDescriptor(
        IServiceOptionsSerializer? optionsSerializer = null,
        IReadOnlyCollection<ServiceFactoryBinding>? factories = null)
        : base(
            DescriptorId,
            "CSV Creator",
            "File",
            "Generate CSV output from message payloads.",
            ServiceType.Csv,
            optionsSerializer,
            factories)
    {
    }
}
