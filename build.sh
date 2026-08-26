#!/bin/bash
# Build script for Unwatched Trickplay Plugin

echo "Building Unwatched Trickplay Plugin..."
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK not found. Please install .NET 8.0 SDK or later."
    echo "Download from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

# Restore packages
echo "[1/3] Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "Error during restore."
    exit 1
fi

# Build Release
echo "[2/3] Building Release configuration..."
dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "Error during build."
    exit 1
fi

# Output location
echo ""
echo "[3/3] Build Complete!"
echo ""
echo "Output: bin/Release/net8.0/UnwatchedTrickplay.dll"
echo ""
echo "Next steps:"
echo "1. Copy the DLL to: \$JELLYFIN_DATA/plugins/UnwatchedTrickplay/"
echo "2. Restart Jellyfin"
echo "3. Configure plugin settings"
echo ""
