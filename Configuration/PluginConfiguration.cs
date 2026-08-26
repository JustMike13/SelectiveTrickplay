using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace SelectiveTrickplay.Configuration
{
    /// <summary>
    /// Plugin configuration for storing selected user IDs.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets the list of selected user IDs for which unwatched content
        /// should have trickplay generated.
        /// </summary>
        public List<string> SelectedUserIds { get; set; } = new List<string>();
    }
}
