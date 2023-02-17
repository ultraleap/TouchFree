using System.Collections.Generic;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;
using Ultraleap.TouchFree.Library.Interactions.InteractionModules;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public abstract class InteractionModule : IInteraction
    {
        public virtual InteractionType InteractionType { get; } = InteractionType.PUSH;

        private HandChirality handChirality;
        public HandType handType;

        public bool ignoreDragging;
        public bool ignoreSwiping;

        protected Positions positions;

        protected float distanceFromScreenMm;

        protected long latestTimestamp;

        protected bool hadHandLastFrame = false;

        private InteractionZoneState lastInteractionZoneState = InteractionZoneState.HAND_EXITED;

        protected IPositioningModule positioningModule;
        protected IPositionStabiliser positionStabiliser;

        protected readonly IHandManager handManager;
        protected readonly IVirtualScreen virtualScreen;
        private readonly IConfigManager configManager;
        private readonly IClientConnectionManager connectionManager;

        protected IEnumerable<PositionTrackerConfiguration> positionConfiguration;

        public InteractionModule(
            IHandManager _handManager, 
            IVirtualScreen _virtualScreen,
            IConfigManager _configManager,
            IClientConnectionManager _connectionManager,
            IPositioningModule _positioningModule,
            IPositionStabiliser _positionStabiliser
        )
        {
            handManager = _handManager;
            virtualScreen = _virtualScreen;
            configManager = _configManager;
            connectionManager = _connectionManager;
            positioningModule = _positioningModule;
            positionStabiliser = _positionStabiliser;

            configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;
            OnInteractionSettingsUpdated(configManager.InteractionConfig);
        }

        public InputActionResult Update(float confidence)
        {
            // Obtain the relevant Hand Data from the HandManager, and call the main UpdateData function
            latestTimestamp = handManager.Timestamp;

            var hand = GetHand();
            var inputAction = UpdateData(hand, confidence);

            if (hand != null)
            {
                hadHandLastFrame = true;
            }
            else
            {
                hadHandLastFrame = false;
            }

            return inputAction;
        }

        // This is the main update loop of the interaction module
        protected abstract InputActionResult UpdateData(Leap.Hand hand, float confidence);

        protected virtual void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            ignoreDragging = !_config.UseScrollingOrDragging;
            ignoreSwiping = !_config.UseSwipeInteraction;
            positionStabiliser.ResetValues();
        }

        protected InputActionResult CreateInputActionResult(InputType _inputType, Positions _positions, float _progressToClick)
        {
            return new InputActionResult(new InputAction(latestTimestamp, InteractionType, handType, handChirality, _inputType, _positions, _progressToClick), _progressToClick);
        }

        Leap.Hand GetHand()
        {
            Leap.Hand hand = null;

            switch (handType)
            {
                case HandType.PRIMARY:
                    hand = handManager.PrimaryHand;
                    break;
                case HandType.SECONDARY:
                    hand = handManager.SecondaryHand;
                    break;
            }

            if (hand != null)
            {
                handChirality = hand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;

                positions = positioningModule.CalculatePositions(hand, positionConfiguration);
                positions = ApplyAdditionalPositionModifiers(positions);
                positions = positioningModule.ApplyStabiliation(positions, positionStabiliser);
                distanceFromScreenMm = positions.DistanceFromScreen * 1000f;
                hand = CheckHandInInteractionZone(hand);
            }

            return hand;
        }

        protected virtual Positions ApplyAdditionalPositionModifiers(Positions positions)
        {
            return positions;
        }

        /// <summary>
        /// Check if the hand is within the interaction zone. Return relevant results.
        /// This should be performed after 'positions' has been calculated.
        /// </summary>
        /// <param name="_hand"></param>
        /// <returns>Returns null if the hand is outside of the interaction zone</returns>
        Leap.Hand CheckHandInInteractionZone(Leap.Hand _hand)
        {
            if (_hand != null && configManager.InteractionConfig.InteractionZoneEnabled)
            {
                if (distanceFromScreenMm < configManager.InteractionConfig.InteractionMinDistanceMm ||
                    distanceFromScreenMm > configManager.InteractionConfig.InteractionMaxDistanceMm)
                {   

                    if (lastInteractionZoneState != InteractionZoneState.HAND_EXITED) 
                    {
                        connectionManager.HandleInteractionZoneEvent(InteractionZoneState.HAND_EXITED);
                        lastInteractionZoneState = InteractionZoneState.HAND_EXITED;
                    }
                    return null;
                }

                if (lastInteractionZoneState != InteractionZoneState.HAND_ENTERED) 
                {
                    connectionManager.HandleInteractionZoneEvent(InteractionZoneState.HAND_ENTERED);
                    lastInteractionZoneState = InteractionZoneState.HAND_ENTERED;
                }

            }

            return _hand;
        }

        /// <summary>
        /// Returns whether the hand is within the interaction zone.
        /// This should be performed after 'positions' has been calculated.
        /// </summary>
        /// <returns>Returns whether the hand is within the interaction zone.</returns>
        public bool IsHandInInteractionZone()
        {
            return !configManager.InteractionConfig.InteractionZoneEnabled ||
                (distanceFromScreenMm >= configManager.InteractionConfig.InteractionMinDistanceMm &&
                 distanceFromScreenMm <= configManager.InteractionConfig.InteractionMaxDistanceMm);
        }
    }
}