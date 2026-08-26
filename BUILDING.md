# Building SelectiveTrickplay Plugin

## Overview

The SelectiveTrickplay plugin requires building from source as Jellyfin plugin packages are not published to standard NuGet feeds.

## Prerequisites

- **Jellyfin Server 10.9.0 or later** (installed locally on your build machine)
- **.NET 8.0 SDK** or later
- **Git** (for cloning the repository)

## Step 1: Install Jellyfin Locally

### Windows
1. Download Jellyfin from https://jellyfin.org/downloads/
2. Install to default location: `C:\Program Files\Jellyfin\Server\`
3. Verify installation contains these DLLs:
   - `MediaBrowser.Controller.dll`
   - `MediaBrowser.Model.dll`
   - `MediaBrowser.Common.dll`

### Linux
```bash
# Ubuntu/Debian
sudo apt-get install jellyfin

# Or from Docker
docker run -it jellyfin/jellyfin:10.9.0
```

Installation typically puts DLLs in:
- `/usr/share/jellyfin/bin/` (system install)
- Or extract from container if using Docker

### macOS
```bash
brew install jellyfin
```

## Step 2: Clone Repository

```bash
git clone https://github.com/JustMike13/SelectiveTrickplay.git
cd SelectiveTrickplay
```

## Step 3: Update DLL Paths

Edit `SelectiveTrickplay.csproj` and update the `<HintPath>` values to match your Jellyfin installation:

### Windows Example
```xml
<Reference Include="MediaBrowser.Controller">
  <HintPath>C:\Program Files\Jellyfin\Server\MediaBrowser.Controller.dll</HintPath>
  <Private>False</Private>
</Reference>
<Reference Include="MediaBrowser.Model">
  <HintPath>C:\Program Files\Jellyfin\Server\MediaBrowser.Model.dll</HintPath>
  <Private>False</Private>
</Reference>
<Reference Include="MediaBrowser.Common">
  <HintPath>C:\Program Files\Jellyfin\Server\MediaBrowser.Common.dll</HintPath>
  <Private>False</Private>
</Reference>
```

### Linux Example
```xml
<Reference Include="MediaBrowser.Controller">
  <HintPath>/usr/share/jellyfin/bin/MediaBrowser.Controller.dll</HintPath>
  <Private>False</Private>
</Reference>
<!-- etc. -->
```

## Step 4: Build

```bash
# Restore dependencies
dotnet restore

# Build Release configuration
dotnet build -c Release
```

## Step 5: Output

The compiled DLL will be at:
```
bin/Release/net8.0/SelectiveTrickplay.dll
```

## Step 6: Deploy

Copy the DLL to your Jellyfin plugins directory:

### Windows
```
C:\Users\[YourUsername]\AppData\Roaming\Jellyfin\plugins\SelectiveTrickplay\SelectiveTrickplay.dll
```

### Linux
```bash
cp bin/Release/net8.0/SelectiveTrickplay.dll ~/.local/share/jellyfin/plugins/SelectiveTrickplay/
```

## Troubleshooting

### "Could not resolve reference to assembly"

**Solution:** Verify the `<HintPath>` in `.csproj` points to your actual Jellyfin installation.

```bash
# Find Jellyfin DLL locations
find / -name "MediaBrowser.Controller.dll" 2>/dev/null
```

### "Type or namespace not found"

**Solution:** Make sure you're using .NET 8.0 SDK:
```bash
dotnet --version  # Should show 8.0.x or higher
```

### Build succeeds but plugin doesn't load

**Common causes:**
- DLL not in correct plugins folder
- Jellyfin version mismatch (requires 10.9.0+)
- Missing dependencies in plugins folder

**Fix:**
1. Check Jellyfin logs for specific errors
2. Verify DLL is in: `<Jellyfin-Data>/plugins/SelectiveTrickplay/`
3. Restart Jellyfin

## Advanced: Docker Build

If you don't want to install Jellyfin locally:

```dockerfile
FROM jellyfin/jellyfin:10.9.0 as builder

RUN apt-get update && apt-get install -y dotnet-sdk-8.0 git

WORKDIR /build
RUN git clone https://github.com/JustMike13/SelectiveTrickplay.git .

# Update paths for Docker
RUN sed -i 's|C:\\.*MediaBrowser|/jellyfin/MediaBrowser|g' SelectiveTrickplay.csproj

# Build
RUN dotnet build -c Release

# Output: bin/Release/net8.0/SelectiveTrickplay.dll
```

## Next Steps

After building:
1. Follow [DEPLOYMENT.md](DEPLOYMENT.md) for installation
2. Configure plugin in Jellyfin dashboard
3. Run scheduled task to generate trickplay

## Support

If you encounter build issues:
1. Check this document's Troubleshooting section
2. Review [DEVELOPMENT.md](DEVELOPMENT.md)
3. Open an issue on GitHub with build output
