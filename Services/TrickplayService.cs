using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using Microsoft.Extensions.Logging;

namespace SelectiveTrickplay.Services
{
    /// <summary>
    /// Service for managing trickplay generation for items.
    /// </summary>
    public class TrickplayService
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<TrickplayService> _logger;
        private readonly ITrickplayManager _trickplayManager;

        public TrickplayService(
            ILibraryManager libraryManager,
            ITrickplayManager trickplayManager,
            ILogger<TrickplayService> logger)
        {
            _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
            _trickplayManager = trickplayManager ?? throw new ArgumentNullException(nameof(trickplayManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Checks if trickplay already exists for the specified item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if trickplay exists; otherwise false.</returns>
        public async Task<bool> HasTrickplayAsync(BaseItem item, CancellationToken cancellationToken)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var trickplayManifest = await _trickplayManager.GetTrickplayManifest(item).ConfigureAwait(false);
                return trickplayManifest.Values.Any(resolutions => resolutions.Count > 0);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking trickplay for item {ItemId}: {Message}", item.Id, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Generates trickplay for the specified item.
        /// </summary>
        /// <param name="video">The video to generate trickplay for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if generation completed; otherwise false.</returns>
        public async Task<bool> GenerateTrickplayAsync(Video video, CancellationToken cancellationToken)
        {
            if (video == null)
            {
                throw new ArgumentNullException(nameof(video));
            }

            try
            {
                // The task calls this only after confirming no trickplay data exists.
                // Force generation so selected media is processed even when library-wide
                // automatic trickplay extraction is disabled.
                await _trickplayManager.RefreshTrickplayDataAsync(
                    video,
                    true,
                    _libraryManager.GetLibraryOptions(video),
                    cancellationToken).ConfigureAwait(false);

                return await HasTrickplayAsync(video, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating trickplay for item {ItemId}: {ItemName}", video.Id, video.Name);
                return false;
            }
        }
    }
}
