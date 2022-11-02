import { ConnectionManager } from "./Connection/ConnectionManager";
import { SVGCursor, SVGCursorParams } from "./Cursors/SvgCursor";
import { TouchlessCursor } from "./Cursors/TouchlessCursor";
import { WebInputController } from "./InputControllers/WebInputController";

var InputController: WebInputController;
export var CurrentCursor : TouchlessCursor;

export function Init(_cursor: TouchlessCursor): void;
export function Init(_svgParams?: SVGCursorParams): void;
export function Init(_cursorOrParams?: SVGCursorParams | TouchlessCursor): void {
    ConnectionManager.init();

    InputController = new WebInputController();

    if (_cursorOrParams instanceof TouchlessCursor) {
        CurrentCursor = _cursorOrParams;
    }
    else if (_cursorOrParams !== undefined) {
        CurrentCursor = SVGCursor.FromParamObj(_cursorOrParams);
    } else {
        CurrentCursor = new SVGCursor();
    }
}