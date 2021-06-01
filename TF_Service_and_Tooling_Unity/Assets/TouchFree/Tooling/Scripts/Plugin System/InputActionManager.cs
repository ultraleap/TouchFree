using UnityEngine;

namespace Ultraleap.TouchFree.Tooling
{
    // Class: InputActionManager
    // The manager for all <InputActions> to be handled and distributed. This runs the
    // received data through any referenced <InputActionPlugins> and finaly distributes the data
    // via the  <TransmitInputAction> event which should be listened to by any class hoping to make
    // use of incoming <InputActions>.
    [DefaultExecutionOrder(-1)]
    public class InputActionManager : MonoBehaviour
    {
        // Delegate: InputActionEvent
        // An Action to distribute a <InputAction> via the <TransmitInputAction> event listener.
        public delegate void InputActionEvent(InputAction _inputData);

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
        [Tooltip("These plugins modify InputActions and are performed in order.")]
        [SerializeField] ToggleablePlugin[] plugins;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                return;
            }
            Instance = this;
        }

        internal void SendInputAction(InputAction _inputAction)
        {
            TransmitRawInputAction?.Invoke(_inputAction);

            InputAction? modifiedInputAction = RunPlugins(_inputAction);

            if (modifiedInputAction.HasValue)
            {
                TransmitInputAction?.Invoke(modifiedInputAction.Value);
            }
        }

        InputAction? RunPlugins(InputAction _inputAction)
        {
            InputAction? modifiedInputAction = _inputAction;

            // Send the input action through the plugins in order
            // if it is returned null from a plugin, return it to be ignored
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

            return modifiedInputAction;
        }
    }

    // Struct: ToggleablePlugin
    // A Data structure used to toggle the use of plugins.
    [System.Serializable]
    internal struct ToggleablePlugin
    {
        public bool enabled;
        public InputActionPlugin plugin;
    }
}