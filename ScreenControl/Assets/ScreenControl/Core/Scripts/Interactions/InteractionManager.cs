using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ultraleap.ScreenControl.Core
{
    [DefaultExecutionOrder(-100)]
    public class InteractionManager : MonoBehaviour
    {
        public delegate void InputAction(ScreenControlTypes.CoreInputAction _inputData);
        public static event InputAction HandleInputAction;

        public static Dictionary<ScreenControlTypes.InteractionType, InteractionModule> interactions =
                  new Dictionary<ScreenControlTypes.InteractionType, InteractionModule>();

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

        private void Awake()
        {
            // if the singleton hasn't been initialized yet
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            instance = this;

            InteractionModule.HandleInputAction += HandleInteractionModuleInputAction;

            interactions.Add(ScreenControlTypes.InteractionType.PUSH, pushInteractionModule);
            interactions.Add(ScreenControlTypes.InteractionType.HOVER, hoverInteractionModule);
            interactions.Add(ScreenControlTypes.InteractionType.GRAB, grabInteractionModule);
        }

        private void OnDestroy()
        {
            InteractionModule.HandleInputAction -= HandleInteractionModuleInputAction;
        }

        // For Config settings and Client Interaction requests
        public void SetActiveInteractions(ScreenControlTypes.InteractionType[] _activateTypes)
        {
            foreach(var interaction in interactions)
            {
                bool set = false;
                foreach(var toActivate in _activateTypes)
                {
                    if(interaction.Key == toActivate)
                    {
                        set = true;
                        interaction.Value.enabled = true;
                        break;
                    }
                }

                if(!set)
                {
                    interaction.Value.enabled = false;
                }
            }
        }

        private void HandleInteractionModuleInputAction(ScreenControlTypes.HandChirality _chirality, ScreenControlTypes.HandType _handType, ScreenControlTypes.CoreInputAction _inputData)
        {
            HandleInputAction?.Invoke(_inputData);
        }
    }
}