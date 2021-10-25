using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Service.Connection;

using Leap;

namespace Ultraleap.TouchFree.Service
{
    public class Startup
    {
        Library.TrackingConnectionManager trackingConnectionManager;
        ClientConnectionManager clientConnectionManager;

        LeapTransform trackingTransform;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUpdateBehaviour();
            services.AddConfigFileWatcher();

            services.AddTrackingConnectionManager();

            services.AddClientConnectionManager();
            services.AddWebSocketReceiver();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWebSockets();

            app.UseTouchFreeRouter();

            trackingConnectionManager = app.ApplicationServices.GetService<Library.TrackingConnectionManager>();
            clientConnectionManager = app.ApplicationServices.GetService<ClientConnectionManager>();

            app.ApplicationServices.GetService<UpdateBehaviour>().OnUpdate += Update;

            // This is here so the test infrastructure has some sign that the app is ready
            Console.WriteLine("Service Setup Complete");

            Console.WriteLine("TouchFree physical config screen height is: " + ConfigManager.PhysicalConfig.ScreenHeightM);
        }

        void Update()
        {
            UpdateTrackingTransform();

            if (trackingConnectionManager.controller.Frame().Hands.Count > 0)
            {
                Vector palmPos = trackingConnectionManager.controller.GetTransformedFrame(trackingTransform).Hands[0].PalmPosition / 1000;
                System.Numerics.Vector3 screenPos = Library.VirtualScreen.virtualScreen.WorldPositionToVirtualScreen(new System.Numerics.Vector3(palmPos.x, palmPos.y, -palmPos.z), out _);

                Library.Positions positions = new Library.Positions(new System.Numerics.Vector2(screenPos.X, screenPos.Y), screenPos.Z);

                Library.InputAction inputAction = new Library.InputAction(
                    0,
                    Library.InteractionType.PUSH,
                    Library.HandType.PRIMARY,
                    Library.HandChirality.RIGHT,
                    Library.InputType.MOVE,
                    positions,
                    0);

                clientConnectionManager.SendInputActionToWebsocket(inputAction);
            }
        }

        void UpdateTrackingTransform()
        {
            // To simplify the configuration values, positive X angles tilt the Leap towards the screen no matter how its mounted.
            // Therefore, we must convert to the real values before using them.
            // If top mounted, the X rotation should be negative if tilted towards the screen so we must negate the X rotation in this instance.
            var isTopMounted = ((ConfigManager.PhysicalConfig.LeapRotationD.Z > 179.9f) && (ConfigManager.PhysicalConfig.LeapRotationD.Z < 180.1f));
            float xAngleDegree = isTopMounted ? -ConfigManager.PhysicalConfig.LeapRotationD.X : ConfigManager.PhysicalConfig.LeapRotationD.X;

            System.Numerics.Quaternion quaternion = System.Numerics.Quaternion.CreateFromYawPitchRoll(Library.VirtualScreen.DegreesToRadians(ConfigManager.PhysicalConfig.LeapRotationD.Y),
                Library.VirtualScreen.DegreesToRadians(-xAngleDegree),
                Library.VirtualScreen.DegreesToRadians(ConfigManager.PhysicalConfig.LeapRotationD.Z));

            trackingTransform = new LeapTransform(new Vector(ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.X,
                ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.Y,
                -ConfigManager.PhysicalConfig.LeapPositionRelativeToScreenBottomM.Z) * 1000, new
                LeapQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W));
        }
    }
}
