using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Converters;

/// <summary>
/// Converts <see cref="ServiceType"/> values to short codes for persistence
/// and parses canonical identifiers.
/// </summary>
public sealed class ServiceTypeJsonConverter : JsonConverter<ServiceType>
{
    public override ServiceType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return ServiceTypeExtensions.TryParse(value, out var result) ? result : default;
    }

    public override void Write(Utf8JsonWriter writer, ServiceType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToCode());
    }
}

