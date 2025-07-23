#!/bin/bash
set -e

# Simple setup script for Codex environment
# Installs .NET 8 SDK and restores project packages.

sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

dotnet restore DesktopApplicationTemplate.sln

# Optionally build the service (UI projects require Windows)
dotnet build DesktopApplicationTemplate.Service/DesktopApplicationTemplate.Service.csproj

echo "Setup complete."
