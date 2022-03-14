using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

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

        public static IServiceCollection AddInteractions(this IServiceCollection services)
        {
            services.AddSingleton<InteractionManager>();

            services.AddSingleton<AirPushInteraction>();
            services.AddSingleton<GrabInteraction>();
            services.AddSingleton<HoverAndHoldInteraction>();
            services.AddSingleton<TouchPlanePushInteraction>();

            return services;
        }

        public static IServiceCollection AddVirtualScreen(this IServiceCollection services)
        {
            services.AddSingleton<IVirtualScreen, VirtualScreen>();
            return services;
        }

        public static IServiceCollection AddPositioning(this IServiceCollection services)
        {
            // Using transient here so we create new instances for the different interactions
            services.AddTransient<IPositioningModule, PositioningModule>();
            services.AddTransient<IPositionStabiliser, PositionStabiliser>();

            services.AddSingleton<IPositionTracker, IndexStableTracker>();
            services.AddSingleton<IPositionTracker, IndexTipTracker>();
            services.AddSingleton<IPositionTracker, NearestTracker>();
            services.AddSingleton<IPositionTracker, WristTracker>();

            return services;
        }
    }
}