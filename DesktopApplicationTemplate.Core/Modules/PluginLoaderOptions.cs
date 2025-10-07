using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace DesktopApplicationTemplate.Core.Modules;

/// <summary>
/// Provides configuration for the <see cref="PluginLoader"/>.
/// </summary>
public sealed class PluginLoaderOptions
{
    public PluginLoaderOptions(string rootDirectory, string extractionDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            throw new ArgumentException("Root directory must be provided", nameof(rootDirectory));
        }

        if (string.IsNullOrWhiteSpace(extractionDirectory))
        {
            throw new ArgumentException("Extraction directory must be provided", nameof(extractionDirectory));
        }

        RootDirectory = EnsureAbsolutePath(rootDirectory);
        ExtractionDirectory = EnsureAbsolutePath(extractionDirectory);
    }

    /// <summary>
    /// Gets the directory that contains plug-in packages.
    /// </summary>
    public string RootDirectory { get; }

    /// <summary>
    /// Gets the directory used to extract plug-in archives.
    /// </summary>
    public string ExtractionDirectory { get; }

    /// <summary>
    /// Gets the full path for a package residing in the plug-in root directory.
    /// Ensures the directory exists.
    /// </summary>
    /// <param name="fileName">The package file name.</param>
    /// <returns>The absolute package path.</returns>
    public string GetPackagePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name must be provided", nameof(fileName));
        }

        Directory.CreateDirectory(RootDirectory);
        return Path.Combine(RootDirectory, fileName);
    }

    /// <summary>
    /// Calculates the extraction path for the provided manifest.
    /// </summary>
    public string GetExtractionPath(PluginManifest manifest)
    {
        return PluginPackageUtilities.GetExtractionPath(this, manifest);
    }

    /// <summary>
    /// Gets the manifest path for the specified plug-in directory.
    /// </summary>
    public string GetManifestPath(string pluginDirectory)
    {
        return PluginPackageUtilities.GetManifestPath(pluginDirectory);
    }

    /// <summary>
    /// Creates options using the provided configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The options instance.</returns>
    public static PluginLoaderOptions FromConfiguration(IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var baseDirectory = AppContext.BaseDirectory;
        var root = configuration["Plugins:Directory"];
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.Combine(baseDirectory, "Plugins");
        }
        else if (!Path.IsPathRooted(root))
        {
            root = Path.Combine(baseDirectory, root);
        }

        var extractionRoot = configuration["Plugins:ExtractionRoot"];
        if (string.IsNullOrWhiteSpace(extractionRoot))
        {
            extractionRoot = Path.Combine(baseDirectory, "PluginCache");
        }
        else if (!Path.IsPathRooted(extractionRoot))
        {
            extractionRoot = Path.Combine(baseDirectory, extractionRoot);
        }

        return new PluginLoaderOptions(root, extractionRoot);
    }

    private static string EnsureAbsolutePath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(fullPath);
        return fullPath;
    }
}
