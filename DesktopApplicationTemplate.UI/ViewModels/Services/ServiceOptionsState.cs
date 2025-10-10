using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.ViewModels.Services
{
    public sealed class ServiceOptionsState : ViewModelBase
    {
        private readonly Dictionary<string, object?> _options = new(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, JsonElement> _serializedOptions = new(StringComparer.OrdinalIgnoreCase);

        public ServiceType ServiceType { get; set; }

        public string? DescriptorId { get; set; }

        public Dictionary<string, JsonElement> SerializedOptions
        {
            get => _serializedOptions;
            set
            {
                if (value is null)
                {
                    _serializedOptions = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
                    return;
                }

                _serializedOptions = value.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Clone(),
                    StringComparer.OrdinalIgnoreCase);
            }
        }

        public void SetOptions<TOptions>(TOptions? options, string? key = null)
            where TOptions : class
        {
            var keyCandidates = EnumerateOptionKeys(key).ToArray();
            var resolvedKey = keyCandidates.FirstOrDefault();
            if (resolvedKey is null)
            {
                return;
            }

            if (options is null)
            {
                RemoveOptionKeys(keyCandidates);
                return;
            }

            var serializer = ResolveSerializer(keyCandidates);
            if (serializer is not null && serializer.OptionsType.IsAssignableFrom(typeof(TOptions)))
            {
                SetOptionsWithSerializer(serializer, options, resolvedKey, keyCandidates);
                return;
            }

            SetOptionsWithDefault(options, resolvedKey, keyCandidates);
        }

        public TOptions? GetOptions<TOptions>(string? key = null)
            where TOptions : class
        {
            var keyCandidates = EnumerateOptionKeys(key).ToArray();
            var resolvedKey = keyCandidates.FirstOrDefault();
            if (resolvedKey is null)
            {
                return null;
            }

            var serializer = ResolveSerializer(keyCandidates);

            foreach (var candidate in keyCandidates)
            {
                if (_options.TryGetValue(candidate, out var raw) && raw is TOptions typed)
                {
                    if (!string.Equals(candidate, resolvedKey, StringComparison.Ordinal))
                    {
                        SetOptions(typed, resolvedKey);
                    }

                    return typed;
                }

                if (!_serializedOptions.TryGetValue(candidate, out var element))
                {
                    continue;
                }

                TOptions? deserialized = null;
                if (serializer is not null && serializer.OptionsType.IsAssignableFrom(typeof(TOptions)))
                {
                    try
                    {
                        deserialized = serializer.Deserialize(element) as TOptions;
                    }
                    catch (JsonException)
                    {
                        deserialized = null;
                    }
                    catch (InvalidCastException)
                    {
                        deserialized = null;
                    }
                }

                if (deserialized is null)
                {
                    try
                    {
                        deserialized = element.Deserialize<TOptions>();
                    }
                    catch (JsonException)
                    {
                        deserialized = null;
                    }
                }

                if (deserialized is not null)
                {
                    SetOptions(deserialized, resolvedKey);
                    return deserialized;
                }
            }

            return null;
        }

        public TOptions GetOrCreateOptions<TOptions>(Func<TOptions> factory, string? key = null)
            where TOptions : class
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var existing = GetOptions<TOptions>(key);
            if (existing is not null)
            {
                return existing;
            }

            var created = factory();
            SetOptions(created, key);
            return created;
        }

        internal bool TryApplySerializedOptions(IServiceOptionsSerializer serializer, JsonElement payload, string? key = null)
        {
            var keyCandidates = EnumerateOptionKeys(key).ToArray();
            var resolvedKey = keyCandidates.FirstOrDefault();
            if (resolvedKey is null)
            {
                return false;
            }

            try
            {
                var options = serializer.Deserialize(payload);
                if (!serializer.OptionsType.IsInstanceOfType(options))
                {
                    return false;
                }

                SetOptionsWithSerializer(serializer, options!, resolvedKey, keyCandidates, payload);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        private IEnumerable<string> EnumerateOptionKeys(string? overrideKey)
        {
            if (!string.IsNullOrWhiteSpace(overrideKey))
            {
                yield return overrideKey;
            }

            if (!string.IsNullOrWhiteSpace(DescriptorId))
            {
                yield return DescriptorId!;
            }

            if (ServiceType != default)
            {
                yield return ServiceType.ToString();
            }
        }

        private IServiceOptionsSerializer? ResolveSerializer(string[] keyCandidates)
        {
            foreach (var candidate in keyCandidates)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                var serializer = ServiceStateCoordinator.ResolveOptionsSerializer(candidate);
                if (serializer is not null)
                {
                    return serializer;
                }
            }

            return null;
        }

        private void SetOptionsWithSerializer(IServiceOptionsSerializer serializer, object options, string resolvedKey, string[] keyCandidates, JsonElement? payloadOverride = null)
        {
            if (!serializer.OptionsType.IsInstanceOfType(options))
            {
                throw new ArgumentException($"Options must be assignable to {serializer.OptionsType}.", nameof(options));
            }

            _options[resolvedKey] = options;

            if (payloadOverride is JsonElement payload)
            {
                _serializedOptions[resolvedKey] = payload.Clone();
            }
            else if (TrySerializeOptions(serializer, options, out var element))
            {
                _serializedOptions[resolvedKey] = element;
            }
            else
            {
                _serializedOptions.Remove(resolvedKey);
            }

            RemoveCandidateKeys(keyCandidates, resolvedKey);
        }

        private void SetOptionsWithDefault<TOptions>(TOptions options, string resolvedKey, string[] keyCandidates)
            where TOptions : class
        {
            _options[resolvedKey] = options;

            try
            {
                _serializedOptions[resolvedKey] = JsonSerializer.SerializeToElement(options);
            }
            catch (NotSupportedException)
            {
                _serializedOptions.Remove(resolvedKey);
            }
            catch (JsonException)
            {
                _serializedOptions.Remove(resolvedKey);
            }

            RemoveCandidateKeys(keyCandidates, resolvedKey);
        }

        private void RemoveOptionKeys(string[] keyCandidates)
        {
            foreach (var candidate in keyCandidates)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                _options.Remove(candidate);
                _serializedOptions.Remove(candidate);
            }
        }

        private void RemoveCandidateKeys(string[] keyCandidates, string resolvedKey)
        {
            foreach (var candidate in keyCandidates)
            {
                if (string.IsNullOrWhiteSpace(candidate) || string.Equals(candidate, resolvedKey, StringComparison.Ordinal))
                {
                    continue;
                }

                _options.Remove(candidate);
                _serializedOptions.Remove(candidate);
            }
        }

        private static bool TrySerializeOptions(IServiceOptionsSerializer serializer, object options, out JsonElement element)
        {
            element = default;
            try
            {
                if (!serializer.OptionsType.IsInstanceOfType(options))
                {
                    return false;
                }

                var buffer = new ArrayBufferWriter<byte>();
                using (var writer = new Utf8JsonWriter(buffer))
                {
                    serializer.Serialize(writer, options);
                }

                element = JsonDocument.Parse(buffer.WrittenMemory).RootElement.Clone();
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
    }
}
