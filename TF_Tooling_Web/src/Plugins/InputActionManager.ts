import { TouchFreeInputAction } from "../TouchFreeToolingTypes";
import { InputActionPlugin } from "./InputActionPlugin";

// Class: InputActionManager
// The manager for all <TouchFreeInputActions> to be handled and distributed. This runs the
// received data through any <InputActionPlugins> given to it and finaly distributes the data
// via the  <TransmitInputAction> event which should be listened to by any class hoping to make
// use of incoming <TouchFreeInputActions>.
export class InputActionManager extends EventTarget {
    // Event: TransmitInputAction
    // An event for transmitting <TouchFreeInputActions> that are received via the <messageReceiver> to
    // be listened to.

    // Event: TransmitInputActionRaw
    // An event for immediately transmitting <TouchFreeInputActions> that are received via the
    // <messageReceiver> to be listened to. This is transmitted before any Plugins are executed.

    // Variable: instance
    // The instance of the singleton for referencing the events transmitted
    static _instance: InputActionManager;

    static plugins: Array<InputActionPlugin> | null = null;

    public static get instance() {
        if (InputActionManager._instance === undefined) {
            InputActionManager._instance = new InputActionManager();
        }

        return InputActionManager._instance;
    }

    // Function: SetPlugins
    // Use this function to set the <InputActionPlugins> that the manager should use, as well as the order the
    // <InputActionPlugins> should be used.
    public static SetPlugins(_plugins: Array<InputActionPlugin>): void {
        this.plugins = _plugins;
    }

    // Function: HandleInputAction
    // Called by the <messageReceiver> to relay a <TouchFreeInputAction> that has been received to any
    // listeners of <TransmitInputAction>.
    public static HandleInputAction(_action: TouchFreeInputAction): void {

        let rawInputActionEvent: CustomEvent<TouchFreeInputAction> = new CustomEvent<TouchFreeInputAction>(
            'TransmitInputActionRaw',
            { detail: _action }
        );
        InputActionManager.instance.dispatchEvent(rawInputActionEvent);

        let action = _action;

        if (this.plugins !== null) {
            for (var i = 0; i < this.plugins.length; i++) {
                let modifiedAction = this.plugins[i].RunPlugin(action);

                if (modifiedAction !== null) {
                    action = modifiedAction;
                } else {
                    // The plugin has cancelled the InputAction entirely
                    return;
                }
            }
        }

        let inputActionEvent: CustomEvent<TouchFreeInputAction> = new CustomEvent<TouchFreeInputAction>(
            'TransmitInputAction',
            { detail: action }
        );

        // Wrapping the function in a timeout of 0 seconds allows the dispatch to be asynchronous
        setTimeout(() => {
            InputActionManager.instance.dispatchEvent(inputActionEvent);
        }, 0);
    }

    public static HandleCloseToSwipe(): void {
        let closeToSwipeEvent: CustomEvent = new CustomEvent(
            'TransmitCloseToSwipe'
        );
        InputActionManager.instance.dispatchEvent(closeToSwipeEvent);
    }
}