# Codex Service Plug-in Template

This template demonstrates how to build a service plug-in that can be packaged with the `ServicePlugin.Packaging` project. It implements:

1. A manifest (`plugin.manifest.json`) that is zipped into `.ccp` and `.chapp` archives.
2. An `IServiceModule` (`TemplateServiceModule`) that registers runtime dependencies.
3. A descriptor (`TemplateServiceDescriptor`) that advertises runtime bindings.
4. A runtime factory (`TemplateRuntimeFactory`) that logs lifecycle events.

Install the template locally and scaffold a new plug-in within the repository root:

```bash
# From the repository root
dotnet new install Templates/ServicePluginTemplate
mkdir Plugins
cd Plugins
dotnet new codex-serviceplugin -n Contoso.Telemetry.Plugin \
  --pluginId Contoso.Telemetry \
  --pluginName "Contoso Telemetry" \
  --pluginVersion 1.0.0 \
  --descriptorId contoso.telemetry.service \
  --descriptorCategory Telemetry
```

The generated project automatically imports `ServicePlugin.Packaging` so building the project produces distributable archives under `bin/<configuration>/<tfm>/plugins`.
