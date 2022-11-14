import TouchFree, { EventHandle } from '../TouchFree';
import { TouchFreeInputAction, InputType } from "../TouchFreeToolingTypes";

// Class: InputController
// InputControllers convert <TouchFreeInputActions> as recieved from the service into appropriate
// inputs for the given environment. This abstract handles connection and should be inherited from
// to develop any further InputControllers.
//
// Override <HandleInputAction> to react to TouchFreeInputActions as they are recieved.
//
// For an example InputController, see <WebInputController>.
export abstract class BaseInputController {
    // Group: MonoBehaviour Overrides

    private static Instantiated: boolean = false;
    private HandleInputActionCallback:EventHandle|undefined;

    // Function: constructor
    // Adds a listener to <InputActionManager> to invoke <HandleInputAction> with <TouchFreeInputActions> as they
    // are received.
    constructor() {
        if (!BaseInputController.Instantiated) {
            BaseInputController.Instantiated = true;
            this.HandleInputActionCallback = TouchFree.RegisterEventCallback("TransmitInputAction", this.HandleInputAction.bind(this));
        }
    }


    // Functions:

    // Function: HandleInputAction
    // This method is the core of the functionality of this class. It will be invoked with
    // the <TouchFreeInputAction> as they are provided to the Tooling from the TouchFree Service.
    //
    // Override this function to implement any custom input handling functionality you wish to see.
    //
    // Parameters:
    //     _inputData - The latest input action recieved from TouchFree Service.
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

    disconnect() {
        this.HandleInputActionCallback?.UnregisterEventCallback();
        BaseInputController.Instantiated = false;
    }
}