using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service
{
    [DefaultExecutionOrder(-100)]
    public class InteractionManager : MonoBehaviour
    {
        public delegate void InteractionInputAction(InputAction _inputData);
        public static event InteractionInputAction HandleInputAction;

        public static Dictionary<InteractionType, InteractionModule> interactions =
                  new Dictionary<InteractionType, InteractionModule>();

        public InteractionModule[] activeInteractions;

        private static InteractionManager instance = null;
        public static InteractionManager Instance
        {
            get
            {
                return instance;
            }
        }

        public InteractionModule pushInteractionModule;
        public InteractionModule hoverInteractionModule;
        public InteractionModule grabInteractionModule;
        public InteractionModule touchPlaneInteractionModule;

        public HandType handType;
        public bool hadHandLastFrame = false;

        private void Awake()
        {
            // if the singleton hasn't been initialized yet
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            instance = this;

            InteractionModule.HandleInputAction += HandleInteractionModuleInputAction;

            interactions.Add(InteractionType.PUSH, pushInteractionModule);
            interactions.Add(InteractionType.HOVER, hoverInteractionModule);
            interactions.Add(InteractionType.GRAB, grabInteractionModule);
            interactions.Add(InteractionType.TOUCHPLANE, touchPlaneInteractionModule);

            InteractionConfig.OnConfigUpdated += InteractionConfigUpdated;

            //SetActiveInteractions(ConfigManager.InteractionConfig.InteractionType);
        }

        private void OnDestroy()
        {
            InteractionModule.HandleInputAction -= HandleInteractionModuleInputAction;
            InteractionConfig.OnConfigUpdated -= InteractionConfigUpdated;
        }

        int lastDominantHybridInteraciton = -1;

        private void LateUpdate()
        {
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

            int dominantInteractionIndex = 0;
            float dominantInteractionProgress = 0;

            if (lastDominantHybridInteraciton != -1 && !activeInteractions[lastDominantHybridInteraciton].isTouching)
            {
                lastDominantHybridInteraciton = -1;
            }

            for (int index = 0; index < activeInteractions.Length; index++)
            {
                if(activeInteractions[index].isTouching && (lastDominantHybridInteraciton == index || lastDominantHybridInteraciton == -1))
                {
                    dominantInteractionIndex = index;
                    dominantInteractionProgress = activeInteractions[index].CalculateProgress(hand);

                    if(lastDominantHybridInteraciton == -1)
                    {
                        lastDominantHybridInteraciton = index;
                    }

                    continue;
                }

                float progress = activeInteractions[index].CalculateProgress(hand);

                if(progress > dominantInteractionProgress)
                {
                    dominantInteractionIndex = index;
                    dominantInteractionProgress = progress;
                }
            }

            activeInteractions[dominantInteractionIndex].RunInteraction(hand, dominantInteractionProgress);

            for (int index = 0; index < activeInteractions.Length; index++)
            {
                activeInteractions[index].RunPostProgressNonInteraction();
            }

            hadHandLastFrame = hand != null;
        }

        public void SetActiveInteractions(InteractionType _activateType)
        {
            SetActiveInteractions(new InteractionType[] { _activateType });
        }

        // For Config settings and Client Interaction requests
        public void SetActiveInteractions(InteractionType[] _activateTypes)
        {
            //foreach(var interaction in interactions)
            //{
            //    bool set = false;
            //    foreach(var toActivate in _activateTypes)
            //    {
            //        if(interaction.Key == toActivate)
            //        {
            //            set = true;

            //            if(!interaction.Value.enabled)
            //            {
            //                interaction.Value.enabled = true;
            //            }
            //            break;
            //        }
            //    }

            //    if(!set)
            //    {
            //        if (interaction.Value.enabled)
            //        {
            //            interaction.Value.enabled = false;
            //        }
            //    }
            //}
        }

        private void HandleInteractionModuleInputAction(HandChirality _chirality, HandType _handType, InputAction _inputData)
        {
            HandleInputAction?.Invoke(_inputData);
        }

        private void InteractionConfigUpdated()
        {
            SetActiveInteractions(ConfigManager.InteractionConfig.InteractionType);
        }
    }
}