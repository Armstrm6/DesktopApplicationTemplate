#!/bin/bash
# Simple setup script to restore, build, and test the solution
set -e

dotnet restore

if [ -z "$SKIP_WORKLOAD" ]; then
    echo "Checking for windowsdesktop workload..."
    if ! dotnet workload list | grep -q windowsdesktop; then
        echo "Installing windowsdesktop workload..."
        dotnet workload install windowsdesktop
    else
        echo "windowsdesktop workload already installed."
    fi
else
    echo "SKIP_WORKLOAD set - skipping windowsdesktop workload installation." 
fi
dotnet build DesktopApplicationTemplate.sln
dotnet test DesktopApplicationTemplate.Tests/DesktopApplicationTemplate.Tests.csproj
dotnet test DesktopApplicationTemplate.Tests.Codex/DesktopApplicationTemplate.Tests.Codex.csproj

echo "Setup complete."
