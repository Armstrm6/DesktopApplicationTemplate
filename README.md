# DesktopApplicationTemplate

![CI](https://github.com/OWNER/REPO/actions/workflows/ci.yml/badge.svg)

This repository contains a basic WPF UI application and a Windows Service.

See `Codex/CollaborationGuidelines.txt` for tips on working with the repository. A running log of past collaboration decisions lives in `Codex/docs/CollaborationAndDebugTips.txt`.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (8.0.404)
- [Git LFS](https://git-lfs.com) for large binary assets
- Windows OS is required to run the WPF UI and service projects.
- WPF is included with the Windows .NET SDK; no separate workload installation is required on Windows.

Ensure the 8.0.404 SDK is installed so the pinned `global.json` version resolves correctly.

### Git LFS

Install and initialize Git LFS, then download any LFS-tracked binaries:

```bash
git lfs install
git lfs pull
```

Run these commands after cloning to ensure all binary dependencies are available.

This repository includes a `global.json` file that pins the required
SDK version. If multiple SDKs are installed, the `dotnet` CLI will use
version `8.0.404` as specified in this file. Running commands from the
repository root will automatically respect this setting.

## Initial setup

After cloning the repository:

- Rely on GitHub Actions to run the standard `dotnet restore` and `dotnet build DesktopApplicationTemplate.sln` commands documented below. Codex runs inside a Linux container without the WindowsDesktop runtime, so capture the CI logs (or maintainer summaries) when documenting build results.
- Run the setup script to configure the Git hooks and [Git LFS](https://git-lfs.com/):

  ```bash
  ./setup.sh
  ```

Run the script any time the project dependencies or hooks need to be refreshed.

## Build the solution

> **Note:** The Codex container lacks the WindowsDesktop runtime and cannot execute these commands. GitHub Actions performs the authoritative build; Windows collaborators only need to re-run the steps locally when diagnosing CI issues.

Restore NuGet packages and build all projects:

```bash
dotnet restore
dotnet build DesktopApplicationTemplate.sln
```

The `dotnet` commands will use the SDK version defined in
`global.json` (`8.0.404`). If you have multiple versions installed,
there is no need to switch manually when working inside this
repository.

You can also execute `setup.sh` to configure Git hooks and Git LFS, restore
dependencies, and build the solution in a single step:

```bash
./setup.sh
```

To build a specific project:

```bash
dotnet build DesktopApplicationTemplate.UI/DesktopApplicationTemplate.UI.csproj

dotnet build DesktopApplicationTemplate.Service/DesktopApplicationTemplate.Service.csproj
```

## Run the projects

Launch the UI from the command line:

```bash
dotnet run --project DesktopApplicationTemplate.UI/DesktopApplicationTemplate.UI.csproj
```

The UI supports both light and dark themes. Open **Settings** within the application to toggle the theme and click **Save** to apply it immediately.

Run the background service (useful for development):

```bash
dotnet run --project DesktopApplicationTemplate.Service/DesktopApplicationTemplate.Service.csproj
```

## Installer notes

The installer copies all runtime dependencies based on the generated `.deps.json`
file of the build output. New library references are automatically detected and
no manual configuration is required.

## Services overview

The UI exposes several built in service types. A brief description of each is shown below.

- **HID** – configure HID devices, forward output to another service, set debounce and key down times, select USB protocol (2.0/3.0) and apply custom formatting.
- **TCP** – run a lightweight TCP server and test message processing scripts.
- **HTTP** – send HTTP requests with editable headers and body fields.
- **File Observer** – watch folders and optionally send TCP commands when new files are detected.
- **Heartbeat** – build simple heartbeat messages with optional `PING` and `STATUS` tokens.
- **CSV Creator** – generate CSV files from values produced by other services.
- **SCP / FTP / MQTT** – provide basic clients for those protocols.

Each service has an editor page where the parameters and test messages can be modified.  A **Help** button is available on these pages to display common ASCII commands (ACK, NAK, ENQ, ETX) which can be inserted when building protocol messages.

## Descriptor-based plug-in workflow

Descriptors replace the legacy `ServiceType` enum as the primary identifier for services. Each descriptor:

1. Supplies metadata (label, category, description, icon glyphs, and accent colors) displayed throughout the UI.
2. Advertises runtime factories and serializers through `ServiceFactoryBinding` entries.
3. Optionally exposes a `LegacyType` so persisted enum-based services migrate seamlessly.

During startup `services.AddServiceModules()` discovers `IServiceModule` implementations, registers their dependencies, and gathers descriptors into the shared `IServiceCatalog`. Classes decorated with `ServiceDescriptorRegistrationAttribute` expose navigation handlers, editors, and runtime factories keyed by descriptor id—no manual switch statements required.

### Migrate an enum-based service

1. Create a descriptor class (for example, derive from `ServiceDescriptorBase`) that exposes a stable `DescriptorId`, presentation metadata, and the previous enum value via the base constructor.
2. Update runtime factories, view models, and navigation handlers to depend on descriptor ids. Decorate each class with `ServiceDescriptorRegistrationAttribute` so dependency injection binds it to the descriptor.
3. Ensure persistence emits the descriptor id and, when applicable, the legacy enum value. `ServicePersistence` automatically upgrades records by consulting `IServiceCatalog` and `ServiceTypeExtensions.ToDescriptorId()`.
4. Remove enum-driven `switch` statements in favor of descriptor lookups from `IServiceCatalog` or descriptor id dictionaries.
5. Validate the migration with the QA checklist in `Codex/docs/PluginVerificationChecklist.md` before releasing the change.

### Add a descriptor-first service

1. Build the runtime: define options, runtime services, and supporting dependencies.
2. Implement the descriptor with presentation metadata and factory bindings.
3. Create view models and XAML pages that reuse shared helpers such as `ServiceCreateViewModelBase`, `ServiceEditViewModelBase`, `ServiceRule`, and `ServiceLogView`.
4. Annotate navigation handlers, editors, and runtime factories with `ServiceDescriptorRegistrationAttribute(DescriptorId, kind)` so the app wires them automatically.
5. Register everything inside an `IServiceModule` by returning the descriptor from `DescribeServices()` and adding dependencies in `RegisterServices`.
6. Package and test the plug-in as described below.

## Packaging service plug-ins

The repository ships with an MSBuild packaging project that bundles plug-in assets into distributable `.peakiot` archives (and optional legacy `.ccp`/`.chapp` packages). To enable packaging in a plug-in project:

1. Import `ServicePlugin.Packaging` before and after the project body:

   ```xml
   <Import Project="..\..\ServicePlugin.Packaging\ServicePlugin.Packaging.props" />
   ...
   <Import Project="..\..\ServicePlugin.Packaging\ServicePlugin.Packaging.targets" />
   ```

2. Author a `plugin.manifest.json` file at the project root. The manifest must identify the plug-in id, name, version, entry assembly, and any assemblies that expose `IServiceModule` implementations. See `Codex/docs/PluginManifestSchema.md` for field descriptions.
3. Build the plug-in with `dotnet build`. The packaging targets run after compilation and emit archives to `bin/<configuration>/<tfm>/plugins`. Set `ServicePluginArchiveExtensions` to `.peakiot` (the default) or include legacy extensions such as `.ccp`/`.chapp` when older hosts require them.

Each archive contains the manifest, compiled descriptors, and copy-local dependencies under `libs/`. Hosts can drop any supported archive into the plug-in directory and the loader will extract it into the cache configured by `PluginLoaderOptions`.

### Templates and samples

- **Template:** `Templates/ServicePluginTemplate` publishes a `dotnet new codex-serviceplugin` template that scaffolds a plug-in project with descriptor, runtime factory, manifest, and packaging imports. Install it locally with `dotnet new install Templates/ServicePluginTemplate` and scaffold new plug-ins under a folder where `..\..\ServicePlugin.Packaging` resolves to the repository root.
- **Samples:** `Samples/TcpRelayPlugin` and `Samples/HttpRelayPlugin` demonstrate packaging descriptor-based services. Both projects import `ServicePlugin.Packaging`, emit archives during CI, and exercise descriptor metadata during packaging validation.
- **Verification:** Follow `Codex/docs/PluginVerificationChecklist.md` to confirm import flows, descriptor rendering, and persistence migrations prior to distributing new packages.

## Testing services locally

After building the solution, run the UI project and navigate to the desired service page. Most services expose a test action (for example, "Send" on the HTTP page or "Test Script" on the TCP page) that can be executed locally. Logs for each service are displayed next to the editor fields.

The background service can also be run from the command line:

```bash
dotnet run --project DesktopApplicationTemplate.Service/DesktopApplicationTemplate.Service.csproj
```

This launches the hosted service which periodically emits a heartbeat message using the settings from `appsettings.json`.

## CSV editor example

Use the menu option **CSV Viewer** to open the CSV configuration window. Columns can be added and associated with a service and an optional script. When another service produces a value, call `CsvService.RecordLog` or `CsvService.AppendRow` with the values and a CSV file will be written using the filename pattern from the editor. For example:

```csharp
// gather data from services
csvService.AppendRow(new [] { tcpValue, httpStatus });
```

This appends a new row to `output_{index}.csv` with the TCP and HTTP values.

## Referencing other service messages

Any log entry can reference another service's message using the format
`SERVICETYPE.ServiceName.Message`. When such a log is added, both services will
link to each other under **Associated Services** and the referenced message will
appear in the target service's log.

## C# message scripts

The **TCP** service can execute small C# scripts for transforming incoming
messages. Create a folder named `Scripts` in the UI project and place your
script files there. A script should expose a `string Process(string message)`
method which receives the incoming message and returns the response. Configure
`AppSettings:DefaultCSharpScriptPath` in the appropriate `appsettings` file to
point at your script. When the TCP editor is opened with language set to `C#`,
the script template will be loaded automatically.

## Running startup scripts

To configure Git hooks and Git LFS, restore dependencies, and build the solution from a shell environment use:

```bash
chmod +x setup.sh
./setup.sh
```

This is helpful on CI hosts or when preparing a fresh development machine.

## Repository size audit

Run the following command to list the largest objects in the Git history:

```bash
git verify-pack -v .git/objects/pack/*.idx | sort -k3 -n | tail -20
```

No objects larger than 100 MB were found during the latest audit. If an object exceeds hosting limits, remove it using [git filter-repo](https://github.com/newren/git-filter-repo) or [BFG Repo-Cleaner](https://rtyley.github.io/bfg-repo-cleaner/), then force-push all refs:

```bash
git filter-repo --path path/to/large/file --invert-paths
git push --force-with-lease --all
git push --force-with-lease --tags
```


## License

This project is licensed under the [MIT License](LICENSE).
