using Ultraleap.TouchFree.Library;

namespace Ultraleap.TouchFree.Tooling
{
    // Class: InputActionPlugin
    // A base class for Plugins which are used by the <InputActionManager> to manipulate the
    // <InputAction> data received from the Service.
    public abstract class InputActionPlugin
    {
        // Variable: InputActionOutput
        // An event for transmitting <InputActions> as they pass through this plugin.
        // This can be used to access the data as it is used by a specific plugin, as to intercept
        // the full cycle of plugins that the <InputActionManager> references.
        public event InputActionManager.InputActionEvent InputActionOutput;

        // Function: RunPlugin
        // Called from <InputActionManager> and provided a <InputAction> as a parameter.
        // This function is a wrapper that guarantees that the results of <ModifyInputAction> are both
        // returned to the <InputActionManager> and transmitted via <TransmitInputAction>.
        public ClientInputAction? RunPlugin(ClientInputAction _inputAction)
        {
            ClientInputAction? modifiedInputAction = ModifyInputAction(_inputAction);

            if (modifiedInputAction.HasValue)
            {
                TransmitInputAction(modifiedInputAction.Value);
            }

            return modifiedInputAction;
        }

        // Function: ModifyInputAction
        // Called from <RunPlugin> and provided a <InputAction> as a parameter.
        // This function is used to manipulate the incoming <InputAction>
        // data. Returns a <InputAction> which is then distributed via the <InputActionManager>.
        protected abstract ClientInputAction? ModifyInputAction(ClientInputAction _inputAction);

        // Function: TransmitInputAction
        // To be used to Invoke the <InputActionOutput> event from any child class of this base.
        internal void TransmitInputAction(ClientInputAction _inputAction)
        {
            InputActionOutput?.Invoke(_inputAction);
        }
    }
}