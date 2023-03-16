using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Service.Connection;

namespace Ultraleap.TouchFree.Service
{
    public class Startup
    {
        InteractionManager interactionManager;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureTouchFreeServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWebSockets();
            app.UseStaticFiles("/settings");

            app.UseRouting();
            app.UseEndpoints(endpoint =>
            {
                endpoint.MapGet("/settings/{**path}", (HttpContext context) =>
                {
                    context.Response.Redirect("/settings/index.html");
                });
            });

            var configFileWatcher = app.ApplicationServices.GetService<ConfigFileWatcher>();
            var configManager = app.ApplicationServices.GetService<IConfigManager>();
            configManager.OnServiceConfigUpdated += ConfigManager_OnServiceConfigUpdated;

            app.UseTouchFreeRouter(configManager);

            interactionManager = app.ApplicationServices.GetService<InteractionManager>();

            // This is here so the test infrastructure has some sign that the app is ready
            TouchFreeLog.WriteLine("Service Setup Complete");
            TouchFreeLog.WriteLine("TouchFree physical config screen height is: " + configManager.PhysicalConfig.ScreenHeightMm + " mm");
        }

        // If we've detected a change to the IP/port we should be operating on, close.
        private void ConfigManager_OnServiceConfigUpdated(ServiceConfig config = null)
        {
            TouchFreeLog.WriteLine($"Warning! TouchFree IP/port config is different to the active IP/port: http://{config.ServiceIP}:{config.ServicePort}");
        }
    }
}
