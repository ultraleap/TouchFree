import { ConfigurationManager } from "../TouchFree/Configuration/ConfigurationManager";
import { InputActionPlugin } from "../TouchFree/Plugins/InputActionPlugin";
import { InputOverride, TouchFreeInputAction, Vector2 } from "../TouchFree/TouchFreeToolingTypes";

export class InputPlugin extends InputActionPlugin {
    ModifyInputAction(_inputAction: TouchFreeInputAction): TouchFreeInputAction | null {
        let newY = (-1 * window.devicePixelRatio) * (_inputAction.CursorPosition[1] - window.innerHeight);

        let pos: Vector2 = new Vector2(_inputAction.CursorPosition[0], newY);

        let override: InputOverride = new InputOverride(_inputAction.InputType, pos);

        ConfigurationManager.SendInputOverride(override);

        return _inputAction;
    }
}