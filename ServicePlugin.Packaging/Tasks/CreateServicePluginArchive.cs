using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ServicePlugin.Packaging.Tasks;

/// <summary>
/// Creates distributable plug-in archives containing descriptors, dependencies, and manifests.
/// </summary>
public sealed class CreateServicePluginArchive : Task
{
    private static readonly char[] ExtensionSeparators = [ ';' ];

    [Required]
    public ITaskItem[] Files { get; set; } = Array.Empty<ITaskItem>();

    [Required]
    public string OutputDirectory { get; set; } = string.Empty;

    [Required]
    public string PackageId { get; set; } = string.Empty;

    [Required]
    public string PackageVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the archive extensions (e.g. ".ccp;.chapp").
    /// </summary>
    [Required]
    public string ArchiveExtensions { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional timestamp format appended to package versions.
    /// </summary>
    public string? BuildMetadataTimestampFormat { get; set; }

    [Output]
    public ITaskItem[] CreatedPackages { get; private set; } = Array.Empty<ITaskItem>();

    public override bool Execute()
    {
        if (Files.Length == 0)
        {
            Log.LogError("No files were supplied to the plug-in packager.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(PackageId))
        {
            Log.LogError("Service plug-in packages require a PackageId.");
            return false;
        }

        var version = string.IsNullOrWhiteSpace(PackageVersion) ? "1.0.0" : PackageVersion.Trim();
        if (!string.IsNullOrWhiteSpace(BuildMetadataTimestampFormat))
        {
            var timestamp = DateTimeOffset.UtcNow.ToString(BuildMetadataTimestampFormat, CultureInfo.InvariantCulture);
            version = FormattableString.Invariant($"{version}+{timestamp}");
        }

        var extensions = ParseExtensions();
        if (extensions.Count == 0)
        {
            Log.LogError("At least one archive extension must be specified.");
            return false;
        }

        var manifestFound = false;
        var packages = new List<ITaskItem>();

        foreach (var extension in extensions)
        {
            var packagePath = CreateArchive(version, extension, ref manifestFound);
            if (packagePath is null)
            {
                return false;
            }

            packages.Add(new TaskItem(packagePath));
        }

        if (!manifestFound)
        {
            Log.LogError("Plug-in packages must include a manifest. Mark the manifest item with metadata 'Kind=Manifest'.");
            return false;
        }

        CreatedPackages = packages.ToArray();
        return !Log.HasLoggedErrors;
    }

    private IReadOnlyCollection<string> ParseExtensions()
    {
        var entries = (ArchiveExtensions ?? string.Empty)
            .Split(ExtensionSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .DefaultIfEmpty(".ccp")
            .Select(NormalizeExtension)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return entries;
    }

    private static string NormalizeExtension(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ".ccp";
        }

        return value.StartsWith('.') ? value : $".{value}";
    }

    private string? CreateArchive(string version, string extension, ref bool manifestFound)
    {
        try
        {
            Directory.CreateDirectory(OutputDirectory);
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, showStackTrace: true);
            return null;
        }

        var archiveFileName = $"{PackageId}-{version}{extension}";
        var archivePath = Path.Combine(OutputDirectory, archiveFileName);

        var stagingRoot = Path.Combine(Path.GetTempPath(), $"ServicePlugin_{Guid.NewGuid():N}");
        Directory.CreateDirectory(stagingRoot);

        try
        {
            foreach (var file in Files)
            {
                var sourcePath = file.ItemSpec;
                if (!File.Exists(sourcePath))
                {
                    Log.LogError($"Packaged file '{sourcePath}' does not exist.");
                    return null;
                }

                var packagePath = file.GetMetadata("PackagePath");
                if (string.IsNullOrWhiteSpace(packagePath))
                {
                    packagePath = Path.GetFileName(sourcePath);
                }

                if (string.Equals(file.GetMetadata("Kind"), "Manifest", StringComparison.OrdinalIgnoreCase))
                {
                    manifestFound = true;
                }

                var destinationPath = Path.Combine(stagingRoot, packagePath);
                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                File.Copy(sourcePath, destinationPath, overwrite: true);
            }

            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            ZipFile.CreateFromDirectory(stagingRoot, archivePath, CompressionLevel.Optimal, includeBaseDirectory: false);
            Log.LogMessage(MessageImportance.High, $"Created service plug-in archive: {archivePath}");
            return archivePath;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, showStackTrace: true);
            return null;
        }
        finally
        {
            try
            {
                if (Directory.Exists(stagingRoot))
                {
                    Directory.Delete(stagingRoot, recursive: true);
                }
            }
            catch (Exception cleanupEx)
            {
                Log.LogWarning($"Failed to clean staging directory '{stagingRoot}': {cleanupEx.Message}");
            }
        }
    }
}
