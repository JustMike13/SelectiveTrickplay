# Selective Trickplay Plugin for Jellyfin

## Overview

The **Selective Trickplay Generator** is a Jellyfin plugin that automatically generates trickplay (preview) thumbnails for movies and episodes based on watch status across selected users.

### Goal

Generate trickplay **only if ANY selected user has NOT watched** the movie/episode **AND** trickplay does not already exist.

## Features

- Multi-user watch status checking
- Selective trickplay generation based on unwatched content
- Scheduled task for automatic processing (default: daily at 03:00 AM)
- Comprehensive logging for monitoring and debugging
- Configuration UI for selecting target users

## Plugin Structure

```
SelectiveTrickplay/
├── SelectiveTrickplay.csproj         # Project file
├── plugin.json                        # Plugin metadata
├── Plugin.cs                          # Main plugin entry point
├── PluginServiceRegistration.cs       # Dependency injection setup
├── Configuration/
│   └── PluginConfiguration.cs         # Configuration storage
├── Tasks/
│   └── SelectiveTrickplayTask.cs      # Scheduled task implementation
├── Services/
│   └── TrickplayService.cs            # Trickplay management
└── Helpers/
    └── UserWatchHelper.cs             # User watch status checks
```

## Requirements

- **.NET:** 8.0 or later
- **Jellyfin:** 10.9.0 or later
- **NuGet Packages:**
  - `Jellyfin.Common` (10.9.0+)
  - `MediaBrowser.Controller` (10.9.0+)
  - `MediaBrowser.Model` (10.9.0+)
  - `MediaBrowser.Common` (10.9.0+)

## Building

### Build Requirements

**Important:** Jellyfin plugin development requires referencing Jellyfin's server assemblies directly. Follow these steps:

1. **Install Jellyfin Server 10.9.0+** on your development machine
2. **Update SelectiveTrickplay.csproj** to reference local Jellyfin DLLs instead of NuGet packages

Replace the current `<ItemGroup>` with package references:

```xml
<ItemGroup>
  <Reference Include="MediaBrowser.Controller">
    <HintPath>C:\Program Files\Jellyfin\Server\MediaBrowser.Controller.dll</HintPath>
  </Reference>
  <Reference Include="MediaBrowser.Model">
    <HintPath>C:\Program Files\Jellyfin\Server\MediaBrowser.Model.dll</HintPath>
  </Reference>
  <Reference Include="MediaBrowser.Common">
    <HintPath>C:\Program Files\Jellyfin\Server\MediaBrowser.Common.dll</HintPath>
  </Reference>
</ItemGroup>
```

Adjust paths to match your Jellyfin installation location.

### Prerequisites

Install .NET 8.0 SDK:
```bash
# Windows (via Chocolatey)
choco install dotnet-sdk-8.0

# macOS (via Homebrew)
brew install dotnet-sdk-8.0

# Linux (Ubuntu/Debian)
sudo apt-get install dotnet-sdk-8.0

# Or download from https://dotnet.microsoft.com/download/dotnet/8.0
```

### Build Steps

1. **Navigate to the project directory:**
   ```bash
   cd SelectiveTrickplay
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

3. **Build the plugin:**
   ```bash
   dotnet build -c Release
   ```

4. **Output location:**
   ```
   bin/Release/net8.0/SelectiveTrickplay.dll
   ```

## Installation

### Method 1: Manual Deployment

1. **Locate your Jellyfin data directory:**
   - **Windows:** `C:\Users\[YourUsername]\AppData\Roaming\Jellyfin\`
   - **Linux:** `~/.local/share/jellyfin/`
   - **Docker:** Mount the data volume and locate it accordingly

2. **Create plugin directory:**
   ```bash
   mkdir -p <Jellyfin-Data>/plugins/SelectiveTrickplay
   ```

3. **Copy the DLL:**
   ```bash
   cp bin/Release/net8.0/SelectiveTrickplay.dll <Jellyfin-Data>/plugins/SelectiveTrickplay/
   ```

4. **Restart Jellyfin:**
   - Via GUI: Admin Dashboard → Restart Server
   - Via command line: `systemctl restart jellyfin` (Linux) or restart via Services (Windows)

### Method 2: Via Jellyfin Plugin Repository

Once approved, the plugin can be installed directly from Jellyfin's plugin repository:
1. Go to **Dashboard → Plugins → Catalog**
2. Search for "Unwatched Trickplay"
3. Click **Install**
4. Restart Jellyfin

## Configuration

### Setting Up Selected Users

1. **Access Plugin Settings:**
   - Navigate to **Dashboard → Plugins → Installed**
   - Click **Settings** on "Selective Trickplay Generator"

2. **Select Users:**
   - Choose one or more users whose unwatched content should have trickplay generated
   - Multi-select is supported

3. **Save Configuration:**
   - Click **Save**
   - Configuration is persisted automatically

### Running the Scheduled Task

**Automatic Execution:**
- By default, runs daily at 03:00 AM
- Can be modified in **Dashboard → Scheduled Tasks**

**Manual Execution:**
1. Go to **Dashboard → Scheduled Tasks**
2. Find "Generate Trickplay for Unwatched Content"
3. Click **Run Now**
4. Monitor progress in the logs

## Logic

### Task Execution Flow

For each movie and episode in the library:

1. **Skip if trickplay exists** - Don't regenerate
2. **Check watch status** - For each selected user:
   - If **ANY** user has NOT watched → Generate trickplay
   - If **ALL** users have watched → Skip
3. **Log results** - Track processing for monitoring

### Pseudocode

```csharp
foreach item in LibraryItems(Movies, Episodes):
    if HasTrickplay(item):
        continue  // Already has preview
    
    foreach user in SelectedUsers:
        if !HasUserWatched(item, user):
            GenerateTrickplay(item)
            break  // Proceed to next item
```

## Logging

The plugin provides comprehensive logging to help monitor and troubleshoot:

**Log Locations:**
- **Windows:** `C:\Users\[Username]\AppData\Roaming\Jellyfin\logs\`
- **Linux:** `~/.local/share/jellyfin/logs/`

**Log Entries Include:**
- Task start/completion
- Selected users count
- Items processed count
- Items generated count
- Items skipped (with reasons)
- Error details

**Example Log Output:**
```
[SelectiveTrickplayTask] Starting Selective Trickplay generation task
[SelectiveTrickplayTask] Processing trickplay for 2 selected users
[SelectiveTrickplayTask] Found 1250 movies and episodes to process
[TrickplayService] Starting trickplay generation for item 12345: Movie Name
[TrickplayService] Successfully generated trickplay for item 12345: Movie Name
[SelectiveTrickplayTask] Selective Trickplay task completed. Total items: 1250, Generated: 45, Skipped (existing trickplay): 800, Skipped (all watched): 405
```

## Edge Cases Handled

| Scenario | Behavior |
|----------|----------|
| No users selected | Task skips without processing |
| Trickplay already exists | Item is skipped |
| All selected users have watched | Item is skipped |
| Any selected user hasn't watched | Trickplay is generated |
| Trickplay generation fails | Error is logged; task continues |
| Invalid user ID in config | Warning logged; invalid ID skipped |
| Cancellation requested | Task stops gracefully |

## Optional Enhancements (Future)

- Add setting: "Generate trickplay only for NEW items"
- Add setting: "Regenerate trickplay if watched status changes"
- Add setting: "Limit generation to X items per run"
- Add setting: "Skip items longer than N minutes"
- Web UI for configuration instead of JSON editing
- Statistics dashboard showing generation history

## Troubleshooting

### Plugin Not Appearing in Settings

1. Verify DLL is in correct directory: `<Jellyfin-Data>/plugins/UnwatchedTrickplay/`
2. Check Jellyfin logs for loading errors
3. Restart Jellyfin service
4. Verify .NET 8.0 runtime is installed

### Task Not Running

1. Check if users are selected in plugin configuration
2. Verify scheduled task is enabled in Dashboard
3. Check logs for errors during execution
4. Manually run task via Dashboard → Scheduled Tasks

### Trickplay Not Being Generated

1. Verify selected users are configured
2. Check if items are actually unwatched for selected users
3. Ensure adequate disk space for trickplay files
4. Review logs for generation errors
5. Verify Jellyfin has permissions to write trickplay files

### Jellyfin Won't Start After Plugin Installation

1. Remove the DLL from plugins directory
2. Restart Jellyfin
3. Check logs for more detailed error information
4. Verify .NET 8.0 compatibility

## License

MIT License - See LICENSE file for details

## Support

For issues, feature requests, or documentation updates:
- GitHub Issues: [https://github.com/example/selective-trickplay/issues](https://github.com/example/selective-trickplay/issues)
- Jellyfin Forums: [https://jellyfin.org/](https://jellyfin.org/)

## Version History

### 1.0.0 (Initial Release)

- Core functionality for selective trickplay generation
- Multi-user watch status checking
- Scheduled task with configurable users
- Comprehensive logging
- Plugin configuration UI support
