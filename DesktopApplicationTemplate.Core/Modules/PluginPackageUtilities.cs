using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DesktopApplicationTemplate.Core.Modules;

/// <summary>
/// Provides helper methods for working with plug-in package manifests and payloads.
/// </summary>
public static class PluginPackageUtilities
{
    /// <summary>
    /// Gets the manifest file name used by plug-in packages.
    /// </summary>
    public const string ManifestFileName = "plugin.manifest.json";

    /// <summary>
    /// Gets the JSON serializer options used for manifests.
    /// </summary>
    public static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    /// <summary>
    /// Builds a manifest path within the provided directory.
    /// </summary>
    public static string GetManifestPath(string pluginDirectory)
    {
        if (string.IsNullOrWhiteSpace(pluginDirectory))
        {
            throw new ArgumentException("Directory must be provided", nameof(pluginDirectory));
        }

        return Path.Combine(pluginDirectory, ManifestFileName);
    }

    /// <summary>
    /// Validates required manifest fields.
    /// </summary>
    public static bool TryValidateBasicManifest(PluginManifest? manifest, string origin, out List<string> errors)
    {
        errors = new List<string>();
        if (manifest is null)
        {
            errors.Add(FormattableString.Invariant($"Plug-in manifest in '{origin}' could not be parsed."));
            return false;
        }

        if (string.IsNullOrWhiteSpace(manifest.Id))
        {
            errors.Add(FormattableString.Invariant($"Plug-in manifest in '{origin}' is missing 'id'."));
        }

        if (string.IsNullOrWhiteSpace(manifest.Version))
        {
            errors.Add(FormattableString.Invariant($"Plug-in manifest in '{origin}' is missing 'version'."));
        }
        else if (!Version.TryParse(manifest.Version, out _))
        {
            errors.Add(FormattableString.Invariant($"Plug-in manifest in '{origin}' specifies an invalid version '{manifest.Version}'."));
        }

        if (string.IsNullOrWhiteSpace(manifest.EntryAssembly))
        {
            errors.Add(FormattableString.Invariant($"Plug-in manifest in '{origin}' is missing 'entryAssembly'."));
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Validates manifest entries that depend on the on-disk plug-in directory.
    /// </summary>
    public static bool TryValidateManifest(PluginManifest? manifest, string pluginDirectory, string origin, out List<string> errors)
    {
        if (!TryValidateBasicManifest(manifest, origin, out errors))
        {
            return false;
        }

        if (manifest is null)
        {
            return false;
        }

        var entryAssembly = ResolveRelativePath(pluginDirectory, manifest.EntryAssembly);
        if (entryAssembly is null)
        {
            errors.Add(FormattableString.Invariant($"Entry assembly '{manifest.EntryAssembly}' resolves outside '{pluginDirectory}'."));
        }
        else if (!File.Exists(entryAssembly))
        {
            errors.Add(FormattableString.Invariant($"Entry assembly '{manifest.EntryAssembly}' was not found at '{entryAssembly}'."));
        }

        foreach (var assemblyReference in manifest.ServiceAssemblies)
        {
            var resolved = ResolveRelativePath(pluginDirectory, assemblyReference);
            if (resolved is null)
            {
                errors.Add(FormattableString.Invariant($"Service assembly '{assemblyReference}' resolves outside '{pluginDirectory}'."));
                continue;
            }

            if (!File.Exists(resolved))
            {
                errors.Add(FormattableString.Invariant($"Service assembly '{assemblyReference}' was not found at '{resolved}'."));
            }
        }

        foreach (var probingPath in manifest.ProbingPaths)
        {
            if (ResolveRelativePath(pluginDirectory, probingPath) is null)
            {
                errors.Add(FormattableString.Invariant($"Probing path '{probingPath}' resolves outside '{pluginDirectory}'."));
            }
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Creates a stable key for identifying a manifest payload.
    /// </summary>
    public static string CreateManifestKey(PluginManifest manifest)
    {
        if (manifest is null)
        {
            throw new ArgumentNullException(nameof(manifest));
        }

        return FormattableString.Invariant($"{manifest.Id.Trim()}|{manifest.Version.Trim()}");
    }

    /// <summary>
    /// Calculates the extraction path for a manifest using the configured options.
    /// </summary>
    public static string GetExtractionPath(PluginLoaderOptions options, PluginManifest manifest)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (manifest is null)
        {
            throw new ArgumentNullException(nameof(manifest));
        }

        var safeId = SanitizeSegment(manifest.Id);
        var safeVersion = SanitizeSegment(manifest.Version);
        return Path.Combine(options.ExtractionDirectory, safeId, safeVersion);
    }

    /// <summary>
    /// Resolves a relative path inside a plug-in directory.
    /// Returns null when the path escapes the root.
    /// </summary>
    public static string? ResolveRelativePath(string root, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var combined = Path.GetFullPath(Path.Combine(root, relativePath));
        var rootPath = EnsureTrailingSeparator(Path.GetFullPath(root));
        if (!combined.StartsWith(rootPath, PathComparison))
        {
            return null;
        }

        return combined;
    }

    /// <summary>
    /// Ensures a path ends with the system directory separator character.
    /// </summary>
    public static string EnsureTrailingSeparator(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))
        {
            return path;
        }

        return path + Path.DirectorySeparatorChar;
    }

    /// <summary>
    /// Replaces invalid file name characters with underscores.
    /// </summary>
    public static string SanitizeSegment(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var invalid = Path.GetInvalidFileNameChars();
        var buffer = value.ToCharArray();
        for (var i = 0; i < buffer.Length; i++)
        {
            if (invalid.Contains(buffer[i]))
            {
                buffer[i] = '_';
            }
        }

        return new string(buffer);
    }

    /// <summary>
    /// Determines whether <paramref name="candidate"/> is contained within <paramref name="root"/>.
    /// </summary>
    public static bool IsPathWithinRoot(string root, string candidate)
    {
        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        var fullRoot = EnsureTrailingSeparator(Path.GetFullPath(root));
        var fullCandidate = Path.GetFullPath(candidate);
        return fullCandidate.StartsWith(fullRoot, PathComparison);
    }

    /// <summary>
    /// Determines whether two paths reference the same location using platform-specific comparison rules.
    /// </summary>
    public static bool PathsEqual(string first, string second)
    {
        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
        {
            return false;
        }

        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(Path.GetFullPath(first), Path.GetFullPath(second), comparison);
    }
}
