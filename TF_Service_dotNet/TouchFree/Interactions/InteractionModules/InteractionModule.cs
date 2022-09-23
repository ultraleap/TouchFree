using System;

using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public abstract class InteractionModule
    {
        public virtual InteractionType InteractionType { get; } = InteractionType.PUSH;

        private HandChirality handChirality;
        public HandType handType;

        public bool ignoreDragging;

        public event Action<InputAction> HandleInputAction;

        protected Positions positions;

        protected float distanceFromScreenMm;

        protected long latestTimestamp;

        protected bool hadHandLastFrame = false;

        protected IPositionStabiliser positioningStabiliser { get => positioningModule.Stabiliser; }
        protected IPositioningModule positioningModule;

        protected readonly HandManager handManager;
        protected readonly IVirtualScreen virtualScreen;
        private readonly IConfigManager configManager;

        public InteractionModule(HandManager _handManager, IVirtualScreen _virtualScreen, IConfigManager _configManager, IPositioningModule _positioningModule, TrackedPosition trackedPosition)
        {
            handManager = _handManager;
            virtualScreen = _virtualScreen;
            configManager = _configManager;

            positioningModule = _positioningModule;
            positioningModule.TrackedPosition = trackedPosition;
        }

        public virtual void Enable()
        {
            configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;
            OnInteractionSettingsUpdated(configManager.InteractionConfig);
        }

        public virtual void Disable()
        {
            configManager.OnInteractionConfigUpdated -= OnInteractionSettingsUpdated;
        }

        public void Update()
        {
            // Obtain the relevant Hand Data from the HandManager, and call the main UpdateData function
            latestTimestamp = handManager.Timestamp;

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

                positions = positioningModule.CalculatePositions(hand);
                distanceFromScreenMm = positions.DistanceFromScreen * 1000f;
                hand = CheckHandInInteractionZone(hand);
            }

            UpdateData(hand);

            if (hand != null)
            {
                hadHandLastFrame = true;
            }
            else
            {
                hadHandLastFrame = false;
            }
        }

        // This is the main update loop of the interaction module
        protected virtual void UpdateData(Leap.Hand hand) { }

        protected void SendInputAction(InputType _inputType, Positions _positions, float _progressToClick)
        {
            InputAction actionData = new InputAction(latestTimestamp, InteractionType, handType, handChirality, _inputType, _positions, _progressToClick);
            HandleInputAction?.Invoke(actionData);
        }

        protected virtual void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            ignoreDragging = !_config.UseScrollingOrDragging;
            positioningStabiliser.ResetValues();
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