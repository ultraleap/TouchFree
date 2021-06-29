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
            //interactions.Add(InteractionType.GRAB, grabInteractionModule);
            interactions.Add(InteractionType.TOUCHPLANE, touchPlaneInteractionModule);

            InteractionConfig.OnConfigUpdated += InteractionConfigUpdated;

            SetActiveInteractions(ConfigManager.InteractionConfig.InteractionType);
        }

        private void OnDestroy()
        {
            InteractionModule.HandleInputAction -= HandleInteractionModuleInputAction;
            InteractionConfig.OnConfigUpdated -= InteractionConfigUpdated;
        }

        public void SetActiveInteractions(InteractionType _activateType)
        {
            SetActiveInteractions(new InteractionType[] { _activateType });
        }

        // For Config settings and Client Interaction requests
        public void SetActiveInteractions(InteractionType[] _activateTypes)
        {
            foreach(var interaction in interactions)
            {
                bool set = false;
                foreach(var toActivate in _activateTypes)
                {
                    if(interaction.Key == toActivate)
                    {
                        set = true;

                        if(!interaction.Value.enabled)
                        {
                            interaction.Value.enabled = true;
                        }
                        break;
                    }
                }

                if(!set)
                {
                    if (interaction.Value.enabled)
                    {
                        interaction.Value.enabled = false;
                    }
                }
            }
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