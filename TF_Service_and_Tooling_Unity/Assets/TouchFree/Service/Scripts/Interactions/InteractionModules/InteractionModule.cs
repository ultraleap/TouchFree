﻿using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service
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
        Leap.Hand CheckHandInInteractionZone(Leap.Hand _hand)
        {
            if (_hand != null && ConfigManager.InteractionConfig.interactionZoneEnabled)
            {
                if (positions.DistanceFromScreen < ConfigManager.InteractionConfig.interactionMinDistanceCm / 100 ||
                    positions.DistanceFromScreen > ConfigManager.InteractionConfig.interactionMaxDistanceCm / 100)
                {
                    return null;
                }
            }

            return _hand;
        }
    }
}