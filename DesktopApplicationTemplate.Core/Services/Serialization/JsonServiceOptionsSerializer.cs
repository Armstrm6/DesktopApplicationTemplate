using System;
using System.Text.Json;

namespace DesktopApplicationTemplate.Core.Services.Serialization;

/// <summary>
/// Serializes service options using <see cref="JsonSerializer"/>.
/// </summary>
/// <typeparam name="TOptions">Options type.</typeparam>
public sealed class JsonServiceOptionsSerializer<TOptions> : IServiceOptionsSerializer<TOptions>
    where TOptions : class
{
    private readonly JsonSerializerOptions _options;
    private readonly Func<TOptions> _factory;

    public JsonServiceOptionsSerializer(Func<TOptions>? factory = null, JsonSerializerOptions? options = null)
    {
        _factory = factory ?? CreateDefaultFactory();
        _options = options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };
    }

    public Type OptionsType => typeof(TOptions);

    public TOptions CreateDefaultOptions() => _factory();

    object IServiceOptionsSerializer.CreateDefaultOptions() => CreateDefaultOptions();

    public TOptions Deserialize(string json) =>
        JsonSerializer.Deserialize(json, OptionsType, _options) as TOptions
        ?? throw new InvalidOperationException($"Unable to deserialize {OptionsType.Name} from payload.");

    object IServiceOptionsSerializer.Deserialize(string json) => Deserialize(json);

    public TOptions Deserialize(JsonElement element) =>
        element.Deserialize<TOptions>(_options)
        ?? throw new InvalidOperationException($"Unable to deserialize {OptionsType.Name} from element.");

    object IServiceOptionsSerializer.Deserialize(JsonElement element) => Deserialize(element);

    public string Serialize(TOptions options) => JsonSerializer.Serialize(options, _options);

    string IServiceOptionsSerializer.Serialize(object options) => Serialize((TOptions)options);

    public void Serialize(Utf8JsonWriter writer, TOptions options)
    {
        JsonSerializer.Serialize(writer, options, _options);
    }

    void IServiceOptionsSerializer.Serialize(Utf8JsonWriter writer, object options) => Serialize(writer, (TOptions)options);

    private static Func<TOptions> CreateDefaultFactory()
    {
        if (typeof(TOptions).GetConstructor(Type.EmptyTypes) == null)
        {
            throw new InvalidOperationException($"Options type {typeof(TOptions).Name} must provide a parameterless constructor or a factory.");
        }

        return Activator.CreateInstance<TOptions>;
    }
}
