using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        internal static UserSettings Load(ILoggingService? logger)
        {
            if (!File.Exists(FilePath))
            {
                return new UserSettings();
            }

            try
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
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
        internal static void Save(UserSettings settings, ILoggingService? logger)
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
                File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, options));
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
                File.WriteAllText(temp, dump);
                Environment.FailFast($"Stack overflow while saving settings. Dump written to {temp}");
            }
        }
    }
}

