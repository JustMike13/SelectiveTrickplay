@echo off
REM Build script for Unwatched Trickplay Plugin

echo Building Unwatched Trickplay Plugin...
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo Error: .NET SDK not found. Please install .NET 8.0 SDK or later.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/8.0
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
echo Output: bin\Release\net8.0\UnwatchedTrickplay.dll
echo.
echo Next steps:
echo 1. Copy the DLL to: %%JELLYFIN_DATA%%\plugins\UnwatchedTrickplay\
echo 2. Restart Jellyfin
echo 3. Configure plugin settings
echo.
pause
