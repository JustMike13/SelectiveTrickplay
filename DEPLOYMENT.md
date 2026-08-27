# Deployment Guide

## Quick Start Deployment

### Prerequisites

- Jellyfin Server 10.11.0 or later
- Access to Jellyfin data directory
- Administrative access to Jellyfin configuration

### Step 1: Build the Plugin

#### Windows
```bash
cd SelectiveTrickplay
build.bat
```

#### macOS/Linux
```bash
cd SelectiveTrickplay
chmod +x build.sh
./build.sh
```

**Output file:** `bin/Release/net9.0/SelectiveTrickplay.zip`

### Step 2: Locate Jellyfin Data Directory

Jellyfin stores data in different locations based on your OS and setup:

**Windows (Default Installation):**
```
C:\Users\[YourUsername]\AppData\Roaming\Jellyfin\
```

**Windows (Docker):**
```
If using Docker, map the volume to find the data directory
```

**Linux (Default):**
```
~/.local/share/jellyfin/
```

**Linux (Docker):**
```
Volume mount → /config or /var/lib/jellyfin
```

**macOS:**
```
~/.config/jellyfin/
```

### Step 3: Create Plugin Directory

```bash
# Windows
mkdir "<Jellyfin-Data>\plugins\SelectiveTrickplay"

# Linux/macOS
mkdir -p "$JELLYFIN_DATA/plugins/SelectiveTrickplay"
```

### Step 4: Deploy the DLL

Copy `SelectiveTrickplay.dll` to the plugin directory:

```bash
# Windows
xcopy "SelectiveTrickplay\bin\Release\net9.0\SelectiveTrickplay.dll" "<Jellyfin-Data>\plugins\SelectiveTrickplay\"

# Linux/macOS
cp SelectiveTrickplay/bin/Release/net9.0/SelectiveTrickplay.dll "$JELLYFIN_DATA/plugins/SelectiveTrickplay/"
```

### Step 5: Restart Jellyfin

**Via Jellyfin Web UI:**
1. Go to Dashboard → Settings
2. Click "Restart Server"

**Via Command Line:**

Windows:
```powershell
Restart-Service jellyfin  # Requires admin
```

Linux:
```bash
sudo systemctl restart jellyfin
```

Docker:
```bash
docker restart jellyfin  # Replace 'jellyfin' with your container name
```

### Step 6: Verify Installation

1. Open Jellyfin Web UI
2. Go to **Dashboard → Plugins → Installed**
3. Look for "Selective Trickplay Generator"
4. If listed, click **Settings** to configure

## Configuration

### Select Users

1. Navigate to **Dashboard → Plugins → Installed**
2. Click **Settings** next to "Unwatched Trickplay Generator"
3. Select the users whose unwatched content should have trickplay generated
4. Click **Save**

### Schedule Configuration

The plugin runs on a schedule (default: 03:00 AM daily).

To modify the schedule:

1. Go to **Dashboard → Scheduled Tasks**
2. Find "Generate Trickplay for Unwatched Content"
3. Click to edit schedule
4. Adjust timing as needed
5. Enable/disable as desired

## Testing

### Manual Task Execution

1. Go to **Dashboard → Scheduled Tasks**
2. Find "Generate Trickplay for Unwatched Content"
3. Click **Run Now**
4. Monitor the progress indicator

### View Logs

**Web UI:**
1. Go to **Dashboard → Logs**
2. Filter for "UnwatchedTrickplayTask"
3. Review for any errors or warnings

**Log Files:**

Windows:
```
C:\Users\[YourUsername]\AppData\Roaming\Jellyfin\logs\
```

Linux:
```
~/.local/share/jellyfin/logs/
```

Look for files containing `UnwatchedTrickplay` or `TrickplayService`.

## Troubleshooting

### Plugin Not Appearing

**Issue:** Plugin doesn't show in installed plugins list

**Solutions:**
1. Verify DLL is in correct location: `<Jellyfin-Data>/plugins/SelectiveTrickplay/SelectiveTrickplay.dll`
2. Check Jellyfin logs for loading errors
3. Ensure .NET 8 runtime is installed:
   ```bash
   dotnet --version
   ```
4. Restart Jellyfin completely (not just web refresh)

### Task Not Running

**Issue:** Scheduled task doesn't execute

**Solutions:**
1. Verify users are selected in plugin settings
2. Check if task is enabled in Scheduled Tasks
3. Verify scheduled task status in logs
4. Try running manually via Dashboard

### Trickplay Not Generated

**Issue:** Files show in library but no trickplay generated

**Solutions:**
1. Check if users are configured in plugin settings
2. Verify content is actually unwatched for selected users
3. Ensure adequate disk space (trickplay files are large)
4. Check Jellyfin permissions can write to trickplay directory
5. Review logs for generation errors

### Permission Denied Errors

**Linux:**
```bash
# Make sure Jellyfin user can access plugins directory
sudo chown -R jellyfin:jellyfin /var/lib/jellyfin/plugins/
chmod -R 755 /var/lib/jellyfin/plugins/
```

### Plugin Causes Jellyfin to Crash

**Solutions:**
1. Remove the DLL from plugins directory
2. Restart Jellyfin
3. Check Jellyfin logs for specific errors
4. Verify .NET 8 runtime is properly installed
5. Try updating Jellyfin to latest version

## Updating the Plugin

1. Stop Jellyfin
2. Delete old DLL: `<Jellyfin-Data>/plugins/SelectiveTrickplay/`
3. Rebuild plugin with new code
4. Copy new DLL to plugins directory
5. Restart Jellyfin

## Uninstallation

1. Stop Jellyfin
2. Remove plugin directory:
   ```bash
   rm -rf "<Jellyfin-Data>/plugins/SelectiveTrickplay"
   ```
3. Restart Jellyfin

The plugin will no longer appear in the installed plugins list.

## Support

For detailed information on:
- Jellyfin plugin development: https://jellyfin.org/docs/general/server/plugins/
- Jellyfin administration: https://jellyfin.org/docs/general/administration/
- .NET SDK installation: https://dotnet.microsoft.com/download/dotnet/8.0

## Performance Considerations

- **First Run:** May take significant time depending on library size
- **Subsequent Runs:** Faster as existing trickplay is skipped
- **Resource Usage:** CPU and disk I/O intensive during generation
- **Timing:** Schedule during off-peak hours if possible
- **Limits:** Consider limiting items per run if server has limited resources (see Optional Enhancements in README)

## Database Backup

Before deploying:
```bash
# Backup Jellyfin data
cp -r "$JELLYFIN_DATA" "$JELLYFIN_DATA.backup"
```

This allows rollback if issues occur.
