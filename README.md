# DesktopApplicationTemplate

This repository contains a basic WPF UI application, a Windows Service and unit tests.

## Prerequisites

- .NET 8 SDK
- Windows OS is required to run the WPF UI and service projects.

## Build the solution

Restore NuGet packages and build all projects:

```bash
dotnet restore
dotnet build DesktopApplicationTemplate.sln
```

You can also execute `setup.sh` to restore dependencies, build the solution and
run the unit tests in a single step:

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

Run the background service (useful for development):

```bash
dotnet run --project DesktopApplicationTemplate.Service/DesktopApplicationTemplate.Service.csproj
```

## Execute unit tests

Use `dotnet test` to run the xUnit tests:

```bash
dotnet test DesktopApplicationTemplate.Tests/DesktopApplicationTemplate.Tests.csproj
```
