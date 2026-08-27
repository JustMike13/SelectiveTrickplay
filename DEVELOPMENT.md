# Development Guide

## Local Development Setup

### Prerequisites

- .NET 8.0 SDK (or later)
- Visual Studio 2022, Visual Studio Code, or JetBrains Rider
- A development Jellyfin instance

### Setting Up Development Environment

1. **Clone/Open the repository**
   ```bash
   cd SelectiveTrickplay
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build for development**
   ```bash
   dotnet build -c Debug
   ```

## Project Structure

```
SelectiveTrickplay/
├── Configuration/
│   ├── PluginConfiguration.cs      # Config model
│   └── configPage.html             # Settings UI
├── Helpers/
│   └── UserWatchHelper.cs          # Watch status queries
├── Services/
│   └── TrickplayService.cs         # Trickplay management
├── Tasks/
│   └── SelectiveTrickplayTask.cs   # Main scheduled task
├── Plugin.cs                        # Plugin entry point
├── PluginServiceRegistration.cs    # Dependency injection
├── SelectiveTrickplay.csproj       # Project configuration
└── plugin.json                      # Plugin metadata
```

## Key Classes

### Plugin.cs
Main entry point for Jellyfin plugin loader. Handles:
- Plugin initialization
- Configuration management
- Web page registration

### PluginServiceRegistration.cs
Dependency injection setup using `IServiceCollection`.

### PluginConfiguration.cs
Stores configuration in Jellyfin's database. Persists:
- Selected user IDs
- Any future settings

### UserWatchHelper.cs
Queries Jellyfin's user data:
- Gets user by ID
- Retrieves play data for item
- Checks watched status

### TrickplayService.cs
Manages trickplay operations:
- Checks if trickplay exists
- Generates new trickplay
- Error handling and logging

### UnwatchedTrickplayTask.cs
Scheduled task implementation:
- Queries library for movies/episodes
- Applies watch status logic
- Triggers generation
- Provides progress and logging

## Debugging

### Visual Studio

1. Open `UnwatchedTrickplay.csproj`
2. Attach debugger to Jellyfin process:
   - Debug → Attach to Process
   - Find jellyfin.exe or dotnet.exe
3. Set breakpoints and run task

### Logging

Add debug logs in any class:
```csharp
_logger.LogDebug("Debug message: {Value}", debugValue);
_logger.LogInformation("Info message");
_logger.LogWarning("Warning message");
_logger.LogError(ex, "Error message");
```

View in Jellyfin dashboard logs or log files.

## Testing

### Manual Testing

1. Build plugin
2. Deploy to test Jellyfin instance
3. Configure users in plugin settings
4. Run scheduled task manually
5. Verify logs and trickplay generation

### Automated Testing (Future)

Add xUnit tests:
```bash
dotnet add package xunit
dotnet add package Moq
```

## Extending the Plugin

### Adding New Settings

1. Add property to `PluginConfiguration.cs`
2. Update `configPage.html` UI
3. Use setting in `SelectiveTrickplayTask.cs`

### Adding Filters/Options

Modify `InternalItemsQuery` in task:
```csharp
var query = new InternalItemsQuery
{
    IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode },
    Recursive = true,
    // Add more filters here
    MinCommunityRating = 5.0,  // Example
};
```

### Custom Logic

Override methods in helper/service classes or add new ones.

## NuGet Package Versions

Update via:
```bash
dotnet add package <PackageName> --version <Version>
```

Required packages:
- Jellyfin.Common
- MediaBrowser.Controller
- MediaBrowser.Model
- MediaBrowser.Common

All should match Jellyfin's target version (currently 10.11.0).

## Building for Release

```bash
dotnet build -c Release
# Output: bin/Release/net9.0/SelectiveTrickplay.zip
```

## Common Issues

### Type Not Found
- Verify correct namespace
- Check NuGet package version matches Jellyfin version
- Clean/rebuild project

### Logger Not Working
- Verify `ILogger<T>` dependency is injected
- Check class name matches in logs

### Configuration Not Saving
- Verify `PluginConfiguration` inherits from `BasePluginConfiguration`
- Check plugin ID matches in `plugin.json` and `Plugin.cs`

## Release Checklist

- [ ] Update version in plugin.json
- [ ] Update README.md changelog
- [ ] Build Release configuration
- [ ] Test with clean Jellyfin install
- [ ] Create GitHub release with DLL
- [ ] Update documentation if needed

## Code Style

- Follow C# naming conventions (PascalCase for public members)
- Use nullable reference types (`#nullable enable`)
- Add XML documentation comments for public APIs
- Use async/await for I/O operations
- Implement proper error handling and logging
