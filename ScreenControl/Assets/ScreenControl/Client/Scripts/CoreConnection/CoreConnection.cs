using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    public abstract class CoreConnection
    {
        public delegate void ClientInputActionEvent(ScreenControlTypes.ClientInputAction _inputData);
        public static event ClientInputActionEvent TransmitInputAction;

        public PhysicalConfiguration physicalConfig
        {
            get
            {
                //TODO: This should either be known (so not needed to be created EVERY get) and sent to Core directly OR explicitly requested from core once and cached
                return new PhysicalConfiguration();
            }
        }

        public virtual void Initialise()
        {
        }

        protected void RelayInputAction(ScreenControlTypes.ClientInputAction _inputData)
        {
            TransmitInputAction?.Invoke(_inputData);
        }
    }
}