using System.Linq;
using Ultraleap.TouchFree.Library;

namespace Ultraleap.TouchFree.Tooling
{
    // Class: InputActionManager
    // The manager for all <InputActions> to be handled and distributed. This runs the
    // received data through any referenced <InputActionPlugins> and finaly distributes the data
    // via the  <TransmitInputAction> event which should be listened to by any class hoping to make
    // use of incoming <InputActions>.
    public class InputActionManager
    {
        // Delegate: InputActionEvent
        // An Action to distribute a <InputAction> via the <TransmitInputAction> event listener.
        public delegate void InputActionEvent(ClientInputAction _inputData);

        // Variable: TransmitInputAction
        // An event for transmitting <InputActions> that have been modified via the active
        // <plugins>
        public static event InputActionEvent TransmitInputAction;

        // Variable: TransmitRawInputAction
        // An event for transmitting <InputActions> that have NOT been modified via any
        // <plugins>
        public static event InputActionEvent TransmitRawInputAction;

        public static InputActionManager Instance;

        // Variable: plugins
        // A pre-defined plugin array of <ToggleablePlugins> that modify incoming <InputActions>
        // based on custom rules.
        ToggleablePlugin[] plugins;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                return;
            }
            Instance = this;
        }

        internal void SendInputAction(ClientInputAction _inputAction)
        {
            TransmitRawInputAction?.Invoke(_inputAction);

            ClientInputAction? modifiedInputAction = RunPlugins(_inputAction);

            if (modifiedInputAction.HasValue)
            {
                TransmitInputAction?.Invoke(modifiedInputAction.Value);
            }
        }

        ClientInputAction? RunPlugins(ClientInputAction _inputAction)
        {
            ClientInputAction? modifiedInputAction = _inputAction;

            // Send the input action through the plugins in order
            // if it is returned null from a plugin, return it to be ignored
            if (plugins?.Any() == true)
            {
                foreach (var plugin in plugins)
                {
                    if (plugin.enabled)
                    {
                        if (modifiedInputAction.HasValue)
                        {
                            modifiedInputAction = plugin.plugin.RunPlugin(modifiedInputAction.Value);
                        }

                        if (!modifiedInputAction.HasValue)
                        {
                            break;
                        }
                    }
                }
            }

            return modifiedInputAction;
        }
    }
}