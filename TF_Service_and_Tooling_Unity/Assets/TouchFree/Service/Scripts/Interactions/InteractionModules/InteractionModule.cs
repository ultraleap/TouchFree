using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service
{
    public abstract class InteractionModule : MonoBehaviour
    {
        public virtual InteractionType InteractionType { get; } = InteractionType.PUSH;

        protected HandChirality handChirality;
        public HandType handType;

        public bool ignoreDragging;
        public PositioningModule positioningModule;

        public delegate void InteractionInputAction(HandChirality _chirality, HandType _handType, InputAction _inputData);
        public static event InteractionInputAction HandleInputAction;

        protected Positions positions;

        public bool isTouching = false;

        public abstract float CalculateProgress(Leap.Hand _hand);
        public abstract void RunInteraction(Leap.Hand _hand, float _progress);

        protected void SendInputAction(InputType _inputType, Positions _positions, float _progressToClick)
        {
            InputAction actionData = new InputAction(HandManager.Instance.Timestamp, InteractionType, handType, handChirality, _inputType, _positions, _progressToClick);
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

        /// <summary>
        /// Check if the hand is within the interaction zone. Return relevant results.
        /// This should be performed after 'positions' has been calculated.
        /// </summary>
        /// <param name="_hand"></param>
        /// <returns>Returns null if the hand is outside of the interaction zone</returns>
        protected Leap.Hand CheckHandInInteractionZone(Leap.Hand _hand)
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