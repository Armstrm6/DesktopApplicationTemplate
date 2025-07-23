#!/bin/bash
# Simple setup script to restore, build, and test the solution
set -e

dotnet restore
dotnet build DesktopApplicationTemplate.sln
dotnet test DesktopApplicationTemplate.Tests/DesktopApplicationTemplate.Tests.csproj

echo "Setup complete."
