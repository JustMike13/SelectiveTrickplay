using System;
using Jellyfin.Plugin.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.DependencyInjection;
using SelectiveTrickplay.Configuration;

namespace SelectiveTrickplay
{
    /// <summary>
    /// Main plugin class for Selective Trickplay Generator.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        /// <inheritdoc />
        public override string Name => "Selective Trickplay Generator";

        /// <inheritdoc />
        public override Guid Id => Guid.Parse("12345678-1234-1234-1234-123456789012");

        /// <summary>
        /// Gets the current plugin instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "Selective Trickplay Configuration",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                }
            };
        }
    }
}
