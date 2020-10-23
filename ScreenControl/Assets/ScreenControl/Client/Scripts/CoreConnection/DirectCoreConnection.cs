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

        //TODO: we will need access to Cores 'InputActionData' if we have core embedded... maybe a custom #define ?
        void ConvertToClientInputAction(Core.ScreenControlTypes.InputActionData _data)
        {
            // Convert Core event into Client event
            ClientInputAction clientInput = new ClientInputAction(_data.Timestamp, (InteractionType)_data.Source, (HandChirality)_data.Chirality, (InputType)_data.Type, _data.CursorPosition, _data.ProgressToClick);
            RelayInputAction(clientInput);
        }
    }
#endif
}