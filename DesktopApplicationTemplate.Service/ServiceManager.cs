using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.Service;

public sealed class ServiceManager : IAsyncDisposable, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ILogger<ServiceManager> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceCatalog _catalog;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _servicesFilePath;
    private readonly string _activeServicesFilePath;
    private readonly Dictionary<string, RunningService> _running = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ServiceRecord> _records = new(StringComparer.Ordinal);

    public ServiceManager(
        ILogger<ServiceManager> logger,
        IConfiguration configuration,
        IServiceCatalog catalog,
        IServiceScopeFactory scopeFactory,
        string? servicesFilePath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

        _servicesFilePath = servicesFilePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services.json");
        _activeServicesFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "activeservices.txt"));
    }

    public IReadOnlyCollection<string> ActiveServices => _running.Keys.ToList();

    public void Sync() => SyncAsync(CancellationToken.None).GetAwaiter().GetResult();

    public async Task SyncAsync(CancellationToken cancellationToken)
    {
        var definitions = LoadDefinitions();
        var activeNames = new HashSet<string>(definitions.Where(d => d.IsActive).Select(d => d.DisplayName), StringComparer.Ordinal);

        foreach (var definition in definitions.Where(d => d.IsActive))
        {
            if (_running.ContainsKey(definition.DisplayName))
            {
                continue;
            }

            await StartServiceAsync(definition, cancellationToken).ConfigureAwait(false);
        }

        foreach (var name in _running.Keys.ToList())
        {
            if (!activeNames.Contains(name))
            {
                await StopServiceAsync(name, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public async Task StopAllAsync(CancellationToken cancellationToken)
    {
        foreach (var name in _running.Keys.ToList())
        {
            await StopServiceAsync(name, cancellationToken).ConfigureAwait(false);
        }

        if (_running.Count == 0)
        {
            DeleteActiveServicesFile();
            _records.Clear();
        }
    }

    public void Dispose()
    {
        StopAllAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAllAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private IReadOnlyList<ServiceDefinition> LoadDefinitions()
    {
        if (!File.Exists(_servicesFilePath))
        {
            return MapConfiguration();
        }

        string json;
        try
        {
            json = File.ReadAllText(_servicesFilePath);
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to read services file '{FilePath}'. Falling back to configuration.", _servicesFilePath);
            return MapConfiguration();
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return MapConfiguration();
        }

        try
        {
            var records = JsonSerializer.Deserialize<List<PersistedServiceRecord>>(json, SerializerOptions) ?? new();
            if (records.Count > 0)
            {
                return MapRecords(records);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Unable to parse persisted services file '{FilePath}'. Attempting legacy format.", _servicesFilePath);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Unsupported data encountered in services file '{FilePath}'. Attempting legacy format.", _servicesFilePath);
        }

        try
        {
            var legacy = JsonSerializer.Deserialize<List<ServiceInfoLegacy>>(json, SerializerOptions) ?? new();
            if (legacy.Count > 0)
            {
                return MapLegacyRecords(legacy);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Unable to parse legacy services file '{FilePath}'. Falling back to configuration.", _servicesFilePath);
        }

        return MapConfiguration();
    }

    private IReadOnlyList<ServiceDefinition> MapConfiguration()
    {
        var items = _configuration.GetSection("Services").Get<List<ServiceConfig>>() ?? new List<ServiceConfig>();
        var ordered = items
            .Select((item, index) => new { Item = item, Order = item.Order, Index = index })
            .OrderBy(entry => entry.Order)
            .ThenBy(entry => entry.Index)
            .Select(entry => entry.Item);

        var result = new List<ServiceDefinition>();
        foreach (var item in ordered)
        {
            if (string.IsNullOrWhiteSpace(item.DisplayName))
            {
                _logger.LogWarning("Service entry missing a display name. Skipping configuration row.");
                continue;
            }

            var resolution = ResolveDescriptor(item.DescriptorId, ParseLegacyType(item.ServiceType));
            if (resolution.Descriptor is null)
            {
                _logger.LogWarning(
                    "No descriptor found for service '{DisplayName}' (DescriptorId: '{DescriptorId}', Legacy: '{LegacyType}').",
                    item.DisplayName,
                    resolution.DescriptorId,
                    item.ServiceType);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(item.ServiceType) && string.IsNullOrWhiteSpace(item.DescriptorId))
            {
                _logger.LogInformation(
                    "Translated legacy service type '{Legacy}' for '{DisplayName}' to descriptor '{DescriptorId}'.",
                    item.ServiceType,
                    item.DisplayName,
                    resolution.DescriptorId);
            }

            result.Add(new ServiceDefinition(
                item.DisplayName,
                resolution.DescriptorId,
                resolution.Descriptor,
                resolution.LegacyType,
                item.IsActive,
                Array.Empty<string>(),
                payload: null));
        }

        return result;
    }

    private IReadOnlyList<ServiceDefinition> MapRecords(IEnumerable<PersistedServiceRecord> records)
    {
        var result = new List<ServiceDefinition>();
        foreach (var record in records.OrderBy(r => r.Order))
        {
            var legacyName = record.LegacyType?.ToString() ?? record.LegacyTypeName;
            var resolution = ResolveDescriptor(record.DescriptorId, ParseLegacyType(legacyName));
            if (resolution.Descriptor is null)
            {
                _logger.LogWarning(
                    "Descriptor '{DescriptorId}' referenced by '{DisplayName}' is not registered. Skipping runtime activation.",
                    resolution.DescriptorId,
                    record.DisplayName);
                continue;
            }

            var payload = DeserializePayload(record, resolution.Descriptor);

            result.Add(new ServiceDefinition(
                record.DisplayName,
                resolution.DescriptorId,
                resolution.Descriptor,
                resolution.LegacyType,
                record.IsActive,
                record.AssociatedServices ?? new List<string>(),
                payload));
        }

        return result;
    }

    private IReadOnlyList<ServiceDefinition> MapLegacyRecords(IEnumerable<ServiceInfoLegacy> legacy)
    {
        var result = new List<ServiceDefinition>();
        foreach (var record in legacy.OrderBy(r => r.Order))
        {
            if (!ServiceTypeExtensions.TryParse(record.ServiceType, out var legacyType))
            {
                _logger.LogWarning("Unmapped legacy service type '{Type}' for '{Name}'.", record.ServiceType, record.DisplayName);
                continue;
            }

            var resolution = ResolveDescriptor(record.DescriptorId, legacyType);
            if (resolution.Descriptor is null)
            {
                _logger.LogWarning(
                    "Descriptor resolution failed for legacy service '{Name}' with type '{Type}'.", record.DisplayName, record.ServiceType);
                continue;
            }

            result.Add(new ServiceDefinition(
                record.DisplayName,
                resolution.DescriptorId,
                resolution.Descriptor,
                resolution.LegacyType,
                record.IsActive,
                record.AssociatedServices ?? new List<string>(),
                payload: null));
        }

        return result;
    }

    private DescriptorResolution ResolveDescriptor(string? descriptorId, ServiceType? legacyType)
    {
        if (!string.IsNullOrWhiteSpace(descriptorId) && _catalog.TryGetById(descriptorId, out var descriptorById))
        {
            return new DescriptorResolution(descriptorById.Id, descriptorById, descriptorById.LegacyType ?? legacyType);
        }

        if (legacyType is not null)
        {
            if (_catalog.TryGetByLegacyType(legacyType.Value, out var descriptorByLegacy))
            {
                return new DescriptorResolution(descriptorByLegacy.Id, descriptorByLegacy, legacyType);
            }

            if (_catalog.LegacyMap.TryGetValue(legacyType.Value, out var mappedId) &&
                _catalog.TryGetById(mappedId, out var mappedDescriptor))
            {
                return new DescriptorResolution(mappedDescriptor.Id, mappedDescriptor, legacyType);
            }

            return new DescriptorResolution(legacyType.Value.ToDescriptorId(), null, legacyType);
        }

        if (!string.IsNullOrWhiteSpace(descriptorId) && _catalog.TryGetById(descriptorId, out var fallbackDescriptor))
        {
            return new DescriptorResolution(fallbackDescriptor.Id, fallbackDescriptor, fallbackDescriptor.LegacyType);
        }

        return new DescriptorResolution(descriptorId ?? string.Empty, null, legacyType);
    }

    private static ServiceType? ParseLegacyType(string? value)
        => ServiceTypeExtensions.TryParse(value, out var parsed) ? parsed : null;

    private object? DeserializePayload(PersistedServiceRecord record, IServiceDescriptor descriptor)
    {
        if (descriptor.OptionsSerializer is null)
        {
            return null;
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(record.SerializedPayload))
            {
                return descriptor.OptionsSerializer.Deserialize(record.SerializedPayload);
            }

            if (record.Payload is JsonElement element)
            {
                return descriptor.OptionsSerializer.Deserialize(element);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize payload for '{DisplayName}'. Using default options instead.", record.DisplayName);
        }

        try
        {
            return descriptor.OptionsSerializer.CreateDefaultOptions();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create default payload for '{DisplayName}'.", record.DisplayName);
            return null;
        }
    }

    private async Task StartServiceAsync(ServiceDefinition definition, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting service '{Name}' using descriptor '{DescriptorId}'.",
            definition.DisplayName,
            definition.DescriptorId);

        var scope = _scopeFactory.CreateScope();
        try
        {
            var binding = definition.Descriptor.Factories
                .FirstOrDefault(f => f.Kind == ServiceFactoryKind.Runtime && typeof(IServiceRuntimeFactory).IsAssignableFrom(f.ContractType));

            if (binding is null)
            {
                _logger.LogWarning(
                    "Descriptor '{DescriptorId}' does not expose a runtime factory. Service '{Name}' will not be started.",
                    definition.DescriptorId,
                    definition.DisplayName);
                scope.Dispose();
                return;
            }

            if (binding.Resolver(scope.ServiceProvider) is not IServiceRuntimeFactory factory)
            {
                _logger.LogWarning(
                    "Runtime factory for descriptor '{DescriptorId}' does not implement {Contract}.",
                    definition.DescriptorId,
                    typeof(IServiceRuntimeFactory).Name);
                scope.Dispose();
                return;
            }

            var context = new ServiceRuntimeContext(
                definition.Descriptor,
                definition.DescriptorId,
                definition.DisplayName,
                definition.AssociatedServices,
                definition.Payload);

            var runtime = factory.Create(context);
            if (runtime is null)
            {
                _logger.LogWarning(
                    "Runtime factory for descriptor '{DescriptorId}' returned null. Service '{Name}' will not be started.",
                    definition.DescriptorId,
                    definition.DisplayName);
                scope.Dispose();
                return;
            }
            await runtime.StartAsync(cancellationToken).ConfigureAwait(false);

            var running = new RunningService(scope, runtime, definition);
            _running[definition.DisplayName] = running;

            _records[definition.DisplayName] = new ServiceRecord(
                definition.DisplayName,
                definition.DescriptorId,
                definition.LegacyType,
                DateTimeOffset.UtcNow);

            WriteActiveServices();

            _logger.LogInformation(
                "Service '{Name}' started with descriptor '{DescriptorId}'.",
                definition.DisplayName,
                definition.DescriptorId);
        }
        catch (Exception ex)
        {
            scope.Dispose();
            _logger.LogError(ex, "Failed to start service '{Name}'.", definition.DisplayName);
        }
    }

    private async Task StopServiceAsync(string displayName, CancellationToken cancellationToken)
    {
        if (!_running.TryGetValue(displayName, out var running))
        {
            return;
        }

        _logger.LogInformation("Stopping service '{Name}'.", displayName);

        _running.Remove(displayName);

        try
        {
            await running.Runtime.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping service '{Name}'.", displayName);
        }
        finally
        {
            try
            {
                await running.Runtime.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing runtime for service '{Name}'.", displayName);
            }

            running.Scope.Dispose();
        }

        if (_records.TryGetValue(displayName, out var record))
        {
            record.Status = "Stopped";
        }

        WriteActiveServices();

        if (_running.Count == 0)
        {
            DeleteActiveServicesFile();
            _records.Clear();
        }

        _logger.LogInformation("Service '{Name}' stopped.", displayName);
    }

    private void WriteActiveServices()
    {
        try
        {
            var pid = Process.GetCurrentProcess().Id;
            var lines = _records.Values
                .Select(r => FormattableString.Invariant(
                    $"{pid}\t{r.Name}\t{r.DescriptorId}\t{(r.LegacyType?.ToLegacyString() ?? string.Empty)}\t{r.StartTime:o}\t{r.Status}"))
                .ToArray();

            if (lines.Length == 0)
            {
                DeleteActiveServicesFile();
                return;
            }

            File.WriteAllLines(_activeServicesFilePath, lines);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to write active services file '{FilePath}'.", _activeServicesFilePath);
        }
    }

    private void DeleteActiveServicesFile()
    {
        try
        {
            if (File.Exists(_activeServicesFilePath))
            {
                File.Delete(_activeServicesFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to delete active services file '{FilePath}'.", _activeServicesFilePath);
        }
    }

    private sealed record ServiceDefinition(
        string DisplayName,
        string DescriptorId,
        IServiceDescriptor Descriptor,
        ServiceType? LegacyType,
        bool IsActive,
        IReadOnlyCollection<string> AssociatedServices,
        object? Payload);

    private readonly record struct DescriptorResolution(
        string DescriptorId,
        IServiceDescriptor? Descriptor,
        ServiceType? LegacyType);

    private sealed record RunningService(IServiceScope Scope, IServiceRuntime Runtime, ServiceDefinition Definition);

    private sealed class ServiceRecord
    {
        public ServiceRecord(string name, string descriptorId, ServiceType? legacyType, DateTimeOffset startTime)
        {
            Name = name;
            DescriptorId = descriptorId;
            LegacyType = legacyType;
            StartTime = startTime;
        }

        public string Name { get; }
        public string DescriptorId { get; }
        public ServiceType? LegacyType { get; }
        public DateTimeOffset StartTime { get; }
        public string Status { get; set; } = "Running";
    }

    private sealed class ServiceConfig
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? DescriptorId { get; set; }
        public string? ServiceType { get; set; }
        public bool IsActive { get; set; }
        public int Order { get; set; }
    }

    private sealed class PersistedServiceRecord
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
        public string? SerializedPayload { get; set; }
    }

    private sealed class ServiceInfoLegacy
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string? DescriptorId { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public int Order { get; set; }
        public List<string>? AssociatedServices { get; set; }
    }
}
