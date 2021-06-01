using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.ScreenControl.Core
{
    public abstract class InteractionModule : MonoBehaviour
    {
        public virtual InteractionType InteractionType { get; } = InteractionType.PUSH;

        private HandChirality handChirality;
        public HandType handType;

        public bool ignoreDragging;
        public PositioningModule positioningModule;

        public delegate void InteractionInputAction(HandChirality _chirality, HandType _handType, InputAction _inputData);
        public static event InteractionInputAction HandleInputAction;

        protected Positions positions;

        protected long latestTimestamp;

        protected bool hadHandLastFrame = false;

        void Update()
        {
            // Obtain the relevant Hand Data from the HandManager, and call the main UpdateData function
            latestTimestamp = HandManager.Instance.Timestamp;

            Leap.Hand hand = null;

            switch (handType)
            {
                case HandType.PRIMARY:

                    hand = HandManager.Instance.PrimaryHand;
                    break;
                case HandType.SECONDARY:
                    hand = HandManager.Instance.SecondaryHand;
                    break;
            }

            if (hand != null)
            {
                handChirality = hand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;
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
            HandleInputAction?.Invoke(handChirality, handType, actionData);
        }

        protected virtual void OnEnable()
        {
            InteractionConfig.OnConfigUpdated += OnSettingsUpdated;
            PhysicalConfig.OnConfigUpdated += OnSettingsUpdated;
            OnSettingsUpdated();
        }

        protected virtual void OnDisable()
        {
            InteractionConfig.OnConfigUpdated -= OnSettingsUpdated;
            PhysicalConfig.OnConfigUpdated -= OnSettingsUpdated;
        }

        protected virtual void OnSettingsUpdated()
        {
            ignoreDragging = !ConfigManager.InteractionConfig.UseScrollingOrDragging;
            ConfigManager.GlobalSettings.CreateVirtualScreen();
            positioningModule.Stabiliser.ResetValues();
        }
    }
}