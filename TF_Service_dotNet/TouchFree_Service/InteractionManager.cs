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

        private InteractionType lastInteraction;

        public InteractionManager(
            UpdateBehaviour _updateBehaviour,
            ClientConnectionManager _connectionManager,
            AirPushInteraction _airPush,
            GrabInteraction _grab,
            HoverAndHoldInteraction _hoverAndHold,
            TouchPlanePushInteraction _touchPlane,
            IConfigManager _configManager)
        {
            airPush = _airPush;
            grab = _grab;
            hoverAndHold = _hoverAndHold;
            touchPlane = _touchPlane;

            airPush.HandleInputAction += _connectionManager.SendInputActionToWebsocket;
            grab.HandleInputAction += _connectionManager.SendInputActionToWebsocket;
            hoverAndHold.HandleInputAction += _connectionManager.SendInputActionToWebsocket;
            touchPlane.HandleInputAction += _connectionManager.SendInputActionToWebsocket;

            _configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;

            OnInteractionSettingsUpdated(_configManager.InteractionConfig);

            _updateBehaviour.OnUpdate += UpdateInteraction;
        }

        protected void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            var currentLastInteraction = lastInteraction;
            var currentInteraction = _config.InteractionType;

            if (currentLastInteraction != currentInteraction)
            {
                GetInteractionModule(currentLastInteraction)?.Disable();
                GetInteractionModule(currentInteraction)?.Enable();
            }

            lastInteraction = currentInteraction;
        }

        private void UpdateInteraction()
        {
            GetInteractionModule(lastInteraction)?.Update();
        }

        private InteractionModule GetInteractionModule(InteractionType type)
        {
            switch (type)
            {
                case InteractionType.TOUCHPLANE:
                    return touchPlane;

                case InteractionType.PUSH:
                    return airPush;

                case InteractionType.HOVER:
                    return hoverAndHold;

                case InteractionType.GRAB:
                    return grab;
            }

            return null;
        }
    }
}