using System;
using System.Collections.Generic;

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

    private static readonly Dictionary<string, ServiceType> LegacyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "MQ", ServiceType.Mqtt },
        { "MQTT", ServiceType.Mqtt },
        { "HT", ServiceType.Http },
        { "HTTP", ServiceType.Http },
        { "FT", ServiceType.Ftp },
        { "FTP", ServiceType.Ftp },
        { "FTP Server", ServiceType.Ftp },
        { "HD", ServiceType.Hid },
        { "HID", ServiceType.Hid },
        { "CV", ServiceType.Csv },
        { "CSV", ServiceType.Csv },
        { "CSV Creator", ServiceType.Csv },
        { "FO", ServiceType.FileObserver },
        { "File Observer", ServiceType.FileObserver },
        { "SC", ServiceType.Scp },
        { "SCP", ServiceType.Scp },
        { "TC", ServiceType.Tcp },
        { "TCP", ServiceType.Tcp },
        { "HB", ServiceType.Heartbeat },
        { "Heartbeat", ServiceType.Heartbeat }
    };

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
        if (value != null && LegacyMap.TryGetValue(value, out result))
        {
            return true;
        }

        return Enum.TryParse(value, true, out result);
    }

    /// <summary>
    /// Converts a <see cref="ServiceType"/> to one of its legacy display strings,
    /// defaulting to the short code if no legacy name exists.
    /// </summary>
    public static string ToLegacyString(this ServiceType type) =>
        type switch
        {
            ServiceType.Ftp => "FTP",
            ServiceType.Mqtt => "MQTT",
            ServiceType.Http => "HTTP",
            ServiceType.Tcp => "TCP",
            ServiceType.Hid => "HID",
            ServiceType.Csv => "CSV Creator",
            ServiceType.FileObserver => "File Observer",
            ServiceType.Scp => "SCP",
            ServiceType.Heartbeat => "Heartbeat",
            _ => type.ToCode()
        };
}

