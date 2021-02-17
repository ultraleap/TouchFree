import { ConnectionManager } from "../Connection/ConnectionManager";
import { ClientInputAction, InputType } from "../ScreenControlTypes";

// Class: InputController
// A layer over Unity's <BaseInput: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.BaseInput.html>
// that connects the BaseInput to ScreenControl's <ClientInputActions> as they are
// provided. Contains setup functions allowing inheritors to define only the input behaviour
// they wish to see.
//
// Override <HandleInputAction> to react to ClientInputActions as they are recieved.
//
// For an example InputController, see <UnityUIInputController>.
export abstract class BaseInputController {
    // Group: MonoBehaviour Overrides

    // Function: Start
    // Adds a listener to <ConnectionManager> to invoke <HandleInputAction> with <ClientInputActions> as they
    // are received.
    constructor() {
        ConnectionManager.instance.addEventListener('TransmitInputAction',
            ((e: CustomEvent<ClientInputAction>) => {
                this.HandleInputAction(e.detail);
            }) as EventListener);
    }

    // Functions:

    // Function: HandleInputAction
    // This method is the core of the functionality of this class. It will be invoked with
    // the <ClientInputAction> as they are provided to the Client from the ScreenControl Service.
    //
    // Override this function to implement any custom input handling functionality you wish to see.
    //
    // Parameters:
    //     _inputData - The latest input action recieved from ScreenControl Service.
    protected HandleInputAction(_inputData: ClientInputAction): void {
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

}
