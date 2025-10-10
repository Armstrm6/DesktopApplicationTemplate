using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Imports plug-in packages and updates the service catalog.
/// </summary>
public interface IPluginImportService
{
    /// <summary>
    /// Imports the specified plug-in archive.
    /// </summary>
    /// <param name="sourcePath">The source archive path.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The result of the import operation.</returns>
    Task<PluginImportResult> ImportAsync(string sourcePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the outcome of a plug-in import operation.
/// </summary>
/// <param name="Success">Indicates whether the import succeeded.</param>
/// <param name="Message">A user-friendly message describing the outcome.</param>
/// <param name="DestinationPath">The destination path of the imported payload, if any.</param>
/// <param name="ImportedDescriptors">Descriptors discovered during the import.</param>
public sealed record PluginImportResult(
    bool Success,
    string Message,
    string? DestinationPath,
    IReadOnlyCollection<IServiceDescriptor> ImportedDescriptors)
{
    public IReadOnlyCollection<ServiceUiRegistration<ServiceListModel, Page>> ImportedUiRegistrations { get; init; }
        = Array.Empty<ServiceUiRegistration<ServiceListModel, Page>>();
}
