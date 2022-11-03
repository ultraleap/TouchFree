import { ConnectionManager } from "./Connection/ConnectionManager";
import { SVGCursor, SVGCursorParams } from "./Cursors/SvgCursor";
import { TouchlessCursor } from "./Cursors/TouchlessCursor";
import { WebInputController } from "./InputControllers/WebInputController";

var InputController: WebInputController;
export var CurrentCursor: TouchlessCursor;

export interface TfInitParams {
    initialiseCursor?: boolean;
}

export function Init(_tfInitParams?: TfInitParams ): void {
    ConnectionManager.init();

    ConnectionManager.AddConnectionListener(() => {
        InputController = new WebInputController();

        if (_tfInitParams === undefined) {
            CurrentCursor = new SVGCursor();
        } else {
            if (_tfInitParams.initialiseCursor === undefined) {
                CurrentCursor = new SVGCursor();
            } else if (_tfInitParams.initialiseCursor) {
                CurrentCursor = new SVGCursor();
            }
        }
    });
}