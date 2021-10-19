using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ultraleap.TouchFree.Service.Connection
{
    public static class WebSocketServerMiddlewareExtensions
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
    }
}