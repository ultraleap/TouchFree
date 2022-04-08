import { ConfigurationManager } from "../TouchFree/Configuration/ConfigurationManager";
import { InputActionPlugin } from "../TouchFree/Plugins/InputActionPlugin";
import { InputOverride, TouchFreeInputAction } from "../TouchFree/TouchFreeToolingTypes";

export class InputPlugin extends InputActionPlugin {
    ModifyInputAction(_inputAction: TouchFreeInputAction): TouchFreeInputAction | null {
        let override: InputOverride = new InputOverride(_inputAction.InputType, _inputAction.CursorPosition);

        ConfigurationManager.SendInputOverride(override);

        return _inputAction;
    }
}