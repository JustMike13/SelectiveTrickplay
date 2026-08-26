using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Trickplay;
using Microsoft.Extensions.Logging;

namespace SelectiveTrickplay.Services
{
    /// <summary>
    /// Service for managing trickplay generation for items.
    /// </summary>
    public class TrickplayService
    {
        private readonly ITrickplayManager _trickplayManager;
        private readonly ILogger<TrickplayService> _logger;

        public TrickplayService(ITrickplayManager trickplayManager, ILogger<TrickplayService> logger)
        {
            _trickplayManager = trickplayManager ?? throw new ArgumentNullException(nameof(trickplayManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Checks if trickplay already exists for the specified item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if trickplay exists; otherwise false.</returns>
        public bool HasTrickplay(BaseItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            try
            {
                // Check if the item has trickplay data
                return _trickplayManager.GetTrickplayInfo(item) != null;
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
        /// <param name="item">The item to generate trickplay for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GenerateTrickplay(BaseItem item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            try
            {
                _logger.LogInformation("Starting trickplay generation for item {ItemId}: {ItemName}", item.Id, item.Name);
                await _trickplayManager.GenerateTrickplay(item, true, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Successfully generated trickplay for item {ItemId}: {ItemName}", item.Id, item.Name);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Trickplay generation cancelled for item {ItemId}: {ItemName}", item.Id, item.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating trickplay for item {ItemId}: {ItemName} - {Message}", item.Id, item.Name, ex.Message);
            }
        }
    }
}
