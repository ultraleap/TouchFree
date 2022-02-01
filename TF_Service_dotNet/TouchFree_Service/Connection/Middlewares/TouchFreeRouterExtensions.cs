using System.Linq;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service.Connection
{
    public static class TouchFreeRouterExtensions
    {
        public static IApplicationBuilder UseTouchFreeRouter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TouchFreeRouter>();
        }

        public static IServiceCollection AddClientConnectionManager(this IServiceCollection services)
        {
            services.AddSingleton<ClientConnectionManager>();
            return services;
        }

        public static IServiceCollection AddWebSocketReceiver(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketReceiver>();
            return services;
        }

        public static IServiceCollection AddUpdateBehaviour(this IServiceCollection services)
        {
            services.AddSingleton<UpdateBehaviour>();
            return services;
        }

        public static IServiceCollection AddTrackingConnectionManager(this IServiceCollection services)
        {
            services.AddSingleton<TrackingConnectionManager>();
            return services;
        }

        public static IServiceCollection AddConfig(this IServiceCollection services)
        {
            var configManager = new ConfigManager();
            services.AddSingleton<IConfigManager>(configManager);
            var watcher = new ConfigFileWatcher(configManager);

            services.BuildServiceProvider().GetService<UpdateBehaviour>().OnUpdate += watcher.Update;

            services.AddSingleton(watcher);

            return services;
        }

        public static IServiceCollection AddHandManager(this IServiceCollection services)
        {
            services.AddSingleton<HandManager>();
            return services;
        }

        public static IServiceCollection AddInteractionManager(this IServiceCollection services)
        {
            services.AddSingleton<InteractionManager>();
            return services;
        }

        public static IServiceCollection AddVirtualScreenManager(this IServiceCollection services)
        {
            services.AddSingleton<IVirtualScreenManager, VirtualScreenManager>();
            return services;
        }
    }
}