using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public abstract class InteractionModule : MonoBehaviour
    {
        public virtual ScreenControlTypes.InteractionType InteractionType { get; } = ScreenControlTypes.InteractionType.PUSH;

        private ScreenControlTypes.HandChirality handChirality;
        public ScreenControlTypes.HandType handType;

        public bool ignoreDragging;
        public PositioningModule positioningModule;

        public delegate void InputAction(ScreenControlTypes.HandChirality _chirality, ScreenControlTypes.HandType _handType, ScreenControlTypes.CoreInputAction _inputData);
        public static event InputAction HandleInputAction;

        protected Positions positions;

        protected long latestTimestamp;

        void Update()
        {
            // Obtain the relevant Hand Data from the HandManager, and call the main UpdateData function
            latestTimestamp = HandManager.Instance.Timestamp;

            Leap.Hand hand = null;

            switch (handType)
            {
                case ScreenControlTypes.HandType.PRIMARY:

                    hand = HandManager.Instance.PrimaryHand;
                    break;
                case ScreenControlTypes.HandType.SECONDARY:
                    hand = HandManager.Instance.SecondaryHand;
                    break;
            }

            if (hand != null)
            {
                handChirality = hand.IsLeft ? ScreenControlTypes.HandChirality.LEFT : ScreenControlTypes.HandChirality.RIGHT;
            }

            UpdateData(hand);
        }

        // This is the main update loop of the interaction module
        protected virtual void UpdateData(Leap.Hand hand) { }

        protected void SendInputAction(ScreenControlTypes.InputType _inputType, Positions _positions, float _progressToClick)
        {
            ScreenControlTypes.CoreInputAction actionData = new ScreenControlTypes.CoreInputAction(latestTimestamp, InteractionType, handType, handChirality, _inputType, _positions, _progressToClick);
            HandleInputAction?.Invoke(handChirality, handType, actionData);
        }

        protected virtual void OnEnable()
        {
            InteractionConfig.OnConfigUpdated += OnSettingsUpdated;
            OnSettingsUpdated();
            ConfigManager.GlobalSettings.CreateVirtualScreen();
            positioningModule.Stabiliser.ResetValues();
            InteractionManager.Instance.RegisterInteraction(InteractionType, this);
        }

        protected virtual void OnDisable()
        {
            InteractionConfig.OnConfigUpdated -= OnSettingsUpdated;
            InteractionManager.Instance.RemoveInteraction(InteractionType);
        }

        protected virtual void OnSettingsUpdated()
        {
            ignoreDragging = !ConfigManager.InteractionConfig.Generic.UseScrollingOrDragging;
        }
    }
}