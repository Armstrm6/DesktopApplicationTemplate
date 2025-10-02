using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Csv.Serialization;

namespace DesktopApplicationTemplate.Services.Csv.Descriptors;

/// <summary>
/// Describes the CSV creator service for the catalog.
/// </summary>
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
            optionsSerializer ?? new CsvServiceOptionsSerializer(),
            factories,
            new ServicePresentationMetadata(
                "📄",
                "#FFD3D3D3",
                "#FF808080",
                "CSV Creator"))
    {
    }
}
