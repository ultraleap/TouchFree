using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
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

        private InteractionType lastInteraction;

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

            ConfigManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;

            OnInteractionSettingsUpdated(ConfigManager.InteractionConfig);
        }

        protected void OnInteractionSettingsUpdated(InteractionConfig _config)
        {
            switch(lastInteraction)
            {
                case InteractionType.TOUCHPLANE:
                    updateBehaviour.OnUpdate -= touchPlane.Update;
                    touchPlane.HandleInputAction -= connectionManager.SendInputActionToWebsocket;
                    touchPlane.Disable();
                    break;

                case InteractionType.PUSH:
                    updateBehaviour.OnUpdate -= airPush.Update;
                    airPush.HandleInputAction -= connectionManager.SendInputActionToWebsocket;
                    airPush.Disable();
                    break;

                case InteractionType.HOVER:
                    updateBehaviour.OnUpdate -= hoverAndHold.Update;
                    hoverAndHold.HandleInputAction -= connectionManager.SendInputActionToWebsocket;
                    hoverAndHold.Disable();
                    break;

                case InteractionType.GRAB:
                    updateBehaviour.OnUpdate -= grab.Update;
                    grab.HandleInputAction -= connectionManager.SendInputActionToWebsocket;
                    grab.Disable();
                    break;
            }

            switch(_config.InteractionType)
            {
                case InteractionType.TOUCHPLANE:
                    updateBehaviour.OnUpdate += touchPlane.Update;
                    touchPlane.HandleInputAction += connectionManager.SendInputActionToWebsocket;
                    touchPlane.Enable();
                    break;

                case InteractionType.PUSH:
                    updateBehaviour.OnUpdate += airPush.Update;
                    airPush.HandleInputAction += connectionManager.SendInputActionToWebsocket;
                    airPush.Enable();
                    break;

                case InteractionType.HOVER:
                    updateBehaviour.OnUpdate += hoverAndHold.Update;
                    hoverAndHold.HandleInputAction += connectionManager.SendInputActionToWebsocket;
                    hoverAndHold.Enable();
                    break;

                case InteractionType.GRAB:
                    updateBehaviour.OnUpdate += grab.Update;
                    grab.HandleInputAction += connectionManager.SendInputActionToWebsocket;
                    grab.Enable();
                    break;
            }

            lastInteraction = _config.InteractionType;
        }
    }
}