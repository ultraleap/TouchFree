import { TouchFreeInputAction } from '../TouchFreeToolingTypes';

/**
 * Base class for input action plugins
 * @remarks
 * The `InputActionManager` runs each plugin upon receiving a message
 * from the service before dispatching an InputAction event.
 * Input action plugins invoke a `"InputActionOutput"` event on themselves
 * for subscribers to listen to if the results of a specific plugin is required.
 * @public
 */
export abstract class InputActionPlugin extends EventTarget {
    /**
     * Run this plugin, modifying the `InputAction` and dispatching an `"InputActionOutput"` event from this plugin
     * @param _inputAction Input action input
     * @returns Modified input action
     */
    RunPlugin(_inputAction: TouchFreeInputAction): TouchFreeInputAction | null {
        const modifiedInputAction = this.ModifyInputAction(_inputAction);

        if (modifiedInputAction != null) {
            this.TransmitInputAction(modifiedInputAction);
        }

        return modifiedInputAction;
    }

    /**
     * Proxy function for derived classes to modify input actions before they are dispatched.
     * 
     * @internal
     */
    ModifyInputAction(_inputAction: TouchFreeInputAction): TouchFreeInputAction | null {
        return _inputAction;
    }

    /**
     * For derived classes to invoke the `InputActionOutput` event.
     * @param _inputAction InputAction state to dispatch event with
     * 
     * @internal
     */
    TransmitInputAction(_inputAction: TouchFreeInputAction): void {
        const InputActionEvent: CustomEvent<TouchFreeInputAction> = new CustomEvent<TouchFreeInputAction>(
            'InputActionOutput',
            { detail: _inputAction }
        );
        this.dispatchEvent(InputActionEvent);
    }
}
