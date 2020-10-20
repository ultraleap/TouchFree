using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.ScreenControl.Client.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Client
{
    public class DirectCoreConnection : CoreConnection
    {
        public override void Initialise()
        {
            //TODO: this listener needs to be removed on deletion!
            InteractionManager.HandleInputAction += ConvertToClientInputAction;
        }

        //TODO: we will need access to Cores 'InputActionData' if we have core embedded... maybe a custom #define ? 
        void ConvertToClientInputAction(InputActionData _data)
        {
            ScreenControlTypes.InteractionType source = ScreenControlTypes.InteractionType.Undefined;
            ScreenControlTypes.HandChirality chirality = ScreenControlTypes.HandChirality.UNKNOWN;
            ScreenControlTypes.InputType type = ScreenControlTypes.InputType.MOVE;

            switch (_data.Source)
            {
                case InteractionType.Undefined:
                    source = ScreenControlTypes.InteractionType.Undefined;
                    break;
                case InteractionType.Push:
                    source = ScreenControlTypes.InteractionType.Push;
                    break;
                case InteractionType.Grab:
                    source = ScreenControlTypes.InteractionType.Grab;
                    break;
                case InteractionType.Hover:
                    source = ScreenControlTypes.InteractionType.Hover;
                    break;
            }

            switch (_data.Chirality)
            {
                case HandChirality.UNKNOWN:
                    chirality = ScreenControlTypes.HandChirality.UNKNOWN;
                    break;
                case HandChirality.LEFT:
                    chirality = ScreenControlTypes.HandChirality.LEFT;
                    break;
                case HandChirality.RIGHT:
                    chirality = ScreenControlTypes.HandChirality.RIGHT;
                    break;
            }

            switch (_data.Type)
            {
                case InputType.HOVER:
                case InputType.MOVE:
                case InputType.DRAG:
                case InputType.HOLD:
                    type = ScreenControlTypes.InputType.MOVE;
                    break;
                case InputType.DOWN:
                    type = ScreenControlTypes.InputType.DOWN;
                    break;
                case InputType.UP:
                    type = ScreenControlTypes.InputType.UP;
                    break;
                case InputType.CANCEL:
                    type = ScreenControlTypes.InputType.CANCEL;
                    break;
            }

            ClientInputAction clientInput = new ClientInputAction(_data.Timestamp, source, chirality, type, _data.CursorPosition, _data.ProgressToClick);
            RelayInputAction(clientInput);
        }
    }
}