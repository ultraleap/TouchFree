using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ultraleap.ScreenControl.Core
{
    [DefaultExecutionOrder(-100)]
    public class InteractionManager : MonoBehaviour
    {
        public delegate void InputAction(ScreenControlTypes.InputActionData _inputData);
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

        private void Awake()
        {
            // if the singleton hasn't been initialized yet
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            instance = this;

            InteractionModule.HandleInputAction += HandleInteractionModuleInputAction;
        }

        private void OnDestroy()
        {
            InteractionModule.HandleInputAction -= HandleInteractionModuleInputAction;
        }

        public void RegisterInteraction(ScreenControlTypes.InteractionType type, InteractionModule interactionObject)
        {
            if (interactions.ContainsKey(type))
            {
                Debug.LogError($@"InteractionManager recieved a request to register an interaction
with Keyed with ${type} but there was already such an interaction registered!");
            }
            else
            {
                interactions.Add(type, interactionObject);
            }
        }

        public void RemoveInteraction(ScreenControlTypes.InteractionType type)
        {
            if (interactions.ContainsKey(type))
            {
                interactions.Remove(type);
            }
            else
            {
                Debug.LogError($"Attempted to remove ");
            }
        }

        // Todo with Config settings
        public void SetActiveInteractions(ScreenControlTypes.InteractionType[] activeTypes)
        {
        }

        private void HandleInteractionModuleInputAction(ScreenControlTypes.HandChirality _chirality, ScreenControlTypes.HandType _handType, ScreenControlTypes.InputActionData _inputData)
        {
            HandleInputAction?.Invoke(_inputData);
        }
    }
}