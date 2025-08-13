#!/bin/bash
# Simple setup script to configure Git hooks, install Git LFS,
# restore dependencies, build the solution, and run tests
set -e

# Configure Git hooks path and install Git LFS
git config core.hooksPath .githooks
git lfs install

dotnet restore

if [ -z "$SKIP_WORKLOAD" ]; then
    if [ "$OS" = "Windows_NT" ]; then
        echo "Checking for wpf workload..."
        if ! dotnet workload list | grep -q wpf; then
            echo "Installing wpf workload..."
            dotnet workload install wpf
        else
            echo "wpf workload already installed."
        fi
    else
        echo "Non-Windows environment detected - skipping wpf workload installation."
    fi
else
    echo "SKIP_WORKLOAD set - skipping wpf workload installation."
fi
dotnet build DesktopApplicationTemplate.sln
dotnet test DesktopApplicationTemplate.Tests/DesktopApplicationTemplate.Tests.csproj
dotnet test DesktopApplicationTemplate.Tests.Codex/DesktopApplicationTemplate.Tests.Codex.csproj

echo "Setup complete."
