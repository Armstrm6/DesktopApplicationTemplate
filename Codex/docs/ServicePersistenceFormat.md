# Service Persistence File Format

DesktopApplicationTemplate persists registered services to a JSON file (`services.json` by default). Each entry captures descriptor metadata, runtime state, and a descriptor-defined options payload. This document summarizes the contract so plug-in authors can ensure their descriptors remain compatible across releases.

## High-level structure

The file stores an array of service records:

```json
[
  {
    "DisplayName": "HTTP - My API",
    "DescriptorId": "custom.http",
    "ServiceType": "HT",
    "IsActive": false,
    "Created": "2025-10-07T18:42:11.3246188Z",
    "Order": 0,
    "AssociatedServices": ["TCP - Listener"],
    "HttpOptions": {
      "BaseUrl": "https://example.com"
    },
    "TotalExecutionTimeMs": 0.0,
    "ExecutionCount": 0
  }
]
```

All properties use PascalCase to match the serialized `PersistedServiceRecord` members. Null values are omitted because persistence enables `JsonIgnoreCondition.WhenWritingNull`.

### Core fields

1. **`DescriptorId`** – Stable identifier exposed by the descriptor. Service persistence always prefers descriptor identifiers to locate factories and serializers.
2. **`ServiceType`** – Canonical short code emitted by `ServiceTypeJsonConverter` (for example, `"HT"` for `Http`). The value is kept in sync with the descriptor metadata and persisted alongside the identifier for quick filtering.
3. **Service-specific options** – Built-in services write their strongly-typed options (`HttpOptions`, `TcpOptions`, and so on). Plug-ins should continue to expose serializers that hydrate their view models from the stored JSON payload.
4. **State metadata** – `DisplayName`, `IsActive`, `Created`, `Order`, `AssociatedServices`, `TotalExecutionTimeMs`, and `ExecutionCount` mirror the values surfaced by `ServiceListModel`.

### Descriptor payload contract

- Implement `IServiceOptionsSerializer` (or `IServiceOptionsSerializer<TOptions>`) on your descriptor to control persistence.
- Built-in descriptors persist their strongly typed option models directly. Plug-ins can emit whatever JSON shape their serializer understands, storing the document under descriptor-specific properties.
- When loading, `ServicePersistence` passes the stored JSON to `Deserialize`. If the serializer is unavailable, the loader falls back to `JsonSerializer.Deserialize<object>`.

### Backward compatibility

- Older files may contain a `Payload` object instead of service-specific option properties. The loader still supports this legacy shape by deserializing the JSON object via the descriptor serializer.
- Persisted records must now provide a `DescriptorId` and canonical `ServiceType` code. The loader no longer translates legacy enum names automatically, so update historical exports before migrating to the trimmed format.
- Descriptors should preserve their identifiers and keep their serializers backward compatible. If options evolve, support migrating legacy JSON inside your `IServiceOptionsSerializer` implementation.

## Validation checklist for plug-in authors

1. Provide a unique `DescriptorId` and canonical `ServiceType` code (if migrating an enum-based service).
2. Supply an `IServiceOptionsSerializer` that round-trips your options and tolerates historical payload shapes.
3. Confirm your descriptor registers factories so `MainViewModel` can request the appropriate UI/service factory during load.
4. Add automated tests that serialize services using your descriptor and verify that deserialization restores the expected options and metadata.
