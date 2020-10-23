using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.ScreenControl.Client.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Client
{
#if SCREENCONTROL_CORE_AVAIL
    public class DirectCoreConnection : CoreConnection
    {
        // Why are we using an initialiser here instead of just a constructor?
        public override void Initialise()
        {
                //TODO: this listener needs to be removed on deletion!
                Core.InteractionManager.HandleInputAction += ConvertToClientInputAction;
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