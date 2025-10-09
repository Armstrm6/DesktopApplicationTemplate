using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Core.Services.Protocols.Hid;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.Persistence
{
    public static class ServicePersistence
    {
        public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services.json");

        public static void Save(IEnumerable<ServiceListModel> services, IServiceCatalog serviceCatalog, ILoggingService? logger = null)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceCatalog is null)
            {
                throw new ArgumentNullException(nameof(serviceCatalog));
            }

            var data = new List<ServiceInfo>();
            var index = 0;

            foreach (var service in services)
            {
                var descriptor = ResolveDescriptor(serviceCatalog, service.DescriptorId, service.Type);
                EnsureSerializedOptions(service, descriptor);

                var info = new ServiceInfo
                {
                    DisplayName = service.DisplayName,
                    DescriptorId = service.DescriptorId,
                    ServiceType = service.Type,
                    IsActive = service.IsActive,
                    Created = DateTime.Now,
                    Order = index++,
                    AssociatedServices = new List<string>(service.AssociatedServices),
                    SerializedOptions = CloneSerializedOptions(service.SerializedOptions),
                    TotalExecutionTimeMs = service.TotalExecutionTimeMs,
                    ExecutionCount = service.ExecutionCount,
                    IncomingMessageCount = service.IncomingMessageCount,
                    OutgoingMessageCount = service.OutgoingMessageCount,
                    Logs = service.Logs
                        .Take(ServiceListModel.MaxLogEntries)
                        .Select(l => new LogEntry
                        {
                            Level = l.Level,
                            Message = l.Message,
                            Color = l.Color,
                            ServiceType = l.ServiceType ?? service.Type,
                            ServiceName = string.IsNullOrWhiteSpace(l.ServiceName) ? service.DisplayName : l.ServiceName
                        })
                        .ToList(),
                    MessageHistory = service.GetMessageHistorySnapshot().ToList(),
                };

                if (descriptor?.OptionsSerializer is { } serializer)
                {
                    var options = GetOptionsForSerializer(service, serializer, descriptor.Id);
                    if (options is null)
                    {
                        try
                        {
                            options = serializer.CreateDefaultOptions();
                        }
                        catch (Exception)
                        {
                            options = null;
                        }
                    }

                    if (options is not null && TrySerializeOptions(serializer, options, out var element))
                    {
                        info.SerializedOptions[descriptor.Id] = element;
                    }
                }

                data.Add(info);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            try
            {
                var json = JsonSerializer.Serialize(data, options);
                logger?.Log($"Persisting services to {FilePath}", LogLevel.Debug);
                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var sw = new StreamWriter(fs);
                sw.Write(json);
                logger?.Log($"Saved {data.Count} services to {FilePath}", LogLevel.Debug);
            }
            catch (StackOverflowException)
            {
                var dumpOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                var dump = JsonSerializer.Serialize(data, dumpOptions);
                var temp = Path.Combine(Path.GetTempPath(), "services_dump.json");
                File.WriteAllText(temp, dump);
                Environment.FailFast($"Stack overflow while saving services. Dump written to {temp}");
            }
        }

        public static List<ServiceListModel> Load(IServiceCatalog serviceCatalog, ILoggingService? logger = null)
        {
            if (serviceCatalog is null)
            {
                throw new ArgumentNullException(nameof(serviceCatalog));
            }

            if (!File.Exists(FilePath))
            {
                logger?.Log("Services file not found", LogLevel.Warning);
                return new List<ServiceListModel>();
            }

            string json;
            try
            {
                json = File.ReadAllText(FilePath);
            }
            catch (FileNotFoundException)
            {
                logger?.Log("Services file not found", LogLevel.Warning);
                return new List<ServiceListModel>();
            }

            try
            {
                var infos = JsonSerializer.Deserialize<List<ServiceInfo>>(json) ?? new List<ServiceInfo>();
                var services = new List<ServiceListModel>(infos.Count);

                foreach (var info in infos)
                {
                    info.AssociatedServices ??= new List<string>();
                    info.Logs ??= new List<LogEntry>();
                    info.MessageHistory ??= new List<ServiceMessageHistoryEntry>();
                    info.SerializedOptions ??= new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
                    info.LegacySerializedOptions ??= new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

                    var service = CreateServiceModel(info, serviceCatalog);
                    services.Add(service);
                }

                logger?.Log($"Loaded {services.Count} services", LogLevel.Debug);
                return services;
            }
            catch (JsonException)
            {
                logger?.Log("Failed to parse services file", LogLevel.Error);
                return new List<ServiceListModel>();
            }
            catch
            {
                logger?.Log("Failed to parse services file", LogLevel.Error);
                return new List<ServiceListModel>();
            }
        }

        private static ServiceListModel CreateServiceModel(ServiceInfo info, IServiceCatalog serviceCatalog)
        {
            var descriptor = ResolveDescriptor(serviceCatalog, info.DescriptorId, info.ServiceType);
            var descriptorId = descriptor?.Id ?? info.DescriptorId;

            var service = new ServiceListModel
            {
                DescriptorId = descriptorId,
                DisplayName = info.DisplayName,
                Type = info.ServiceType,
                Order = info.Order,
                TotalExecutionTimeMs = info.TotalExecutionTimeMs,
                ExecutionCount = info.ExecutionCount
            };

            service.SerializedOptions = CloneSerializedOptions(info.SerializedOptions);

            foreach (var associated in info.AssociatedServices)
            {
                service.AssociatedServices.Add(associated);
            }

            service.LoadPersistedLogs(info.Logs);
            service.LoadMessageHistory(info.MessageHistory);
            service.InitializeMessageCounts(info.IncomingMessageCount, info.OutgoingMessageCount);
            service.InitializeActivationState(info.IsActive);

            ApplyOptions(info, service, descriptor);

            return service;
        }

        private static void ApplyOptions(ServiceInfo info, ServiceListModel service, IServiceDescriptor? descriptor)
        {
            var serializer = descriptor?.OptionsSerializer;
            var applied = false;

            if (serializer is not null)
            {
                if (!string.IsNullOrWhiteSpace(descriptor?.Id) &&
                    info.SerializedOptions.TryGetValue(descriptor.Id, out var descriptorPayload) &&
                    service.TryApplySerializedOptions(serializer, descriptorPayload, descriptor.Id))
                {
                    applied = true;
                }

                if (!applied &&
                    info.SerializedOptions.TryGetValue(info.ServiceType.ToString(), out var typePayload) &&
                    service.TryApplySerializedOptions(serializer, typePayload, descriptor?.Id ?? service.DescriptorId))
                {
                    applied = true;
                }

                if (!applied)
                {
                    foreach (var payload in info.SerializedOptions)
                    {
                        if (service.TryApplySerializedOptions(serializer, payload.Value, descriptor?.Id ?? payload.Key))
                        {
                            applied = true;
                            break;
                        }
                    }
                }
            }

            if (!applied)
            {
                applied = TryApplyLegacyOptions(info, service, descriptor);
            }

            if (!applied && serializer is not null)
            {
                try
                {
                    var defaults = serializer.CreateDefaultOptions();
                    if (defaults is not null && TrySerializeOptions(serializer, defaults, out var element))
                    {
                        service.TryApplySerializedOptions(serializer, element, descriptor?.Id ?? service.DescriptorId);
                        applied = true;
                    }
                }
                catch (Exception)
                {
                    // Ignore failures and fall through to type-based defaults.
                }
            }
        }

        private static IServiceDescriptor? ResolveDescriptor(IServiceCatalog catalog, string? descriptorId, ServiceType serviceType)
        {
            if (!string.IsNullOrWhiteSpace(descriptorId) && catalog.TryGetById(descriptorId, out var descriptor))
            {
                return descriptor;
            }

            return catalog.Descriptors.FirstOrDefault(d => d.ServiceType == serviceType);
        }

        private static void EnsureSerializedOptions(ServiceListModel service, IServiceDescriptor? descriptor)
        {
            if (descriptor?.OptionsSerializer is not { } serializer)
            {
                return;
            }

            var options = GetOptionsForSerializer(service, serializer, descriptor.Id);
            if (options is null)
            {
                try
                {
                    options = serializer.CreateDefaultOptions();
                }
                catch (Exception)
                {
                    options = null;
                }
            }

            if (options is null)
            {
                return;
            }

            var key = !string.IsNullOrWhiteSpace(descriptor.Id)
                ? descriptor.Id
                : (!string.IsNullOrWhiteSpace(service.DescriptorId)
                    ? service.DescriptorId
                    : service.Type.ToString());

            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (TrySerializeOptions(serializer, options, out var element))
            {
                service.SerializedOptions[key] = element;
            }
        }

        private static Dictionary<string, JsonElement> CloneSerializedOptions(IReadOnlyDictionary<string, JsonElement>? source)
        {
            var result = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            if (source is null)
            {
                return result;
            }

            foreach (var pair in source)
            {
                result[pair.Key] = pair.Value.Clone();
            }

            return result;
        }

        private static bool TryApplyLegacyOptions(ServiceInfo info, ServiceListModel service, IServiceDescriptor? descriptor)
        {
            if (info.LegacySerializedOptions is null || info.LegacySerializedOptions.Count == 0)
            {
                return false;
            }

            if (descriptor?.OptionsSerializer is { } serializer)
            {
                foreach (var key in LegacyOptionKeys)
                {
                    if (!info.LegacySerializedOptions.TryGetValue(key.Key, out var payload) ||
                        payload.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                    {
                        continue;
                    }

                    if (service.TryApplySerializedOptions(serializer, payload, descriptor.Id ?? key.Key))
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (var key in LegacyOptionKeys)
            {
                if (!info.LegacySerializedOptions.TryGetValue(key.Key, out var payload) ||
                    payload.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                {
                    continue;
                }

                if (TryDeserializeLegacyOptions(service, key.Value, payload))
                {
                    return true;
                }
            }

            if (info.ServiceType == ServiceType.Mqtt)
            {
                var fallback = ResolveDefaultMqttOptions() ?? new MqttServiceOptions();
                service.SetOptions(fallback, service.DescriptorId);
                return true;
            }

            return false;
        }

        private static MqttServiceOptions? ResolveDefaultMqttOptions()
        {
            var provider = App.AppHost?.Services;
            var options = provider?.GetService<IOptions<MqttServiceOptions>>();
            if (options is null)
            {
                return null;
            }

            var value = options.Value;
            return new MqttServiceOptions
            {
                Host = value.Host,
                Port = value.Port,
                ClientId = value.ClientId,
                Username = value.Username,
                Password = value.Password,
                ConnectionType = value.ConnectionType,
                WebSocketPath = value.WebSocketPath,
                ClientCertificate = value.ClientCertificate?.ToArray(),
                WillTopic = value.WillTopic,
                WillPayload = value.WillPayload,
                WillQualityOfService = value.WillQualityOfService,
                WillRetain = value.WillRetain,
                KeepAliveSeconds = value.KeepAliveSeconds,
                CleanSession = value.CleanSession,
                ReconnectDelay = value.ReconnectDelay
            };
        }

        private static bool TryDeserializeLegacyOptions(ServiceListModel service, ServiceType serviceType, JsonElement payload)
        {
            try
            {
                switch (serviceType)
                {
                    case ServiceType.Tcp:
                        var tcp = payload.Deserialize<TcpServiceOptions>();
                        if (tcp is not null)
                        {
                            service.SetOptions(tcp, service.DescriptorId);
                            return true;
                        }
                        break;
                    case ServiceType.Ftp:
                        var ftp = payload.Deserialize<FtpServerOptions>();
                        if (ftp is not null)
                        {
                            service.SetOptions(ftp, service.DescriptorId);
                            return true;
                        }
                        break;
                    case ServiceType.Http:
                        var http = payload.Deserialize<HttpServiceOptions>();
                        if (http is not null)
                        {
                            service.SetOptions(http, service.DescriptorId);
                            return true;
                        }
                        break;
                    case ServiceType.Csv:
                        var csv = payload.Deserialize<CsvServiceOptions>();
                        if (csv is not null)
                        {
                            service.SetOptions(csv, service.DescriptorId);
                            return true;
                        }
                        break;
                    case ServiceType.Heartbeat:
                        var heartbeat = payload.Deserialize<HeartbeatServiceOptions>();
                        if (heartbeat is not null)
                        {
                            service.SetOptions(heartbeat, service.DescriptorId);
                            return true;
                        }
                        break;
                    case ServiceType.FileObserver:
                        var observer = payload.Deserialize<FileObserverServiceOptions>();
                        if (observer is not null)
                        {
                            service.SetOptions(observer, service.DescriptorId);
                            return true;
                        }
                        break;
                    case ServiceType.Hid:
                        var hid = payload.Deserialize<HidServiceOptions>();
                        if (hid is not null)
                        {
                            service.SetOptions(hid, service.DescriptorId);
                            return true;
                        }
                        break;
                    case ServiceType.Scp:
                        var scp = payload.Deserialize<ScpServiceOptions>();
                        if (scp is not null)
                        {
                            service.SetOptions(scp, service.DescriptorId);
                            return true;
                        }
                        break;
                    case ServiceType.Mqtt:
                        var mqtt = payload.Deserialize<MqttServiceOptions>();
                        if (mqtt is not null)
                        {
                            service.SetOptions(mqtt, service.DescriptorId);
                            return true;
                        }
                        break;
                }
            }
            catch (JsonException)
            {
                return false;
            }

            return false;
        }

        private static object? GetOptionsForSerializer(ServiceListModel service, IServiceOptionsSerializer serializer, string? key)
        {
            var method = typeof(ServiceListModel).GetMethod(nameof(ServiceListModel.GetOptions), BindingFlags.Public | BindingFlags.Instance);
            if (method is null)
            {
                return null;
            }

            try
            {
                var generic = method.MakeGenericMethod(serializer.OptionsType);
                return generic.Invoke(service, new object?[] { key });
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (TargetInvocationException)
            {
                return null;
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

                element = JsonDocument.Parse(buffer.WrittenSpan).RootElement.Clone();
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

        private static readonly IReadOnlyDictionary<string, ServiceType> LegacyOptionKeys =
            new Dictionary<string, ServiceType>(StringComparer.OrdinalIgnoreCase)
            {
                ["TcpOptions"] = ServiceType.Tcp,
                ["FtpOptions"] = ServiceType.Ftp,
                ["HttpOptions"] = ServiceType.Http,
                ["CsvOptions"] = ServiceType.Csv,
                ["HeartbeatOptions"] = ServiceType.Heartbeat,
                ["FileObserverOptions"] = ServiceType.FileObserver,
                ["HidOptions"] = ServiceType.Hid,
                ["ScpOptions"] = ServiceType.Scp,
                ["MqttOptions"] = ServiceType.Mqtt
            };
    }

    public class ServiceInfo
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? DescriptorId { get; set; }
        public ServiceType ServiceType { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public int Order { get; set; }
        public List<string> AssociatedServices { get; set; } = new();
        [JsonInclude]
        public Dictionary<string, JsonElement> SerializedOptions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public double TotalExecutionTimeMs { get; set; }
        public int ExecutionCount { get; set; }
        public int IncomingMessageCount { get; set; }
        public int OutgoingMessageCount { get; set; }
        public List<LogEntry> Logs { get; set; } = new();
        public List<ServiceMessageHistoryEntry> MessageHistory { get; set; } = new();
        [JsonExtensionData]
        public Dictionary<string, JsonElement> LegacySerializedOptions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

}
