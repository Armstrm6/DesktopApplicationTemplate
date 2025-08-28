#!/bin/bash
# Simple setup script to configure Git hooks, install Git LFS,
# restore dependencies, build the solution, and run tests
set -e

# Configure Git hooks path and install Git LFS
git config core.hooksPath .githooks
git lfs install

dotnet restore
# Determine the host operating system
OS_FAMILY=""
if [ -n "$OSTYPE" ]; then
    case "$OSTYPE" in
        msys*|cygwin*|win32*) OS_FAMILY="Windows" ;;
        *) OS_FAMILY="" ;;
    esac
fi

if [ -z "$OS_FAMILY" ]; then
    OS_FAMILY=$(dotnet --info | grep -m 1 '^OS Name:' | cut -d: -f2 | xargs)
fi

if [ -z "$SKIP_WORKLOAD" ]; then
    if [[ "$OS_FAMILY" == "Windows" ]]; then
        echo "Checking for wpf workload on Windows..."
        if ! dotnet workload list | grep -q wpf; then
            echo "Installing wpf workload..."
            dotnet workload install wpf
        else
            echo "wpf workload already installed."
        fi
    else
        echo "Non-Windows OS ($OS_FAMILY) detected; skipping wpf workload installation."
    fi
else
    echo "SKIP_WORKLOAD set - skipping wpf workload installation."
fi
dotnet build DesktopApplicationTemplate.sln
dotnet test DesktopApplicationTemplate.Tests/DesktopApplicationTemplate.Tests.csproj

echo "Setup complete."
