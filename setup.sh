#!/bin/bash
# Simple setup script to configure Git hooks, install Git LFS,
# restore dependencies, build the solution, and run tests
set -e

# Configure Git hooks path and install Git LFS
git config core.hooksPath .githooks
git lfs install

dotnet restore
dotnet build DesktopApplicationTemplate.sln
dotnet test DesktopApplicationTemplate.Tests/DesktopApplicationTemplate.Tests.csproj
dotnet test DesktopApplicationTemplate.Tests.Codex/DesktopApplicationTemplate.Tests.Codex.csproj

echo "Setup complete."
