using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
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
        private readonly SelectiveTrickplayLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectiveTrickplayTask"/> class.
        /// </summary>
        /// <param name="libraryManager">Library manager used to query video items.</param>
        /// <param name="trickplayService">Service used to manage trickplay data.</param>
        /// <param name="userWatchHelper">Helper used to query user watch status.</param>
        /// <param name="logger">Plugin-specific logger.</param>
        public SelectiveTrickplayTask(
            ILibraryManager libraryManager,
            TrickplayService trickplayService,
            UserWatchHelper userWatchHelper,
            SelectiveTrickplayLogger logger)
        {
            _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
            _trickplayService = trickplayService ?? throw new ArgumentNullException(nameof(trickplayService));
            _userWatchHelper = userWatchHelper ?? throw new ArgumentNullException(nameof(userWatchHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the task name.
        /// </summary>
        public string Name => "Generate Trickplay for Unwatched Content";

        /// <summary>
        /// Gets the task key.
        /// </summary>
        public string Key => "SelectiveTrickplayGenerator";

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
            return new TaskTriggerInfo[0];
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="progress">Progress reporter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Selective Trickplay generation task");

            var configuration = Plugin.Instance?.Configuration;
            if (configuration is null)
            {
                _logger.LogError("Plugin configuration is unavailable. Task will not proceed.");
                return;
            }

            // Validate configuration
            if (configuration.SelectedUserIds == null || configuration.SelectedUserIds.Count == 0)
            {
                _logger.LogWarning("No users selected for trickplay generation. Task will not proceed.");
                return;
            }

            var selectedUserIds = new List<Guid>();
            foreach (var selectedUserId in configuration.SelectedUserIds)
            {
                if (Guid.TryParse(selectedUserId, out var userId))
                {
                    selectedUserIds.Add(userId);
                }
                else
                {
                    _logger.LogWarning(string.Format("Skipping invalid selected user ID {0}", selectedUserId));
                }
            }

            if (selectedUserIds.Count == 0)
            {
                _logger.LogWarning("No valid users selected for trickplay generation. Task will not proceed.");
                return;
            }

            var videos = _libraryManager
                .GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode }
                })
                .OfType<Video>()
                .Where(video => video.SupportsPlayedStatus)
                .ToList();

            _logger.LogInformation(string.Format(
                "Processing {0} video items for {1} selected users",
                videos.Count,
                selectedUserIds.Count));

            for (var index = 0; index < videos.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var video = videos[index];
                _logger.LogInformation(string.Format("Inspecting media item {0} ({1})", video.Name, video.Id));

                var hasUnwatchedSelectedUser = selectedUserIds.Any(userId => !_userWatchHelper.HasUserWatched(video, userId));
                if (!hasUnwatchedSelectedUser)
                {
                    _logger.LogInformation(string.Format(
                        "Skipped media item {0} ({1}): all selected users have played it",
                        video.Name,
                        video.Id));
                }
                else if (await _trickplayService.HasTrickplayAsync(video, cancellationToken).ConfigureAwait(false))
                {
                    _logger.LogInformation(string.Format(
                        "Skipped media item {0} ({1}): trickplay already exists",
                        video.Name,
                        video.Id));
                }
                else if (await _trickplayService.GenerateTrickplayAsync(video, cancellationToken).ConfigureAwait(false))
                {
                    _logger.LogInformation(string.Format(
                        "Generated trickplay for media item {0} ({1})",
                        video.Name,
                        video.Id));
                }
                else
                {
                    _logger.LogError(string.Format(
                        "Failed to generate trickplay for media item {0} ({1})",
                        video.Name,
                        video.Id));
                }

                progress.Report((index + 1) * 100d / videos.Count);
            }

            _logger.LogInformation(string.Format("Selective Trickplay task completed after inspecting {0} video items.", videos.Count));
        }
    }
}
