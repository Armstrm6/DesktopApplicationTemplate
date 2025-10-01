# Service Persistence File Format

DesktopApplicationTemplate persists registered services to a JSON file (`services.json` by default). Each entry captures descriptor metadata, runtime state, and a descriptor-defined options payload. This document summarizes the contract so plug-in authors can ensure their descriptors remain compatible across releases.

## High-level structure

The file stores an array of service records:

```json
[
  {
    "DisplayName": "HTTP - My API",
    "DescriptorId": "custom.http",
    "LegacyType": "http",
    "LegacyTypeName": "Http",
    "IsActive": false,
    "Created": "2025-10-07T18:42:11.3246188Z",
    "Order": 0,
    "AssociatedServices": ["TCP - Listener"],
    "SerializedPayload": "{\"BaseUrl\":\"https://example.com\"}",
    "TotalExecutionTimeMs": 0.0,
    "ExecutionCount": 0
  }
]
```

All properties use PascalCase to match the serialized `PersistedServiceRecord` members. Null values are omitted because persistence enables `JsonIgnoreCondition.WhenWritingNull`.

### Core fields

1. **`DescriptorId`** – Stable identifier exposed by the descriptor. Service persistence always prefers descriptor identifiers to locate factories and serializers.
2. **`SerializedPayload`** – Descriptor-provided serialization of the options payload. `ServicePersistence` invokes `IServiceOptionsSerializer.Serialize` on save and `Deserialize` on load, so plug-ins can choose any JSON shape that fits their options model.
3. **`LegacyType` / `LegacyTypeName`** – Compatibility values retained for migrations. `LegacyType` uses the short code written by `ServiceTypeJsonConverter` (for example, `"tcp"`); `LegacyTypeName` records the enum name (for example, `"Tcp"`). During load, the catalog uses `IServiceCatalog.LegacyMap` to translate legacy codes into the correct descriptor.
4. **State metadata** – `DisplayName`, `IsActive`, `Created`, `Order`, `AssociatedServices`, `TotalExecutionTimeMs`, and `ExecutionCount` mirror the values surfaced by `ServiceListModel`.

### Descriptor payload contract

- Implement `IServiceOptionsSerializer` (or `IServiceOptionsSerializer<TOptions>`) on your descriptor to control persistence.
- `SerializedPayload` **must** be valid JSON representing your options type. The serializer can emit compact or indented JSON; persistence stores the raw string.
- When loading, `ServicePersistence` passes the stored string to `Deserialize`. If the serializer is unavailable, the loader falls back to `JsonSerializer.Deserialize<object>`.

### Backward compatibility

- Older files may contain a `Payload` object instead of `SerializedPayload`. The loader still supports this legacy shape by deserializing the JSON object via the descriptor serializer.
- Very old releases used service-type names (`"ServiceType": "FTP"`). Those entries are mapped through `ServiceTypeExtensions.TryParse` and `IServiceCatalog.LegacyMap` so descriptors continue to resolve.
- Descriptors should preserve their identifiers and keep their serializers backward compatible. If options evolve, support migrating legacy JSON inside your `IServiceOptionsSerializer` implementation.

## Validation checklist for plug-in authors

1. Provide a unique `DescriptorId` and `LegacyType` (if migrating an enum-based service).
2. Supply an `IServiceOptionsSerializer` that round-trips your options and tolerates historical payload shapes.
3. Confirm your descriptor registers factories so `MainViewModel` can request the appropriate UI/service factory during load.
4. Add automated tests that serialize services using your descriptor and verify that deserialization restores the expected options and metadata.
