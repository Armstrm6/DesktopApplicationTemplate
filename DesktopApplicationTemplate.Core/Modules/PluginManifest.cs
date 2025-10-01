using System.Collections.Generic;

namespace DesktopApplicationTemplate.Core.Modules;

/// <summary>
/// Represents the metadata that describes a plug-in package.
/// </summary>
public sealed class PluginManifest
{
    /// <summary>
    /// Gets or sets the unique identifier for the plug-in.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-friendly display name for the plug-in.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the semantic version of the plug-in.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary assembly that defines the plug-in entry point.
    /// </summary>
    public string EntryAssembly { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assemblies that should be scanned for <see cref="IServiceModule"/> implementations.
    /// When empty, <see cref="EntryAssembly"/> is used.
    /// </summary>
    public List<string> ServiceAssemblies { get; set; } = new();

    /// <summary>
    /// Gets or sets optional probing paths, relative to the plug-in root, where dependencies should be resolved from.
    /// </summary>
    public List<string> ProbingPaths { get; set; } = new();
}
