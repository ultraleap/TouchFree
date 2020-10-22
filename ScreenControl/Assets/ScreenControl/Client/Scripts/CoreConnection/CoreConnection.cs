using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    public abstract class CoreConnection
    {
        public delegate void ClientInputActionEvent(ScreenControlTypes.ClientInputAction _inputData);
        public event ClientInputActionEvent TransmitInputAction;

        public virtual void Initialise()
        {
        }

        protected void RelayInputAction(ScreenControlTypes.ClientInputAction _inputData)
        {
            TransmitInputAction?.Invoke(_inputData);
        }
    }
}