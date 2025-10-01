# Custom Service Guide

This guide explains how to extend the application with new services by reusing existing components and `IServiceModule`.

## Reuse shared components

- **ServiceRule**: central validation helper injected into create and edit view models for required field checks.
- **ServiceScreen**: wraps user interactions for options models and exposes events that views can bind to.
- **Base view models**: inherit from `ServiceCreateViewModelBase` and `ServiceEditViewModelBase` to get common save logic, logging, and navigation.
- **Shared controls**: components such as `ServiceLogView` and `ServiceMessageTableView` provide consistent UI panels that can be embedded in new service pages.

## Register dependencies with `IServiceModule`

`IServiceModule` allows each service to package its DI registrations. Implement the interface for your service and expose the corresponding `ServiceType`:

```csharp
public class SampleServiceModule : IServiceModule
{
    public ServiceType Type => ServiceType.Sample;

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<SampleService>();
        services.AddTransient<SampleCreateViewModel>();
        services.AddTransient<SampleEditViewModel>();
        services.AddTransient<SamplePage>();
    }
}
```

Modules are discovered at startup by calling `services.AddServiceModules()` during host configuration.

## Adding a new service type

1. **Update the enum** – add a value to `ServiceType` to identify the service.
2. **Create options and implementation** – define an options class and the runtime service that executes the work.
3. **Build the UI** – create view models and views, reusing `ServiceRule`, `ServiceScreen`, and base classes for common behavior.
4. **Implement an `IServiceModule`** – register the service, view models, and views with the DI container.
5. **Wire up navigation** – add create and edit handlers or factory entries so the main window can navigate to the new views.
6. **Test and document** – run `dotnet` commands and update docs as needed.

## Package and distribute plug-ins

1. **Import the packaging targets** – reference `ServicePlugin.Packaging` by adding the props/targets pair to the plug-in `.csproj`. This enables the `PackServicePlugin` target after every build.
2. **Author `plugin.manifest.json`** – populate the manifest with the plug-in id, name, version, entry assembly, and any assemblies that expose `IServiceModule` implementations. The loader validates the manifest against the schema documented in `Codex/docs/PluginManifestSchema.md`.
3. **Build to create archives** – run `dotnet build` for the plug-in. The packaging target copies the manifest, compiled assemblies, and dependencies into `.ccp` and `.chapp` archives under `bin/<configuration>/<tfm>/plugins`.
4. **Distribute the archive** – drop the generated archive into the host's plug-in directory (or publish it via your preferred channel). The loader extracts each archive into its versioned cache and automatically registers the descriptors.

### Reusable resources

- **Template** – `Templates/ServicePluginTemplate` exposes a `dotnet new codex-serviceplugin` template that scaffolds a descriptor, runtime factory, manifest, and packaging imports.
- **Samples** – `Samples/TcpRelayPlugin` and `Samples/HttpRelayPlugin` provide ready-to-build examples that produce distributable archives during CI, making them ideal smoke tests for packaging changes.
