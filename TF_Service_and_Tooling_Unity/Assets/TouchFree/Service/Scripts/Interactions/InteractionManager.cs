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

        public static Dictionary<InteractionType, InteractionModulePair> interactions =
                  new Dictionary<InteractionType, InteractionModulePair>();

        private static InteractionManager instance = null;
        public static InteractionManager Instance
        {
            get
            {
                return instance;
            }
        }

        public InteractionModulePair pushInteractionModules;
        public InteractionModulePair hoverInteractionModules;
        public InteractionModulePair grabInteractionModules;
        public InteractionModulePair touchPlaneInteractionModules;

        public GameObject secondaryInteractionModulesParent;

        private void Awake()
        {
            // if the singleton hasn't been initialized yet
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            instance = this;

            InteractionModule.HandleInputAction += HandleInteractionModuleInputAction;

            interactions.Add(InteractionType.PUSH, pushInteractionModules);
            interactions.Add(InteractionType.HOVER, hoverInteractionModules);
            interactions.Add(InteractionType.GRAB, grabInteractionModules);
            interactions.Add(InteractionType.TOUCHPLANE, touchPlaneInteractionModules);

            InteractionConfig.OnConfigUpdated += InteractionConfigUpdated;

            SetActiveInteractions(ConfigManager.InteractionConfig.InteractionType);
            SetActiveInteractions(ConfigManager.InteractionConfig.InteractionTypeSecondary, false);
        }

        private void OnDestroy()
        {
            InteractionModule.HandleInputAction -= HandleInteractionModuleInputAction;
            InteractionConfig.OnConfigUpdated -= InteractionConfigUpdated;
        }

        public void SetActiveInteractions(InteractionType _activateType, bool _primary = true)
        {
            SetActiveInteractions(new InteractionType[] { _activateType }, _primary);
        }

        // For Config settings and Client Interaction requests
        public void SetActiveInteractions(InteractionType[] _activateTypes, bool _primary = true)
        {
            foreach(var interaction in interactions)
            {
                bool set = false;
                foreach(var toActivate in _activateTypes)
                {
                    if(interaction.Key == toActivate)
                    {
                        set = true;

                        if(_primary)
                        {
                            interaction.Value.primaryModule.enabled = true;
                        }
                        else
                        {
                            interaction.Value.secondaryModule.enabled = true;
                        }

                        break;
                    }
                }

                if(!set)
                {
                    if (_primary)
                    {
                        interaction.Value.primaryModule.enabled = false;
                    }
                    else
                    {
                        interaction.Value.secondaryModule.enabled = false;
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
            SetActiveInteractions(ConfigManager.InteractionConfig.InteractionTypeSecondary, false);
        }

        public void EnableSecondaryInteractionModules()
        {
            secondaryInteractionModulesParent.SetActive(true);
        }
    }

    [System.Serializable]
    public struct InteractionModulePair
    {
        public InteractionModule primaryModule;
        public InteractionModule secondaryModule;
    }
}