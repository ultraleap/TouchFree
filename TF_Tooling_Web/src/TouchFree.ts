import { ConnectionManager } from "./Connection/ConnectionManager";
import { SVGCursor, SVGCursorParams } from "./Cursors/SvgCursor";
import { TouchlessCursor } from "./Cursors/TouchlessCursor";
import { WebInputController } from "./InputControllers/WebInputController";

var InputController: WebInputController;
export var CurrentCursor: TouchlessCursor;

export function Init(_cursor: TouchlessCursor): void;
export function Init(_svgParams?: SVGCursorParams): void;
export function Init(_cursorOrParams?: SVGCursorParams | TouchlessCursor): void {
    ConnectionManager.init();

    ConnectionManager.AddConnectionListener(() => {
        InputController = new WebInputController();

        if (_cursorOrParams instanceof TouchlessCursor) {
            console.log("arg was a Cursor");
            CurrentCursor = _cursorOrParams;
        }
        else if (_cursorOrParams !== undefined) {
            console.log("arg was cursor params");
            CurrentCursor = SVGCursor.FromParamObj(_cursorOrParams);
        } else {
            console.log("arg was undef");
            CurrentCursor = new SVGCursor();
        }
    });
}