using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connection;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public class InteractionManager
    {
        private readonly AirPushInteraction airPush;
        private readonly GrabInteraction grab;
        private readonly HoverAndHoldInteraction hoverAndHold;
        private readonly TouchPlanePushInteraction touchPlane;

        private InteractionType lastInteraction;

        private readonly UpdateBehaviour updateBehaviour;
        private readonly ClientConnectionManager connectionManager;

        public InteractionManager(
            UpdateBehaviour _updateBehaviour,
            ClientConnectionManager _connectionManager,
            AirPushInteraction _airPush,
            GrabInteraction _grab,
            HoverAndHoldInteraction _hoverAndHold,
            TouchPlanePushInteraction _touchPlane,
            IConfigManager _configManager)
        {
            updateBehaviour = _updateBehaviour;
            connectionManager = _connectionManager;

            airPush = _airPush;
            grab = _grab;
            hoverAndHold = _hoverAndHold;
            touchPlane = _touchPlane;

            _configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;

            OnInteractionSettingsUpdated(_configManager.InteractionConfig);
        }

        protected void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
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