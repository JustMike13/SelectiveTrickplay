using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using SelectiveTrickplay.Configuration;
using SelectiveTrickplay.Helpers;
using SelectiveTrickplay.Services;

namespace SelectiveTrickplay.Tasks
{
    /// <summary>
    /// Scheduled task for generating trickplay for unwatched content based on selected users.
    /// </summary>
    public class SelectiveTrickplayTask : IScheduledTask
    {
        private readonly ILibraryManager _libraryManager;
        private readonly TrickplayService _trickplayService;
        private readonly UserWatchHelper _userWatchHelper;
        private readonly PluginConfiguration _configuration;
        private readonly ILogger<UnwatchedTrickplayTask> _logger;

        public SelectiveTrickplayTask(
            ILibraryManager libraryManager,
            TrickplayService trickplayService,
            UserWatchHelper userWatchHelper,
            PluginConfiguration configuration,
            ILogger<SelectiveTrickplayTask> logger)
        {
            _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
            _trickplayService = trickplayService ?? throw new ArgumentNullException(nameof(trickplayService));
            _userWatchHelper = userWatchHelper ?? throw new ArgumentNullException(nameof(userWatchHelper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the task name.
        /// </summary>
        public string Name => "Generate Trickplay for Unwatched Content";

        /// <summary>
        /// Gets the task description.
        /// </summary>
        public string Description => "Generates trickplay thumbnails for movies and episodes that are unwatched by at least one selected user.";

        /// <summary>
        /// Gets the task category.
        /// </summary>
        public string Category => "Library";

        /// <summary>
        /// Gets the default triggers for the task.
        /// </summary>
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new[]
            {
                new TaskTriggerInfo
                {
                    Type = TaskTriggerType.DailyTrigger,
                    TimeOfDayTicks = TimeSpan.FromHours(3).Ticks // 03:00 AM
                }
            };
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Unwatched Trickplay generation task");

            // Validate configuration
            if (_configuration.SelectedUserIds == null || _configuration.SelectedUserIds.Count == 0)
            {
                _logger.LogWarning("No users selected for trickplay generation. Task will not proceed.");
                return;
            }

            _logger.LogInformation("Processing trickplay for {SelectedUserCount} selected users", _configuration.SelectedUserIds.Count);

            // Convert selected user IDs to Guids
            var selectedUserGuids = new List<Guid>();
            foreach (var userIdString in _configuration.SelectedUserIds)
            {
                if (Guid.TryParse(userIdString, out var userGuid))
                {
                    selectedUserGuids.Add(userGuid);
                }
                else
                {
                    _logger.LogWarning("Invalid user ID format: {UserId}", userIdString);
                }
            }

            if (selectedUserGuids.Count == 0)
            {
                _logger.LogWarning("No valid user IDs found in configuration.");
                return;
            }

            // Query all movies and episodes
            var query = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode },
                Recursive = true,
                IsFolder = false
            };

            var items = _libraryManager.GetItemList(query);
            _logger.LogInformation("Found {ItemCount} movies and episodes to process", items.Count);

            int processedCount = 0;
            int skippedExisting = 0;
            int skippedWatched = 0;
            int generatedCount = 0;

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip if trickplay already exists
                if (_trickplayService.HasTrickplay(item))
                {
                    _logger.LogDebug("Skipping item {ItemId}: {ItemName} - trickplay already exists", item.Id, item.Name);
                    skippedExisting++;
                    continue;
                }

                // Check if ANY selected user has NOT watched this item
                bool shouldGenerate = false;
                foreach (var userId in selectedUserGuids)
                {
                    if (!_userWatchHelper.HasUserWatched(item, userId))
                    {
                        shouldGenerate = true;
                        _logger.LogDebug("User {UserId} has not watched item {ItemId}: {ItemName}", userId, item.Id, item.Name);
                        break;
                    }
                }

                if (!shouldGenerate)
                {
                    _logger.LogDebug("Skipping item {ItemId}: {ItemName} - all selected users have watched it", item.Id, item.Name);
                    skippedWatched++;
                    continue;
                }

                // Generate trickplay
                _logger.LogInformation("Generating trickplay for item {ItemId}: {ItemName}", item.Id, item.Name);
                await _trickplayService.GenerateTrickplay(item, cancellationToken).ConfigureAwait(false);
                generatedCount++;

                processedCount++;

                // Report progress
                if (items.Count > 0)
                {
                    progress?.Report((processedCount * 100.0) / items.Count);
                }
            }

            _logger.LogInformation(
                "Unwatched Trickplay task completed. " +
                "Total items: {TotalItems}, Generated: {Generated}, " +
                "Skipped (existing trickplay): {SkippedExisting}, " +
                "Skipped (all watched): {SkippedWatched}",
                items.Count, generatedCount, skippedExisting, skippedWatched);
        }
    }
}
