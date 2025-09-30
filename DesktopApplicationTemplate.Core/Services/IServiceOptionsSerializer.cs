using System;
using System.Text.Json;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Provides helpers for serializing and deserializing service option payloads.
/// </summary>
public interface IServiceOptionsSerializer
{
    /// <summary>
    /// Gets the concrete options type handled by the serializer.
    /// </summary>
    Type OptionsType { get; }

    /// <summary>
    /// Creates a default instance of the options type.
    /// </summary>
    object CreateDefaultOptions();

    /// <summary>
    /// Deserializes the provided JSON payload into the options type.
    /// </summary>
    object Deserialize(string json);

    /// <summary>
    /// Deserializes the provided JSON element into the options type.
    /// </summary>
    object Deserialize(JsonElement element);

    /// <summary>
    /// Serializes the supplied options instance to a JSON payload.
    /// </summary>
    string Serialize(object options);

    /// <summary>
    /// Writes the supplied options instance to the provided JSON writer.
    /// </summary>
    void Serialize(Utf8JsonWriter writer, object options);
}

/// <summary>
/// Strongly typed serializer contract for service options.
/// </summary>
/// <typeparam name="TOptions">Options type.</typeparam>
public interface IServiceOptionsSerializer<TOptions> : IServiceOptionsSerializer
{
    /// <summary>
    /// Creates a default instance of the options type.
    /// </summary>
    new TOptions CreateDefaultOptions();

    /// <summary>
    /// Deserializes the provided JSON payload into the options type.
    /// </summary>
    new TOptions Deserialize(string json);

    /// <summary>
    /// Deserializes the provided JSON element into the options type.
    /// </summary>
    new TOptions Deserialize(JsonElement element);

    /// <summary>
    /// Serializes the supplied options instance to a JSON payload.
    /// </summary>
    string Serialize(TOptions options);

    /// <summary>
    /// Writes the supplied options instance to the provided JSON writer.
    /// </summary>
    void Serialize(Utf8JsonWriter writer, TOptions options);
}
