using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SelectiveTrickplay.Configuration;
using SelectiveTrickplay.Helpers;
using SelectiveTrickplay.Services;
using SelectiveTrickplay.Tasks;

namespace SelectiveTrickplay
{
    /// <summary>
    /// Service collection extensions for registering plugin services.
    /// </summary>
    public static class PluginServiceRegistration
    {
        /// <summary>
        /// Registers the plugin services in the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddUnwatchedTrickplayPlugin(this IServiceCollection services)
        {
            // Register the plugin configuration
            services.AddSingleton(_ => Plugin.Instance?.Configuration ?? new PluginConfiguration());

            // Register helper services
            services.AddScoped<UserWatchHelper>();

            // Register business services
            services.AddScoped<TrickplayService>();

            // Register scheduled task
            services.AddScoped<UnwatchedTrickplayTask>();

            return services;
        }
    }
}
