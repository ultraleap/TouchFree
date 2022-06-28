using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;

namespace Ultraleap.TouchFree.Service.Connection
{
    public static class TouchFreeRouterExtensions
    {
        public static IApplicationBuilder UseTouchFreeRouter(this IApplicationBuilder builder, IConfigManager configManager)
        {
            return builder.UseMiddleware<TouchFreeRouter>(configManager);
        }

        public static IServiceCollection AddClientConnectionManager(this IServiceCollection services)
        {
            services.AddSingleton<ClientConnectionManager>();
            services.AddSingleton<IClientConnectionManager>(x => x.GetService<ClientConnectionManager>());
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
            services.AddSingleton<ITrackingConnectionManager>(x => x.GetService<TrackingConnectionManager>());
            return services;
        }

        public static IServiceCollection AddConfig(this IServiceCollection services)
        {
            var configManager = new ConfigManager();
            services.AddSingleton<IConfigManager>(configManager);
            services.AddSingleton<IQuickSetupHandler, QuickSetupHandler>();
            var watcher = new ConfigFileWatcher(configManager);

            services.BuildServiceProvider().GetService<UpdateBehaviour>().OnUpdate += watcher.Update;

            services.AddSingleton(watcher);

            return services;
        }

        public static IServiceCollection AddHandManager(this IServiceCollection services)
        {
            services.AddSingleton<IHandManager, HandManager>();
            return services;
        }

        public static IServiceCollection AddInteractions(this IServiceCollection services)
        {
            services.AddSingleton<InteractionManager>();

            services.AddSingleton<IInteraction, AirPushInteraction>();
            services.AddSingleton<IInteraction, GrabInteraction>();
            services.AddSingleton<IInteraction, HoverAndHoldInteraction>();
            services.AddSingleton<IInteraction, TouchPlanePushInteraction>();

            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
            services.Configure<InteractionTuning>(configuration.GetSection(nameof(InteractionTuning)));

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