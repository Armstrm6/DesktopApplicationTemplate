using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Converters;

/// <summary>
/// Converts <see cref="ServiceType"/> values to short string codes for persistence
/// and supports reading legacy names.
/// </summary>
public sealed class ServiceTypeJsonConverter : JsonConverter<ServiceType>
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

    public override ServiceType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return TryParse(value, out var result) ? result : default;
    }

    public override void Write(Utf8JsonWriter writer, ServiceType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(ToCode(value));
    }

    public static bool TryParse(string? value, out ServiceType result)
    {
        if (value != null && LegacyMap.TryGetValue(value, out result))
        {
            return true;
        }

        return Enum.TryParse(value, true, out result);
    }

    public static string ToCode(ServiceType type) =>
        CodeMap.TryGetValue(type, out var code) ? code : type.ToString();

    public static string ToLegacyString(ServiceType type)
    {
        // Return one of the legacy names for display, defaulting to code.
        return type switch
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
            _ => ToCode(type)
        };
    }
}
