import { ClientInputAction } from "../ScreenControlTypes";

export class InputActionManager extends EventTarget {
    // Event: TransmitInputAction
    // An event for transmitting <ClientInputActions> that are received via the <messageReceiver> to
    // be listened to.

    // Variable: instance
    // The instance of the singleton for referencing the events transmitted
    static _instance: InputActionManager;

    public static get instance() {
        if (InputActionManager._instance == null) {
            InputActionManager._instance = new InputActionManager();
        }

        return InputActionManager._instance;
    }

    // Function: HandleInputAction
    // Called by the <messageReceiver> to relay a <ClientInputAction> that has been received to any
    // listeners of <TransmitInputAction>.
    public static HandleInputAction(_action: ClientInputAction): void {
        let inputActionEvent: CustomEvent<ClientInputAction> = new CustomEvent<ClientInputAction>(
            'TransmitInputAction',
            { detail: _action }
        );

        InputActionManager.instance.dispatchEvent(inputActionEvent);
    }
}