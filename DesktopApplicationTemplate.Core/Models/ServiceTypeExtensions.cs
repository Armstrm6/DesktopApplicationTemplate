using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopApplicationTemplate.Models;

/// <summary>
/// Provides helpers for converting <see cref="ServiceType"/> values to and from
/// short string codes used in persistence and configuration.
/// </summary>
public static class ServiceTypeExtensions
{
    private static readonly Dictionary<ServiceType, string> CodeMap = new()
    {
        { ServiceType.Mqtt, "MQ" },
        { ServiceType.Http, "HT" },
        { ServiceType.Ftp, "FT" },
        { ServiceType.Hid, "HD" },
        { ServiceType.Csv, "CV" },
        { ServiceType.FileObserver, "FO" },
        { ServiceType.Scp, "SC" },
        { ServiceType.Tcp, "TC" },
        { ServiceType.Heartbeat, "HB" }
    };

    private static readonly Dictionary<string, ServiceType> CodeLookup =
        CodeMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Converts a <see cref="ServiceType"/> to its short code representation.
    /// </summary>
    public static string ToCode(this ServiceType type) =>
        CodeMap.TryGetValue(type, out var code) ? code : type.ToString();

    /// <summary>
    /// Tries to parse a short code or legacy name into a <see cref="ServiceType"/>.
    /// </summary>
    public static bool TryParse(string? value, out ServiceType result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();

        if (CodeLookup.TryGetValue(trimmed, out result))
        {
            return true;
        }

        var normalized = trimmed
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        if (!string.Equals(normalized, trimmed, StringComparison.Ordinal) &&
            Enum.TryParse(normalized, true, out result))
        {
            return true;
        }

        return Enum.TryParse(trimmed, true, out result);
    }

    /// <summary>
    /// Provides the default prefix used when generating service names.
    /// </summary>
    public static string ToBaseName(this ServiceType type) =>
        type switch
        {
            ServiceType.Ftp => "FTP",
            ServiceType.Mqtt => "MQTT",
            ServiceType.Http => "HTTP",
            ServiceType.Tcp => "TCP",
            ServiceType.Hid => "HID",
            ServiceType.Csv => "CSV",
            ServiceType.FileObserver => "FileObserver",
            ServiceType.Scp => "SCP",
            ServiceType.Heartbeat => "Heartbeat",
            _ => type.ToCode()
        };
}

