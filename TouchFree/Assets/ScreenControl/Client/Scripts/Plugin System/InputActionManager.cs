using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    // Class: InputActionManager
    // The manager for all <ClientInputActions> to be handled and distributed. This runs the
    // received data through any referenced <InputActionPlugins> and finaly distributes the data
    // via the  <TransmitInputAction> event which should be listened to by any class hoping to make
    // use of incoming <ClientInputActions>.
    [DefaultExecutionOrder(-1)]
    public class InputActionManager : MonoBehaviour
    {
        // Delegate: ClientInputActionEvent
        // An Action to distribute a <ClientInputAction> via the <TransmitInputAction> event listener.
        public delegate void ClientInputActionEvent(ClientInputAction _inputData);

        // Variable: TransmitInputAction
        // An event for transmitting <ClientInputActions> that have been modified via the active
        // <plugins>
        public static event ClientInputActionEvent TransmitInputAction;

        // Variable: TransmitRawInputAction
        // An event for transmitting <ClientInputActions> that have NOT been modified via any
        // <plugins>
        public static event ClientInputActionEvent TransmitRawInputAction;

        public static InputActionManager Instance;

        // Variable: plugins
        // A pre-defined plugin array of <ToggleablePlugins> that modify incoming <ClientInputActions>
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

        internal void SendInputAction(ClientInputAction _inputAction)
        {
            TransmitRawInputAction?.Invoke(_inputAction);
            RunPlugins(ref _inputAction);
            TransmitInputAction?.Invoke(_inputAction);
        }

        void RunPlugins(ref ClientInputAction _inputAction)
        {
            // Send the input action through the plugins in order
            // if it is returned null from a plugin, return it to be ignored
            foreach (var plugin in plugins)
            {
                if (plugin.enabled)
                {
                    _inputAction = plugin.plugin.RunPlugin(_inputAction);
                }
            }
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