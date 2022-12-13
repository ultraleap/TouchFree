import TouchFree, { EventHandle } from '../TouchFree';
import { TouchFreeInputAction, InputType } from '../TouchFreeToolingTypes';

/**
 * Converts {@link TouchFreeInputAction}s into inputs for specific environments.
 * 
 * @remarks
 * This base class handles subscribing to the TouchFree `'TransmitInputAction'` event.
 * Override {@link HandleInputAction} in subclasses to implement specific behaviour.
 * @public
 */
export abstract class BaseInputController {

    private static Instantiated = false;
    private HandleInputActionCallback: EventHandle | undefined;

    /**
     * Subscribes to the TouchFree `'TransmitInputAction'` event, invoke {@link HandleInputAction}
     * with {@link TouchFreeInputAction}s as they are received.
     * 
     * @remarks
     * Calling this constructor more than once without {@link disconnect}ing the previous
     * is a no-op - only one `InputController` can be initialized at one time.
     */
    constructor() {
        if (!BaseInputController.Instantiated) {
            BaseInputController.Instantiated = true;
            this.HandleInputActionCallback = TouchFree.RegisterEventCallback(
                'TransmitInputAction',
                this.HandleInputAction.bind(this)
            );
        }
    }

    /**
     * Override to implement `InputController` specific behaviour for {@link TouchFreeInputAction}s
     * @param _inputData - The latest input action received from TouchFree Service.
     */
    protected HandleInputAction(_inputData: TouchFreeInputAction): void {
        switch (_inputData.InputType) {
            case InputType.MOVE:
                break;

            case InputType.DOWN:
                break;

            case InputType.UP:
                break;

            case InputType.CANCEL:
                break;
        }
    }

    /**
     * Unregisters the event callback and resets initialization state.
     * 
     * @remarks
     * Must be called before constructing another `InputController` when
     * switching. Only one can be active at a time.
     */
    disconnect() {
        this.HandleInputActionCallback?.UnregisterEventCallback();
        BaseInputController.Instantiated = false;
    }
}
