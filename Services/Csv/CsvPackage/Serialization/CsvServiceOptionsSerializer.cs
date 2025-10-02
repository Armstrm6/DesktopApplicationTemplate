using System;
using System.Text.Json;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Csv.Options;

namespace DesktopApplicationTemplate.Services.Csv.Serialization;

/// <summary>
/// Provides serialization support for <see cref="CsvServiceOptions"/> payloads.
/// </summary>
public sealed class CsvServiceOptionsSerializer : IServiceOptionsSerializer<CsvServiceOptions>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    /// <inheritdoc />
    public Type OptionsType => typeof(CsvServiceOptions);

    /// <inheritdoc />
    public CsvServiceOptions CreateDefaultOptions() => new();

    object IServiceOptionsSerializer.CreateDefaultOptions() => CreateDefaultOptions();

    /// <inheritdoc />
    public CsvServiceOptions Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new CsvServiceOptions();
        }

        return JsonSerializer.Deserialize<CsvServiceOptions>(json, SerializerOptions) ?? new CsvServiceOptions();
    }

    object IServiceOptionsSerializer.Deserialize(string json) => Deserialize(json);

    /// <inheritdoc />
    public CsvServiceOptions Deserialize(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null)
        {
            return new CsvServiceOptions();
        }

        return element.Deserialize<CsvServiceOptions>(SerializerOptions) ?? new CsvServiceOptions();
    }

    object IServiceOptionsSerializer.Deserialize(JsonElement element) => Deserialize(element);

    /// <inheritdoc />
    public string Serialize(CsvServiceOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return JsonSerializer.Serialize(options, SerializerOptions);
    }

    string IServiceOptionsSerializer.Serialize(object options)
    {
        if (options is not CsvServiceOptions typedOptions)
        {
            throw new ArgumentException($"Expected {typeof(CsvServiceOptions)}", nameof(options));
        }

        return Serialize(typedOptions);
    }

    /// <inheritdoc />
    public void Serialize(Utf8JsonWriter writer, CsvServiceOptions options)
    {
        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        JsonSerializer.Serialize(writer, options, SerializerOptions);
    }

    void IServiceOptionsSerializer.Serialize(Utf8JsonWriter writer, object options)
    {
        if (options is not CsvServiceOptions typedOptions)
        {
            throw new ArgumentException($"Expected {typeof(CsvServiceOptions)}", nameof(options));
        }

        Serialize(writer, typedOptions);
    }
}
