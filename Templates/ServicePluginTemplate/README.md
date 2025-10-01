# Codex Service Plug-in Template

This template demonstrates how to build a service plug-in that can be packaged with the `ServicePlugin.Packaging` project. It implements:

1. A manifest (`plugin.manifest.json`) that is zipped into `.ccp` and `.chapp` archives.
2. An `IServiceModule` (`TemplateServiceModule`) that registers runtime dependencies.
3. A descriptor (`TemplateServiceDescriptor`) that advertises runtime bindings.
4. A runtime factory (`TemplateRuntimeFactory`) that logs lifecycle events.

When migrating an existing enum-based service, start by cloning this template and then:

1. Copy the legacy options model and runtime logic into the generated project.
2. Replace the placeholder descriptor with one that calls the `ServiceDescriptorBase` constructor using your legacy `ServiceType` value so persisted records upgrade automatically.
3. Port create/edit view models and tag them with `ServiceDescriptorRegistrationAttribute` so the host binds them to the descriptor id.
4. Update persistence helpers to emit the descriptor id and validate the upgrade by following `Codex/docs/PluginVerificationChecklist.md`.

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
