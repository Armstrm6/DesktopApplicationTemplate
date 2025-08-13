# DesktopApplicationTemplate

This repository contains a basic WPF UI application, a Windows Service and unit tests.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (8.0.404)
- Windows OS is required to run the WPF UI and service projects.

This repository includes a `global.json` file that pins the required
SDK version. If multiple SDKs are installed, the `dotnet` CLI will use
version `8.0.404` as specified in this file. Running commands from the
repository root will automatically respect this setting.

## Initial setup

After cloning the repository, run the setup script to configure the Git
hooks and [Git LFS](https://git-lfs.com/). The script also restores
dependencies, builds the solution, and executes the unit tests:

```bash
./setup.sh
```

Run the script any time the project dependencies or hooks need to be refreshed.

## Build the solution

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
dependencies, build the solution and run the unit tests in a single step:

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

## Execute unit tests

Use `dotnet test` to run the xUnit tests. The repository includes a
`tests.runsettings` file that ensures the entire test suite runs even
when some tests fail:

```bash
dotnet test DesktopApplicationTemplate.Tests/DesktopApplicationTemplate.Tests.csproj --settings tests.runsettings
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

To configure Git hooks and Git LFS, restore dependencies, build the solution
and run the unit tests from a shell environment use:

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
