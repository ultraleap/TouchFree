using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    // Class: InputActionPlugin
    // A base class for Plugins which are used by the <InputActionManager> to manipulate the
    // <ClientInputAction> data received from the Service.
    public abstract class InputActionPlugin : MonoBehaviour
    {
        // Variable: InputActionOutput
        // An event for transmitting <ClientInputActions> as they pass through this plugin.
        // This can be used to access the data as it is used by a specific plugin, as to intercept
        // the full cycle of plugins that the <InputActionManager> references.
        public event InputActionManager.ClientInputActionEvent InputActionOutput;

        // Function: RunPlugin
        // Called from <InputActionManager> and provided a <ClientInputAction> as a parameter.
        // This function is a wrapper that guarantees that the results of <ModifyInputAction> are both
        // returned to the <InputActionManager> and transmitted via <TransmitInputAction>.
        public ClientInputAction RunPlugin(ClientInputAction _inputAction)
        {
            _inputAction = ModifyInputAction(_inputAction);
            TransmitInputAction(_inputAction);
            return _inputAction;
        }

        // Function: ModifyInputAction
        // Called from <RunPlugin> and provided a <ClientInputAction> as a parameter.
        // This function is used to manipulate the incoming <ClientInputAction>
        // data. Returns a <ClientInputAction> which is then distributed via the <InputActionManager>.
        protected abstract ClientInputAction ModifyInputAction(ClientInputAction _inputAction);

        // Function: TransmitInputAction
        // To be used to Invoke the <InputActionOutput> event from any child class of this base.
        internal void TransmitInputAction(ClientInputAction _inputAction)
        {
            InputActionOutput?.Invoke(_inputAction);
        }
    }
}