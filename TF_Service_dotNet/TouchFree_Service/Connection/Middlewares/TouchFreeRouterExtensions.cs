using Microsoft.AspNetCore.Builder;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connection;

namespace Ultraleap.TouchFree.Service.Connection
{
    public static class TouchFreeRouterExtensions
    {
        public static IApplicationBuilder UseTouchFreeRouter(this IApplicationBuilder builder, IWebSocketHandler webSocketHandler, ITouchFreeLogger logger)
        {
            return builder.UseMiddleware<TouchFreeRouter>(webSocketHandler, logger);
        }
    }
}