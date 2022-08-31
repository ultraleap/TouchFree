using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Connections;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Configuration.QuickSetup;
using Ultraleap.TouchFree.Library.Interactions;
using Ultraleap.TouchFree.Library.Interactions.PositionTrackers;
using Ultraleap.TouchFree.Library.Connections.MessageQueues;

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

        public static IServiceCollection AddMessageQueueHandlers(this IServiceCollection services)
        {
            services.AddSingleton<IMessageQueueHandler, ConfigurationChangeQueueHandler>();
            services.AddSingleton<IMessageQueueHandler, ConfigurationFileChangeQueueHandler>();
            services.AddSingleton<IMessageQueueHandler, ConfigurationStateRequestQueueHandler>();
            services.AddSingleton<IMessageQueueHandler, ConfigurationFileRequestQueueHandler>();
            services.AddSingleton<IMessageQueueHandler, QuickSetupQueueHandler>();
            services.AddSingleton<IMessageQueueHandler, ServiceStatusQueueHandler>();
            services.AddSingleton<IMessageQueueHandler, HandDataStreamStateQueueHandler>();
            services.AddSingleton<IMessageQueueHandler, TrackingApiChangeQueueHandler>();
            return services;
        }

        public static IServiceCollection AddUpdateBehaviour(this IServiceCollection services)
        {
            services.AddSingleton<IUpdateBehaviour, UpdateBehaviour>();
            return services;
        }

        public static IServiceCollection AddTrackingConnectionManager(this IServiceCollection services)
        {
            services.AddSingleton<ITrackingConnectionManager, TrackingConnectionManager>();
            return services;
        }

        public static IServiceCollection AddTrackingDiagnosticApi(this IServiceCollection services)
        {
            services.AddSingleton<ITrackingDiagnosticApi, TrackingDiagnosticApi>();
            return services;
        }

        public static IServiceCollection AddConfig(this IServiceCollection services)
        {
            services.AddSingleton<IConfigManager, ConfigManager>();
            services.AddSingleton<IQuickSetupHandler, QuickSetupHandler>();
            services.AddSingleton<ConfigFileWatcher>();

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
            services.AddSingleton<IInteraction, VelocitySwipeInteraction>();
            services.AddSingleton<IInteraction, AirClickInteraction>();

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
            services.AddSingleton<IPositionTracker, HandPointingTracker>();
            services.AddSingleton<IPositionTracker, ProjectionTracker>();

            return services;
        }
    }
}