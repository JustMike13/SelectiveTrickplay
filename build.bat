@echo off
REM Build script for Selective Trickplay Plugin

echo Building Selective Trickplay Plugin...
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo Error: .NET SDK not found. Please install .NET 9.0 SDK or later.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/9.0
    exit /b 1
)

REM Restore packages
echo [1/3] Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo Error during restore.
    exit /b 1
)

REM Build Release
echo [2/3] Building Release configuration...
dotnet build -c Release
if errorlevel 1 (
    echo Error during build.
    exit /b 1
)

REM Output location
echo.
echo [3/3] Build Complete!
echo.
echo Package: bin\Release\net9.0\SelectiveTrickplay.zip
echo.
echo Next steps:
echo 1. Upload SelectiveTrickplay.zip to the v1.0.0 GitHub release.
echo 2. Add the raw manifest URL to Jellyfin's plugin repositories.
echo 3. Refresh the catalog and install the plugin.
echo.
pause
