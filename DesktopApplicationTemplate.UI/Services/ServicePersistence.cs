using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.Persistence
{
    public static class ServicePersistence
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services.json");

        public static void Save(IEnumerable<ServiceListModel> services, IServiceCatalog catalog, ILoggingService? logger = null)
        {
            var records = new List<PersistedServiceRecord>();
            var index = 0;
            foreach (var service in services)
            {
                var descriptor = ResolveDescriptor(service.DescriptorId, service.Type, catalog);
                var payload = CreatePayloadElement(service.DescriptorPayload, descriptor?.OptionsSerializer);
                records.Add(new PersistedServiceRecord
                {
                    DisplayName = service.DisplayName,
                    DescriptorId = descriptor?.Id ?? ResolveDescriptorId(service),
                    LegacyType = service.Type,
                    LegacyTypeName = service.Type.ToString(),
                    IsActive = service.IsActive,
                    Created = DateTime.Now,
                    Order = index++,
                    AssociatedServices = new List<string>(service.AssociatedServices),
                    Payload = payload,
                    TotalExecutionTimeMs = service.TotalExecutionTimeMs,
                    ExecutionCount = service.ExecutionCount
                });
            }

            WriteRecords(records, logger);
        }

        public static List<ServiceInfo> Load(IServiceCatalog catalog, ILoggingService? logger = null)
        {
            if (!File.Exists(FilePath))
            {
                logger?.Log("Services file not found", LogLevel.Warning);
                return new List<ServiceInfo>();
            }

            string json;
            try
            {
                json = File.ReadAllText(FilePath);
            }
            catch (FileNotFoundException)
            {
                logger?.Log("Services file not found", LogLevel.Warning);
                return new List<ServiceInfo>();
            }

            try
            {
                var records = JsonSerializer.Deserialize<List<PersistedServiceRecord>>(json, SerializerOptions) ?? new();
                return MapRecords(records, catalog, logger);
            }
            catch (JsonException)
            {
                // fall back to legacy payload shape
            }
            catch (NotSupportedException)
            {
                // fall back to legacy payload shape
            }

            var legacy = JsonSerializer.Deserialize<List<LegacyServiceInfo>>(json, SerializerOptions) ?? new List<LegacyServiceInfo>();
            return MapLegacyRecords(legacy, catalog, logger);
        }

        private static void WriteRecords(IReadOnlyCollection<PersistedServiceRecord> records, ILoggingService? logger)
        {
            try
            {
                var json = JsonSerializer.Serialize(records, SerializerOptions);
                logger?.Log($"Persisting services to {FilePath}", LogLevel.Debug);
                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(FilePath, json);
                logger?.Log($"Saved {records.Count} services to {FilePath}", LogLevel.Debug);
            }
            catch (StackOverflowException)
            {
                var dumpOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                var dump = JsonSerializer.Serialize(records, dumpOptions);
                var temp = Path.Combine(Path.GetTempPath(), "services_dump.json");
                File.WriteAllText(temp, dump);
                Environment.FailFast($"Stack overflow while saving services. Dump written to {temp}");
            }
        }

        private static List<ServiceInfo> MapRecords(IEnumerable<PersistedServiceRecord> records, IServiceCatalog catalog, ILoggingService? logger)
        {
            var result = new List<ServiceInfo>();
            foreach (var record in records.OrderBy(r => r.Order))
            {
                var descriptor = ResolveDescriptor(record.DescriptorId, record.LegacyType ?? ParseLegacyType(record.LegacyTypeName), catalog);
                var payload = DeserializePayload(record.Payload, descriptor?.OptionsSerializer);

                var info = new ServiceInfo
                {
                    DisplayName = record.DisplayName,
                    DescriptorId = descriptor?.Id ?? record.DescriptorId,
                    ServiceType = descriptor?.LegacyType ?? record.LegacyType ?? ParseLegacyType(record.LegacyTypeName),
                    IsActive = record.IsActive,
                    Created = record.Created,
                    Order = record.Order,
                    AssociatedServices = record.AssociatedServices ?? new List<string>(),
                    Payload = payload,
                    TotalExecutionTimeMs = record.TotalExecutionTimeMs,
                    ExecutionCount = record.ExecutionCount
                };

                result.Add(info);
                TryRestoreGlobalOptions(descriptor, payload);
            }

            logger?.Log($"Loaded {result.Count} services", LogLevel.Debug);
            return result;
        }

        private static List<ServiceInfo> MapLegacyRecords(IEnumerable<LegacyServiceInfo> legacy, IServiceCatalog catalog, ILoggingService? logger)
        {
            var result = new List<ServiceInfo>();
            foreach (var record in legacy)
            {
                if (!ServiceTypeExtensions.TryParse(record.ServiceType, out var type))
                {
                    logger?.Log($"Unmapped service type '{record.ServiceType}' for '{record.DisplayName}'", LogLevel.Warning);
                    continue;
                }

                var descriptor = ResolveDescriptor(record.DescriptorId, type, catalog) ?? ResolveDescriptor(type.ToDescriptorId(), type, catalog);
                var payload = ResolveLegacyPayload(type, record);

                var info = new ServiceInfo
                {
                    DisplayName = record.DisplayName,
                    DescriptorId = descriptor?.Id ?? record.DescriptorId ?? type.ToDescriptorId(),
                    ServiceType = descriptor?.LegacyType ?? type,
                    IsActive = record.IsActive,
                    Created = record.Created,
                    Order = record.Order,
                    AssociatedServices = record.AssociatedServices ?? new List<string>(),
                    Payload = payload,
                    TotalExecutionTimeMs = record.TotalExecutionTimeMs,
                    ExecutionCount = record.ExecutionCount
                };

                result.Add(info);
                TryRestoreGlobalOptions(descriptor, payload);
            }

            logger?.Log($"Loaded {result.Count} services", LogLevel.Debug);
            return result;
        }

        private static object? ResolveLegacyPayload(ServiceType type, LegacyServiceInfo record) => type switch
        {
            ServiceType.Tcp => record.TcpOptions,
            ServiceType.Ftp => record.FtpOptions,
            ServiceType.Http => record.HttpOptions,
            ServiceType.Csv => record.CsvOptions,
            _ => null
        };

        private static IServiceDescriptor? ResolveDescriptor(string? descriptorId, ServiceType? legacyType, IServiceCatalog catalog)
        {
            if (!string.IsNullOrWhiteSpace(descriptorId) && catalog.TryGetById(descriptorId!, out var descriptor))
            {
                return descriptor;
            }

            if (legacyType.HasValue && catalog.TryGetByLegacyType(legacyType.Value, out descriptor))
            {
                return descriptor;
            }

            return null;
        }

        private static string ResolveDescriptorId(ServiceListModel service)
        {
            if (!string.IsNullOrWhiteSpace(service.DescriptorId))
            {
                return service.DescriptorId;
            }

            if (service.Type != default)
            {
                return service.Type.ToDescriptorId();
            }

            return service.DisplayName;
        }

        private static JsonElement? CreatePayloadElement(object? payload, IServiceOptionsSerializer? serializer)
        {
            if (payload is null)
            {
                return null;
            }

            if (payload is JsonElement element)
            {
                return element.Clone();
            }

            if (payload is JsonDocument document)
            {
                return document.RootElement.Clone();
            }

            if (serializer is not null)
            {
                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream))
                {
                    serializer.Serialize(writer, payload);
                }

                stream.Position = 0;
                using var doc = JsonDocument.Parse(stream);
                return doc.RootElement.Clone();
            }

            return JsonSerializer.SerializeToElement(payload, payload.GetType(), SerializerOptions);
        }

        private static object? DeserializePayload(JsonElement? element, IServiceOptionsSerializer? serializer)
        {
            if (!element.HasValue)
            {
                return null;
            }

            var value = element.Value;
            if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            if (serializer is not null)
            {
                return serializer.Deserialize(value);
            }

            return value.Deserialize<object>(SerializerOptions);
        }

        private static ServiceType? ParseLegacyType(string? value)
        {
            if (ServiceTypeExtensions.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }

        private static void TryRestoreGlobalOptions(IServiceDescriptor? descriptor, object? payload)
        {
            if (descriptor?.OptionsSerializer is null || payload is null || App.AppHost?.Services is null)
            {
                return;
            }

            var optionsType = descriptor.OptionsSerializer.OptionsType;
            if (!optionsType.IsInstanceOfType(payload))
            {
                return;
            }

            var serviceProvider = App.AppHost.Services;
            var optionsTypeGeneric = typeof(IOptions<>).MakeGenericType(optionsType);
            var optionsInstance = serviceProvider.GetService(optionsTypeGeneric);
            if (optionsInstance is null)
            {
                return;
            }

            var valueProperty = optionsTypeGeneric.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
            if (valueProperty?.GetValue(optionsInstance) is not object target)
            {
                return;
            }

            foreach (var property in optionsType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .Where(p => p.CanRead && p.CanWrite))
            {
                var value = property.GetValue(payload);
                property.SetValue(target, value);
            }
        }
    }

    public class ServiceInfo
    {
        public string DisplayName { get; set; } = string.Empty;
        public string DescriptorId { get; set; } = string.Empty;
        public ServiceType ServiceType { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public int Order { get; set; }
        public List<string> AssociatedServices { get; set; } = new();
        public object? Payload { get; set; }
        public double TotalExecutionTimeMs { get; set; }
        public int ExecutionCount { get; set; }
    }

    internal class PersistedServiceRecord
    {
        public string DisplayName { get; set; } = string.Empty;
        public string DescriptorId { get; set; } = string.Empty;
        public ServiceType? LegacyType { get; set; }
        public string? LegacyTypeName { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public int Order { get; set; }
        public List<string>? AssociatedServices { get; set; }
        public JsonElement? Payload { get; set; }
        public double TotalExecutionTimeMs { get; set; }
        public int ExecutionCount { get; set; }
    }

    internal class LegacyServiceInfo
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string? DescriptorId { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public int Order { get; set; }
        public List<string>? AssociatedServices { get; set; }
        public TcpServiceOptions? TcpOptions { get; set; }
        public FtpServerOptions? FtpOptions { get; set; }
        public HttpServiceOptions? HttpOptions { get; set; }
        public CsvServiceOptions? CsvOptions { get; set; }
        public double TotalExecutionTimeMs { get; set; }
        public int ExecutionCount { get; set; }
    }
}
