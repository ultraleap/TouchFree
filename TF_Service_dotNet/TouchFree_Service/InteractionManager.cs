using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;

using Ultraleap.TouchFree.Service.Connection;

namespace Ultraleap.TouchFree.Service
{
    public class InteractionManager
    {
        private readonly AirPushInteraction airPush;
        private readonly GrabInteraction grab;
        private readonly HoverAndHoldInteraction hoverAndHold;
        private readonly TouchPlanePushInteraction touchPlane;
        private readonly VelocitySwipeInteraction velocitySwipe;

        private readonly UpdateBehaviour updateBehaviour;
        private readonly ClientConnectionManager connectionManager;

        private InteractionModule currentInteraction;

        public InteractionManager(
            UpdateBehaviour _updateBehaviour,
            ClientConnectionManager _connectionManager,
            AirPushInteraction _airPush,
            GrabInteraction _grab,
            HoverAndHoldInteraction _hoverAndHold,
            TouchPlanePushInteraction _touchPlane,
            VelocitySwipeInteraction _velocitySwipe,
            IConfigManager _configManager)
        {
            updateBehaviour = _updateBehaviour;
            connectionManager = _connectionManager;

            airPush = _airPush;
            grab = _grab;
            hoverAndHold = _hoverAndHold;
            touchPlane = _touchPlane;
            velocitySwipe = _velocitySwipe;

            _configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;

            OnInteractionSettingsUpdated(_configManager.InteractionConfig);
        }

        protected void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            var initialisationNotStarted = currentInteraction == null;

            switch(_config.InteractionType)
            {
                case InteractionType.TOUCHPLANE:
                    currentInteraction = touchPlane;
                    break;

                case InteractionType.PUSH:
                    currentInteraction = airPush;
                    break;

                case InteractionType.HOVER:
                    currentInteraction = hoverAndHold;
                    break;

                case InteractionType.GRAB:
                    currentInteraction = grab;
                    break;
            }

            currentInteraction.Enable();

            if (initialisationNotStarted)
            {
                updateBehaviour.OnUpdate += Update;
            }
        }

        protected void Update()
        {
            if (currentInteraction != null)
            {
                var inputAction = currentInteraction.Update();
                if (inputAction.HasValue)
                {
                    connectionManager.SendInputActionToWebsocket(inputAction.Value);
                }
            }
        }
    }
}