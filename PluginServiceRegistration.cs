using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using SelectiveTrickplay.Helpers;
using SelectiveTrickplay.Services;

namespace SelectiveTrickplay
{
    /// <summary>
    /// Registers services used by the plugin with Jellyfin's dependency injection container.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<SelectiveTrickplayLogger>();
            serviceCollection.AddSingleton<TrickplayService>();
            serviceCollection.AddSingleton<UserWatchHelper>();
        }
    }
}
