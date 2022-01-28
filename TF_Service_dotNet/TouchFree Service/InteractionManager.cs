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

        public InteractionManager(
            UpdateBehaviour _updateBehaviour,
            HandManager _handManager,
            ClientConnectionManager _connectionManager,
            IVirtualScreenManager _virtualScreenManager,
            IConfigManager _configManager)
        {
            handManager = _handManager;
            updateBehaviour = _updateBehaviour;
            connectionManager = _connectionManager;

            airPush = new AirPushInteraction(handManager, _virtualScreenManager, _configManager);
            grab = new GrabInteraction(handManager, _virtualScreenManager, _configManager);
            hoverAndHold = new HoverAndHoldInteraction(handManager, _virtualScreenManager, _configManager);
            touchPlane = new TouchPlanePushInteraction(handManager, _virtualScreenManager, _configManager);

            _configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;

            OnInteractionSettingsUpdated(_configManager.InteractionConfig);
        }

        protected void OnInteractionSettingsUpdated(InteractionConfig _config)
        {
            switch(lastInteraction)
            {
                case InteractionType.TOUCHPLANE:
                    DisableInteraction(touchPlane);
                    break;

                case InteractionType.PUSH:
                    DisableInteraction(airPush);
                    break;

                case InteractionType.HOVER:
                    DisableInteraction(hoverAndHold);
                    break;

                case InteractionType.GRAB:
                    DisableInteraction(grab);
                    break;
            }

            switch(_config.InteractionType)
            {
                case InteractionType.TOUCHPLANE:
                    EnableInteraction(touchPlane);
                    break;

                case InteractionType.PUSH:
                    EnableInteraction(airPush);
                    break;

                case InteractionType.HOVER:
                    EnableInteraction(hoverAndHold);
                    break;

                case InteractionType.GRAB:
                    EnableInteraction(grab);
                    break;
            }

            lastInteraction = _config.InteractionType;
        }

        protected void EnableInteraction(InteractionModule target)
        {
            updateBehaviour.OnUpdate += target.Update;
            target.HandleInputAction += connectionManager.SendInputActionToWebsocket;
            target.Enable();
        }

        protected void DisableInteraction(InteractionModule target)
        {
            updateBehaviour.OnUpdate -= target.Update;
            target.HandleInputAction -= connectionManager.SendInputActionToWebsocket;
            target.Disable();
        }
    }
}