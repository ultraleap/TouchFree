using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    namespace ScreenControlTypes
    {
        public enum InteractionType
        {
            Undefined,
            Push,
            Grab,
            Hover,
        }
        public enum TrackedHand
        {
            PRIMARY,    // The current tracked hand
            LEFT,
            RIGHT
        }
    }

    public class InteractionModule : MonoBehaviour
    {
        public virtual ScreenControlTypes.InteractionType InteractionType { get; } = ScreenControlTypes.InteractionType.Undefined;

        public ScreenControlTypes.TrackedHand trackedHand;

        public bool ignoreDragging;
        public PositioningModule positioningModule;

        public delegate void InputAction(ScreenControlTypes.TrackedHand trackedHand, ScreenControlTypes.InputActionData _inputData);
        public static event InputAction HandleInputAction;

        protected Positions positions;

        private ScreenControlTypes.HandChirality handChirality = ScreenControlTypes.HandChirality.UNKNOWN;

        protected long latestTimestamp;

        void Update()
        {
            // Obtain the relevant Hand Data from the HandManager, and call the main UpdateData function

            latestTimestamp = HandManager.Instance.Timestamp;

            switch (trackedHand)
            {
                case ScreenControlTypes.TrackedHand.PRIMARY:
                    Leap.Hand hand = HandManager.Instance.PrimaryHand;
                    if (hand != null)
                    {
                        handChirality = hand.IsLeft ? ScreenControlTypes.HandChirality.LEFT : ScreenControlTypes.HandChirality.RIGHT;
                    }
                    // If the hand == null, keep the stored chirality.
                    UpdateData(hand);
                    break;
                case ScreenControlTypes.TrackedHand.LEFT:
                    handChirality = ScreenControlTypes.HandChirality.LEFT;
                    UpdateData(HandManager.Instance.LeftHand);
                    break;
                case ScreenControlTypes.TrackedHand.RIGHT:
                    handChirality = ScreenControlTypes.HandChirality.RIGHT;
                    UpdateData(HandManager.Instance.RightHand);
                    break;
            }
        }

        // This is the main update loop of the interaction module
        protected virtual void UpdateData(Leap.Hand hand) { }

        protected void SendInputAction(ScreenControlTypes.InputType _type, Positions _positions, float _progressToClick)
        {
            ScreenControlTypes.InputActionData actionData = new ScreenControlTypes.InputActionData(latestTimestamp, InteractionType, handChirality, _type, _positions, _progressToClick);
            HandleInputAction?.Invoke(trackedHand, actionData);
        }

        protected virtual void OnEnable()
        {
            SettingsConfig.OnConfigUpdated += OnSettingsUpdated;
            OnSettingsUpdated();
            PhysicalConfigurable.CreateVirtualScreen(PhysicalConfigurable.Config);
            positioningModule.Stabiliser.ResetValues();
        }

        protected virtual void OnDisable()
        {
            SettingsConfig.OnConfigUpdated -= OnSettingsUpdated;
        }

        protected virtual void OnSettingsUpdated()
        {
            ignoreDragging = !SettingsConfig.Config.UseScrollingOrDragging;
        }
    }
}