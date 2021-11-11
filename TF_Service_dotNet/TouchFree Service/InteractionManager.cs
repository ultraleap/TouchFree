using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Interactions;

using Ultraleap.TouchFree.Service.Connection;

namespace Ultraleap.TouchFree.Service
{
    public class InteractionManager
    {
        private AirPushInteraction airPush;
        private GrabInteraction grab;
        private HoverAndHoldInteraction hoverAndHold;
        private TouchPlanePushInteraction touchPlane;

        private readonly HandManager handManager;
        private readonly UpdateBehaviour updateBehaviour;
        private readonly ClientConnectionManager connectionManager;

        public InteractionManager(UpdateBehaviour _updateBehaviour, HandManager _handManager, ClientConnectionManager _connectionManager)
        {
            handManager = _handManager;
            updateBehaviour = _updateBehaviour;
            connectionManager = _connectionManager;

            airPush = new AirPushInteraction(handManager);
            grab = new GrabInteraction(handManager);
            hoverAndHold = new HoverAndHoldInteraction(handManager);
            touchPlane = new TouchPlanePushInteraction(handManager);

            // This will need to be swapped with a system to add/remove these listeners per the "active" interaction

            //updateBehaviour.OnUpdate += touchPlane.Update;
            //touchPlane.HandleInputAction += connectionManager.SendInputActionToWebsocket;
            //touchPlane.Enable();

            //updateBehaviour.OnUpdate += airPush.Update;
            //airPush.HandleInputAction += connectionManager.SendInputActionToWebsocket;
            //airPush.Enable();

            //updateBehaviour.OnUpdate += hoverAndHold.Update;
            //hoverAndHold.HandleInputAction += connectionManager.SendInputActionToWebsocket;
            //hoverAndHold.Enable();

            updateBehaviour.OnUpdate += grab.Update;
            grab.HandleInputAction += connectionManager.SendInputActionToWebsocket;
            grab.Enable();
        }
    }
}