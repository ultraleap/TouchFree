import TouchFree from "../TouchFree";
import { TouchFreeEvent, TouchFreeInputAction } from "../TouchFreeToolingTypes";

export abstract class InputActionPlugin extends EventTarget{
    // Event: InputActionOutput
    // An event for transmitting <TouchFreeInputActions> as they pass through this plugin.
    // This can be used to access the data as it is used by a specific plugin, as to intercept
    // the full cycle of plugins that the <InputActionManager> references.

    // Function: RunPlugin
    // Called from <InputActionManager> and provided a <TouchFreeInputAction> as a parameter.
    // This function is a wrapper that guarantees that the results of <ModifyInputAction> are both
    // returned to the <InputActionManager> and transmitted via <TransmitInputAction>.
    RunPlugin(_inputAction: TouchFreeInputAction): TouchFreeInputAction | null {
        let modifiedInputAction = this.ModifyInputAction(_inputAction);

        if (modifiedInputAction != null) {
            this.TransmitInputAction(modifiedInputAction);
        }

        return modifiedInputAction;
    }

    // Function: ModifyInputAction
    // Called from <RunPlugin> and provided a <InputAction> as a parameter.
    // This function is used to manipulate the incoming <TouchFreeInputAction>
    // data. Returns a <TouchFreeInputAction> which is then distributed via the <InputActionManager>.
    ModifyInputAction(_inputAction: TouchFreeInputAction): TouchFreeInputAction | null{
        return _inputAction;
    }

    // Function: TransmitInputAction
    // To be used to Invoke the InputActionOutput event from any child class of this base.
    TransmitInputAction(_inputAction: TouchFreeInputAction): void{
        let InputActionEvent: CustomEvent<TouchFreeInputAction> = new CustomEvent<TouchFreeInputAction>(
            "InputActionOutput",
            { detail: _inputAction }
        );
        this.dispatchEvent(InputActionEvent);
    }
}