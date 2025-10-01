# Custom Service Guide

This guide explains how to extend the application with descriptor-driven services, migrate legacy enum-based implementations, and package the result as a plug-in.

## Understand the descriptor workflow

1. **Descriptors advertise capability** – implement `IServiceDescriptor` (or inherit from `ServiceDescriptorBase`) to describe presentation metadata, runtime factories, and serialization behavior. The descriptor identifier becomes the primary lookup key for persistence, navigation, and plug-in imports.
2. **Modules bundle registrations** – each plug-in exposes one or more `IServiceModule` implementations. During host startup `services.AddServiceModules()` discovers these modules, registers their dependencies, and aggregates their descriptors into the shared `IServiceCatalog`.
3. **Attributes connect UI handlers** – classes tagged with `ServiceDescriptorRegistrationAttribute` declare how view models, navigation handlers, and other UI assets bind to a descriptor identifier. The application maps descriptor ids to these registrations at boot.
4. **Legacy services stay compatible** – descriptors may expose `LegacyType` values so persisted enum-based services load through the new catalog automatically.

## Migrate an enum-based service

Use this checklist when upgrading a service that previously depended on the `ServiceType` enum:

1. **Create a descriptor** – add a new class that derives from `ServiceDescriptorBase`. Provide a stable `DescriptorId`, fill in presentation metadata, and pass the legacy enum value to the base constructor.
2. **Expose factories** – populate the descriptor's `ServiceFactoryBinding` collection so runtime factories, editors, and navigation handlers can resolve through `IServiceCatalog`.
3. **Register descriptor-aware handlers** – annotate edit, navigation, and runtime factory classes with `ServiceDescriptorRegistrationAttribute(DescriptorId, kind)` so dependency injection connects them to the descriptor instead of enum switches.
4. **Update persistence hooks** – ensure the service's options serializer implements `IServiceOptionsSerializer` and is referenced by the descriptor. Persisted records should prefer `DescriptorId` while continuing to supply `LegacyType` for migrations.
5. **Remove enum branches** – replace `switch` statements and lookup tables keyed by `ServiceType` with descriptor id dictionaries (for example, `IServiceCatalog.Descriptors`). When legacy values remain necessary, convert them with `ServiceTypeExtensions.ToDescriptorId()`.
6. **Validate migrations** – load existing persisted data through `ServicePersistence` to confirm legacy enum values resolve to the descriptor id and that UI bindings display descriptor metadata.

## Build a descriptor-first service

1. **Define options and runtime logic** – create an options model, runtime service, and any supporting factories that perform the work.
2. **Implement the descriptor** – derive from `ServiceDescriptorBase`, provide presentation metadata, wire in factories, and optionally supply a payload description delegate for UI tooltips.
3. **Author UI bindings** – create view models and pages that reuse shared helpers (`ServiceRule`, `ServiceScreen`, `ServiceCreateViewModelBase`, `ServiceEditViewModelBase`, `ServiceLogView`, and `ServiceMessageTableView`). Decorate each class with `ServiceDescriptorRegistrationAttribute` so the host links it to the descriptor id.
4. **Register via `IServiceModule`** – implement an `IServiceModule` that adds the runtime services, view models, and views to dependency injection and yields the descriptor from `DescribeServices()`.
5. **Document and test** – update the README and changelog with the new descriptor, and rely on CI to execute descriptor-focused tests described in `Codex/CollaborationGuidelines.txt`.

## Package and distribute plug-ins

1. **Import the packaging targets** – reference `ServicePlugin.Packaging` by adding the props/targets pair to the plug-in `.csproj`. This enables the `PackServicePlugin` target after every build.
2. **Author `plugin.manifest.json`** – populate the manifest with the plug-in id, name, version, entry assembly, and any assemblies that expose `IServiceModule` implementations. The loader validates the manifest against the schema documented in `Codex/docs/PluginManifestSchema.md`.
3. **Build to create archives** – run `dotnet build` for the plug-in. The packaging target copies the manifest, compiled assemblies, and dependencies into `.peakiot` archives under `bin/<configuration>/<tfm>/plugins`.
4. **Distribute the archive** – drop the generated archive into the host's plug-in directory (or publish it via your preferred channel). The loader extracts each supported archive into its versioned cache and automatically registers the descriptors.

### Reusable resources

- **Template** – `Templates/ServicePluginTemplate` exposes a `dotnet new codex-serviceplugin` template that scaffolds a descriptor, runtime factory, manifest, and packaging imports.
- **Samples** – `Samples/TcpRelayPlugin` and `Samples/HttpRelayPlugin` provide ready-to-build examples that produce distributable archives during CI, making them ideal smoke tests for packaging changes.
- **Verification** – follow the QA checklist in `Codex/docs/PluginVerificationChecklist.md` to confirm imports, descriptor rendering, and persistence migrations before releasing new plug-ins.
