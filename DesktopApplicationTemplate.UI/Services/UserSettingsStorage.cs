using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Provides centralized serialization for user settings so multiple UI components
    /// can load and persist preferences without duplicating file handling logic.
    /// </summary>
    internal static class UserSettingsStorage
    {
        internal static string FilePath { get; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userSettings.json");

        /// <summary>
        /// Loads settings from disk, returning a default instance when the file is missing
        /// or cannot be read.
        /// </summary>
        internal static async Task<UserSettings> LoadAsync(ILoggingService? logger, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return new UserSettings();
            }

            try
            {
                await using var stream = new FileStream(
                    FilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true);

                var settings = await JsonSerializer.DeserializeAsync<UserSettings>(stream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                return settings ?? new UserSettings();
            }
            catch (IOException ex)
            {
                logger?.Log($"Failed to load settings from '{FilePath}'. Using default settings. Error: {ex}", LogLevel.Error);
                return new UserSettings();
            }
        }

        /// <summary>
        /// Saves the provided settings to disk.
        /// </summary>
        internal static async Task SaveAsync(UserSettings settings, ILoggingService? logger, CancellationToken cancellationToken = default)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            try
            {
                await using var stream = new FileStream(
                    FilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    useAsync: true);

                await JsonSerializer.SerializeAsync(stream, settings, options, cancellationToken).ConfigureAwait(false);
            }
            catch (StackOverflowException)
            {
                var dumpOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                var dump = JsonSerializer.Serialize(settings, dumpOptions);
                var temp = Path.Combine(Path.GetTempPath(), "settings_dump.json");
                await File.WriteAllTextAsync(temp, dump, cancellationToken).ConfigureAwait(false);
                Environment.FailFast($"Stack overflow while saving settings. Dump written to {temp}");
            }
        }
    }
}

