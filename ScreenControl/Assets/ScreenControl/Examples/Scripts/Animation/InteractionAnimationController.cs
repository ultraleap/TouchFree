using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Ultraleap.ScreenControl.Core;
using CoreTypes = Ultraleap.ScreenControl.Core.ScreenControlTypes;
using Ultraleap.ScreenControl.Client;
using ClientTypes = Ultraleap.ScreenControl.Client.ScreenControlTypes;

namespace Animation
{
    /// <Summary>
    /// This is an example of how to connect interactions to the Unity animation system
    /// Change animation state using triggers that align with interaction events
    /// Use animation blending, controlled by the Progress To Click
    /// </Summary>
    public class InteractionAnimationController : MonoBehaviour
    {
        public Animator animator;

        [Tooltip("Select which hand events to connect to the animation")]
        public CoreTypes.TrackedHand trackedHand;

        [Tooltip("Select whether to prefix the parameter names with the interaction type, " +
                 "making it possible to apply different animations for each interaction type")]
        public bool prefixSource = false;

        private List<string> parameterNames;
        private string currentState;
        private const string HIDDEN = "HIDDEN";
        private const string VISIBLE = "VISIBLE";
        private const string PROGRESS = "PROGRESS";
        private const string MOVING = "MOVING";


        void OnEnable()
        {
            parameterNames = new List<string>();
            foreach (var parameter in animator.parameters)
            {
                parameterNames.Add(parameter.name);
            }
            currentState = "Unknown";

            ConnectionManager.AddConnectionListener(OnCoreConnection);
        }

        void OnCoreConnection()
        {
            ConnectionManager.coreConnection.TransmitInputAction += OnHandleInputAction;
        }

        void OnDisable()
        {
            ConnectionManager.coreConnection.TransmitInputAction -= OnHandleInputAction;
        }

        // Update is called once per frame
        void Update()
        {
            CheckForActiveHand();
        }

        public void OnHandleInputAction(ClientTypes.ClientInputAction _inputData)
        {
            if (currentState == HIDDEN) return;

            string sourceName = prefixSource ? _inputData.Source.ToString() : "";

            if (_inputData.Type != ClientTypes.InputType.MOVE)
            {
                ChangeState(sourceName + _inputData.Type.ToString());
            }
            else
            {
                SetMoveState(sourceName + _inputData.Type.ToString());
                SetProgressValue(sourceName + PROGRESS, _inputData.ProgressToClick);
            }
        }

        /// <Summary>
        /// Sets a trigger to activate a new state. As a convention the parameters are named by their input source and type, eg. PushDOWN
        /// </Summary>
        /// <param name="newState">The name of the trigger parameter for the state we will transition to</param>
        public void ChangeState(string newState)
        {
            if (newState != currentState && parameterNames.Contains(newState))
            {
                animator.SetTrigger(newState);
                currentState = newState;
            }
        }

        /// <Summary>
        /// Sets a trigger to activate the move state when the hand first appears.
        /// </Summary>
        /// <param name="flagName">The name of the bool parameter that indicates if the hand has entered the move state</param>
        /// <param name="triggerName">The name of the trigger parameter for the state we will transition to</param>
        public void SetMoveState(string parameterName)
        {
            if (parameterNames.Contains(MOVING) && animator.GetBool(parameterName) == false)
            {
                animator.SetBool(MOVING, true);
                if (parameterNames.Contains(parameterName))
                {
                    ChangeState(parameterName);
                }
            }
        }

        /// <Summary>
        /// Set the progress to click value in the Animator, only if the parameter exists.
        /// </Summary>
        public void SetProgressValue(string parameterName, float value)
        {
            if (parameterNames.Contains(parameterName))
            {
                animator.SetFloat(parameterName, value);
            }
        }

        /// <Summary>
        /// Check to see if the selected tracked hand is currently visible and set the approriate state.
        /// </Summary>
        public void CheckForActiveHand()
        {
            var handPresent = false;

            switch (trackedHand)
            {
                case CoreTypes.TrackedHand.PRIMARY:
                    handPresent = HandManager.Instance.PrimaryHand == null ? false : true;
                    break;
                case CoreTypes.TrackedHand.LEFT:
                    handPresent = HandManager.Instance.LeftHand == null ? false : true;
                    break;
                case CoreTypes.TrackedHand.RIGHT:
                    handPresent = HandManager.Instance.RightHand == null ? false : true;
                    break;

            }

            if (currentState != HIDDEN && !handPresent)
            {
                DisableHand();
            }
            else if (currentState == HIDDEN && handPresent)
            {
                EnableHand();
            }
        }

        /// <Summary>
        /// Change to the VISIBLE state and ensure the HIDDEN trigger is cleared
        /// </Summary>
        private void EnableHand()
        {
            animator.ResetTrigger(HIDDEN);
            ChangeState(VISIBLE);
        }

        /// <Summary>
        /// Change to the HIDDEN state and reset the Animator
        /// </Summary>
        private void DisableHand()
        {
            ChangeState(HIDDEN);
            ResetAnimator();
        }

        /// <Summary>
        /// Clear all triggers, bool and float parameters in the animator
        /// </Summary>
        private void ResetAnimator()
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name != HIDDEN)
                {
                    animator.ResetTrigger(parameter.name);
                }
                else if (parameter.type == AnimatorControllerParameterType.Float)
                {
                    animator.SetFloat(parameter.name, 0);
                }
                else if (parameter.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(parameter.name, false);
                }
            }
        }
    }
}