using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Provides operations for packaging plug-in payloads.
/// </summary>
public interface IPluginExportService
{
    /// <summary>
    /// Gets descriptors that can be exported from the current environment.
    /// </summary>
    IReadOnlyCollection<PluginDescriptorExportInfo> GetExportableDescriptors();

    /// <summary>
    /// Exports the selected descriptors into a plug-in package.
    /// </summary>
    Task<PluginExportResult> ExportAsync(PluginExportRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides descriptor metadata displayed in the export workflow.
/// </summary>
/// <param name="Id">The descriptor identifier.</param>
/// <param name="DisplayName">The human-friendly display name.</param>
/// <param name="Category">The descriptor category.</param>
public sealed record PluginDescriptorExportInfo(string Id, string DisplayName, string Category);

/// <summary>
/// Describes export parameters supplied by the UI.
/// </summary>
/// <param name="PluginId">Unique identifier written to the manifest.</param>
/// <param name="PluginName">Display name written to the manifest.</param>
/// <param name="Version">Semantic version recorded in the manifest.</param>
/// <param name="DescriptorIds">Descriptors that should be included.</param>
/// <param name="DestinationPath">Optional destination path for the package.</param>
public sealed record PluginExportRequest(
    string PluginId,
    string PluginName,
    string Version,
    IReadOnlyCollection<string> DescriptorIds,
    string? DestinationPath);

/// <summary>
/// Represents the result of an export operation.
/// </summary>
/// <param name="Success">Indicates whether the export succeeded.</param>
/// <param name="Message">User-friendly status message.</param>
/// <param name="PackagePath">Path to the generated package when successful.</param>
/// <param name="Descriptors">Descriptors included in the package.</param>
public sealed record PluginExportResult(
    bool Success,
    string Message,
    string? PackagePath,
    IReadOnlyCollection<string> Descriptors);
