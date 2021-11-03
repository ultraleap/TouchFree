using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public abstract class InteractionModule
    {
        public virtual InteractionType InteractionType { get; } = InteractionType.PUSH;

        private HandChirality handChirality;
        public HandType handType;

        public bool ignoreDragging;

        public delegate void InteractionInputAction(InputAction _inputData);
        public event InteractionInputAction HandleInputAction;

        protected Positions positions;

        protected long latestTimestamp;

        protected bool hadHandLastFrame = false;

        protected PositionStabiliser positioningStabiliser;
        protected PositioningModule positioningModule;
        protected readonly HandManager handManager;

        public InteractionModule(HandManager _handManager)
        {
            handManager = _handManager;
            positioningStabiliser = new PositionStabiliser();

            positioningModule = new PositioningModule(positioningStabiliser, TrackedPosition.NEAREST);
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

        protected virtual void OnEnable()
        {
            ConfigManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;
            OnInteractionSettingsUpdated(ConfigManager.InteractionConfig);
        }

        protected virtual void OnDisable()
        {
            ConfigManager.OnInteractionConfigUpdated -= OnInteractionSettingsUpdated;

        }

        protected virtual void OnInteractionSettingsUpdated(InteractionConfig _config)
        {
            ignoreDragging = !_config.UseScrollingOrDragging;
            positioningModule.Stabiliser.ResetValues();
        }

        /// <summary>
        /// Check if the hand is within the interaction zone. Return relevant results.
        /// This should be performed after 'positions' has been calculated.
        /// </summary>
        /// <param name="_hand"></param>
        /// <returns>Returns null if the hand is outside of the interaction zone</returns>
        Leap.Hand CheckHandInInteractionZone(Leap.Hand _hand)
        {
            if (_hand != null && ConfigManager.InteractionConfig.InteractionZoneEnabled)
            {
                if (positions.DistanceFromScreen < ConfigManager.InteractionConfig.InteractionMinDistanceCm / 100 ||
                    positions.DistanceFromScreen > ConfigManager.InteractionConfig.InteractionMaxDistanceCm / 100)
                {
                    return null;
                }
            }

            return _hand;
        }
    }
}