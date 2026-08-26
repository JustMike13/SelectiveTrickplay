using System;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

namespace SelectiveTrickplay.Helpers
{
    /// <summary>
    /// Helper class for determining user watch status on items.
    /// </summary>
    public class UserWatchHelper
    {
        private readonly IUserManager _userManager;

        public UserWatchHelper(IUserManager userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        /// <summary>
        /// Determines if a user has watched a specific item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="userId">The user ID to check.</param>
        /// <returns>True if the user has watched the item; otherwise false.</returns>
        public bool HasUserWatched(BaseItem item, Guid userId)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            }

            try
            {
                var user = _userManager.GetUserById(userId);
                if (user == null)
                {
                    return false;
                }

                // In Jellyfin 10.11+, UserData is a collection
                // Search for the user's data entry
                if (item.UserData != null)
                {
                    var userData = item.UserData.FirstOrDefault(ud => ud.UserId == userId);
                    if (userData != null)
                    {
                        return userData.Played || userData.PlayCount > 0;
                    }
                }

                return false;
            }
            catch
            {
                // If we can't determine watch status, assume not watched
                return false;
            }
        }
    }
}
