using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.ScreenControl.Client.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Client
{
#if SCREENCONTROL_CORE
    public class DirectCoreConnection : CoreConnection
    {
        internal DirectCoreConnection()
        {
            Core.InteractionManager.HandleInputAction += ConvertToClientInputAction;
        }

        ~DirectCoreConnection()
        {
            Debug.Log("Destructor invoked");
            Core.InteractionManager.HandleInputAction -= ConvertToClientInputAction;
        }

        void ConvertToClientInputAction(Core.ScreenControlTypes.InputActionData _data)
        {
            // Convert Core event into Client event
            ClientInputAction clientInput = new ClientInputAction(
                _data.Timestamp,
                (InteractionType)_data.SourceInteraction,
                (HandType)_data.HandType,
                (HandChirality)_data.Chirality,
                (InputType)_data.InputType,
                _data.CursorPosition,
                _data.ProgressToClick);

            RelayInputAction(clientInput);
        }
    }
#endif
}