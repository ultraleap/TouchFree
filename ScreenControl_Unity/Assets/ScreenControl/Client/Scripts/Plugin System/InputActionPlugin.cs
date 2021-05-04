using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    // Class: InputActionPlugin
    // A base class for Plugins which are used by the <InputActionManager> to manipulate the
    // <ClientInputAction> data received from the Service.
    public class InputActionPlugin : MonoBehaviour
    {
        // Variable: TransmitInputAction
        // An event for transmitting <ClientInputActions> as they pass through this plugin.
        // This can be used to access the data as it is used by a specific plugin, as to intercept
        // the full cycle of plugins that the <InputActionManager> references.
        public event InputActionManager.ClientInputActionEvent TransmitInputAction;

        // Function: RunPlugin
        // Called from the <InputActionManager> and provided a <ClientInputAction> as a parameter.
        // This function is designed to be overridden and used to manipulate the incoming <ClientInputAction>
        // data. Returns a <ClientInputAction> which is then distributed via the <InputActionManager>.
        //
        // Invoke the <TransmitInputAction> event by calling <TransmitInputActionEvent> if you
        // intend to make use of the specific data this plugin outputs.
        internal virtual ClientInputAction RunPlugin(ClientInputAction _inputAction)
        {
            TransmitInputActionEvent(_inputAction);
            return _inputAction;
        }

        // Function: TransmitInputActionEvent
        // To be used to Invoke the <TransmitInputAction> event from any child class of this base.
        internal void TransmitInputActionEvent(ClientInputAction _inputAction)
        {
            TransmitInputAction?.Invoke(_inputAction);
        }
    }
}