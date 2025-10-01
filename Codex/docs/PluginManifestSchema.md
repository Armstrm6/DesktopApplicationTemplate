# Plug-in Manifest Schema

The `PluginLoader` scans the configured plug-in directory for either folders or `.zip` archives that contain a `plugin.manifest.json` file. Each manifest must adhere to the schema below so the loader can validate the package, extract archives into the versioned cache, and register service modules.

## Manifest Fields

| Field | Required | Description |
| --- | --- | --- |
| `id` | ✔️ | Stable identifier for the plug-in. This value is used when caching extracted archives, so it must remain constant across versions. |
| `name` | ✔️ | Human-friendly name displayed in logs. |
| `version` | ✔️ | Semantic version string (e.g., `1.2.0`). Different versions may coexist because each version is extracted into an isolated cache folder. |
| `entryAssembly` | ✔️ | Relative path to the plug-in entry assembly. The loader builds an `AssemblyLoadContext` around this file to probe dependencies. |
| `serviceAssemblies` | Optional | Collection of additional assemblies to scan for `IServiceModule` implementations. When omitted, the entry assembly is scanned. Paths are relative to the plug-in root. |
| `probingPaths` | Optional | Relative directories that contain dependency assemblies or native libraries. Entries are added to the custom `AssemblyLoadContext` resolver so each plug-in can ship private dependency versions. |

## Example Manifest

```json
{
  "id": "Contoso.Telemetry",
  "name": "Telemetry Services",
  "version": "1.0.0",
  "entryAssembly": "Contoso.Telemetry.dll",
  "serviceAssemblies": [
    "Contoso.Telemetry.dll",
    "Contoso.Telemetry.Services.dll"
  ],
  "probingPaths": [
    "libs",
    "native"
  ]
}
```

## Packaging Guidance

1. Place the manifest at the root of either the plug-in directory or `.zip` archive.
2. All assembly paths must stay within the package root; the loader rejects paths that escape via `..` segments.
3. When shipping a `.zip` archive, the loader extracts the archive to `<ExtractionRoot>/<id>/<version>` where `ExtractionRoot` defaults to `PluginCache` under the application base directory. Existing cache directories are replaced when a new archive is processed.
4. Private dependencies can be placed in subdirectories referenced by `probingPaths`. They are resolved before probing shared application assemblies, allowing multiple plug-ins to ship different dependency versions simultaneously.

## Configuring the Loader

The loader reads configuration from the `Plugins` section:

```json
"Plugins": {
  "Directory": "Plugins",
  "ExtractionRoot": "PluginCache"
}
```

- `Directory` is the location scanned for plug-in directories or archives. Relative paths are resolved against the application base directory.
- `ExtractionRoot` controls where archives are unpacked. Relative paths are also resolved against the application base directory.

Both hosts (`DesktopApplicationTemplate.UI` and `DesktopApplicationTemplate.Service`) log structured messages for successes and failures to simplify diagnostics when a plug-in cannot be loaded.
