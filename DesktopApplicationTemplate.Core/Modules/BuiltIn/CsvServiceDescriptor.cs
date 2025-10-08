using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Models;
using ModelsServiceType = DesktopApplicationTemplate.Models.ServiceType;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Describes the built-in CSV logging service.
/// </summary>
public sealed class CsvServiceDescriptor : BuiltInServiceDescriptor<CsvServiceOptions>
{
    public CsvServiceDescriptor()
        : base(
            ServiceDescriptorIds.Csv,
            "CSV Creator",
            BuiltInServiceCategories.DataProcessing,
            ModelsServiceType.Csv,
            new ServicePresentationMetadata(
                "📄",
                "LightGray",
                "Gray",
                "CSV"))
    {
    }
}
