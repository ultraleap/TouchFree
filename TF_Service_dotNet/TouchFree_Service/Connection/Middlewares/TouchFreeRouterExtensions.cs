using Microsoft.AspNetCore.Builder;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Service.Connection.Middlewares;

public static class TouchFreeRouterExtensions
{
    public static IApplicationBuilder UseTouchFreeRouter(this IApplicationBuilder builder, IConfigManager configManager) =>
        builder.UseMiddleware<TouchFreeRouter>(configManager);
}