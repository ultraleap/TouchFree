using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Service.Connection;

using Leap;

namespace Ultraleap.TouchFree.Service
{
    public class Startup
    {
        ClientConnectionManager clientConnectionManager;
        HandManager handManager;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUpdateBehaviour();
            services.AddConfigFileWatcher();

            services.AddTrackingConnectionManager();
            services.AddHandManager();

            services.AddClientConnectionManager();
            services.AddWebSocketReceiver();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWebSockets();

            app.UseTouchFreeRouter();

            clientConnectionManager = app.ApplicationServices.GetService<ClientConnectionManager>();
            handManager = app.ApplicationServices.GetService<HandManager>();
            handManager.HandsUpdated += UpdateHand;
            app.ApplicationServices.GetService<UpdateBehaviour>().OnUpdate += Update;

            // This is here so the test infrastructure has some sign that the app is ready
            Console.WriteLine("Service Setup Complete");

            Console.WriteLine("TouchFree physical config screen height is: " + ConfigManager.PhysicalConfig.ScreenHeightM);
        }

        #region Temporary test methods
        Hand primaryHand;
        void UpdateHand(Hand primary, Hand secondary)
        {
            primaryHand = primary;
        }

        void Update()
        {
            if(primaryHand != null)
            {
                Vector palmPos = primaryHand.PalmPosition / 1000;
                System.Numerics.Vector3 screenPos = VirtualScreen.virtualScreen.WorldPositionToVirtualScreen(new System.Numerics.Vector3(palmPos.x, palmPos.y, -palmPos.z), out _);

                Positions positions = new Positions(new System.Numerics.Vector2(screenPos.X, screenPos.Y), screenPos.Z);

               InputAction inputAction = new InputAction(
                    0,
                    InteractionType.PUSH,
                    HandType.PRIMARY,
                    handManager.primaryChirality,
                    InputType.MOVE,
                    positions,
                    0);

                clientConnectionManager.SendInputActionToWebsocket(inputAction);
            }
        }

        #endregion
    }
}
