using Microsoft.AspNetCore.Builder;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service.Connection
{
    public static class TouchFreeRouterExtensions
    {
        public static IApplicationBuilder UseTouchFreeRouter(this IApplicationBuilder builder, IConfigManager configManager)
        {
            return builder.UseMiddleware<TouchFreeRouter>(configManager);
        }
    }
}