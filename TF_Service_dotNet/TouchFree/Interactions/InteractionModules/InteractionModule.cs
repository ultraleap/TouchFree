using System;
using System.Collections.Generic;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions.InteractionModules;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public abstract class InteractionModule
    {
        public virtual InteractionType InteractionType { get; } = InteractionType.PUSH;

        private HandChirality handChirality;
        public HandType handType;

        public bool ignoreDragging;

        protected Positions positions;

        protected float distanceFromScreenMm;

        protected long latestTimestamp;

        protected bool hadHandLastFrame = false;

        protected IPositioningModule positioningModule;
        protected IPositionStabiliser positionStabiliser;

        protected readonly HandManager handManager;
        protected readonly IVirtualScreen virtualScreen;
        private readonly IConfigManager configManager;

        protected IEnumerable<PositionTrackerConfiguration> positionConfiguration;

        public InteractionModule(HandManager _handManager, IVirtualScreen _virtualScreen, IConfigManager _configManager, IPositioningModule _positioningModule, IPositionStabiliser _positionStabiliser)
        {
            handManager = _handManager;
            virtualScreen = _virtualScreen;
            configManager = _configManager;
            positioningModule = _positioningModule;
            positionStabiliser = _positionStabiliser;
        }

        public virtual void Enable()
        {
            configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;
            OnInteractionSettingsUpdated(configManager.InteractionConfig);
        }

        public InputAction? Update()
        {
            // Obtain the relevant Hand Data from the HandManager, and call the main UpdateData function
            latestTimestamp = handManager.Timestamp;

            var hand = GetHand();
            var inputAction = UpdateData(hand);

            if (hand != null)
            {
                hadHandLastFrame = true;
            }
            else
            {
                hadHandLastFrame = false;
            }

            if (inputAction != null && inputAction.actionDetected)
            {
                return new InputAction(latestTimestamp, InteractionType, handType, handChirality, inputAction.inputType, inputAction.positions, inputAction.progressToClick);
            }
            else
            {
                return null;
            }

        }

        // This is the main update loop of the interaction module
        protected abstract InputActionResult UpdateData(Leap.Hand hand);

        protected virtual void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            ignoreDragging = !_config.UseScrollingOrDragging;
            positionStabiliser.ResetValues();
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
                distanceFromScreenMm = positions.DistanceFromScreen * 1000f;
                hand = CheckHandInInteractionZone(hand);
            }

            return hand;
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
                    return null;
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