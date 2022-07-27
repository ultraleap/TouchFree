import { InputActionPlugin } from "./InputActionPlugin";

// Class: InputActionManager
// The manager for all <TouchFreeInputActions> to be handled and distributed. This runs the
// received data through any <InputActionPlugins> given to it and finaly distributes the data
// via the  <TransmitInputAction> event which should be listened to by any class hoping to make
// use of incoming <TouchFreeInputActions>.
export class HandDataManager extends EventTarget {
    // Event: TransmitHandData
    // An event for transmitting <TouchFreeInputActions> that are received via the <messageReceiver> to
    // be listened to.

    // Event: TransmitInputActionRaw
    // An event for immediately transmitting <TouchFreeInputActions> that are received via the
    // <messageReceiver> to be listened to. This is transmitted before any Plugins are executed.

    // Variable: instance
    // The instance of the singleton for referencing the events transmitted
    private static _instance: HandDataManager;

    static plugins: Array<InputActionPlugin> | null = null;

    public static get instance() {
        if (HandDataManager._instance === undefined) {
            HandDataManager._instance = new HandDataManager();
        }

        return HandDataManager._instance;
    }

    static lastFrame: number | undefined = undefined;

    // Function: SetPlugins
    // Use this function to set the <InputActionPlugins> that the manager should use, as well as the order the
    // <InputActionPlugins> should be used.
    public static SetPlugins(_plugins: Array<InputActionPlugin>): void {
        this.plugins = _plugins;
    }

    // Function: HandleInputAction
    // Called by the <messageReceiver> to relay a <TouchFreeInputAction> that has been received to any
    // listeners of <TransmitInputAction>.
    public static HandleInputAction(_data: any): void {
        const currentTimeStamp = Date.now();
        if (!HandDataManager.lastFrame || HandDataManager.lastFrame + 100 < currentTimeStamp ) {
            let rawHandsEvent: CustomEvent<any> = new CustomEvent<any>(
                'TransmitHandData',
                { detail: _data }
            );
            HandDataManager.instance.dispatchEvent(rawHandsEvent);
            HandDataManager.lastFrame = currentTimeStamp;
        }
    }
}