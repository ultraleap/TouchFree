import { TouchFreeInputAction } from "../TouchFreeToolingTypes";
import { InputActionManager } from "../Plugins/InputActionManager";
import { SwipeDirection } from "../Connection/TouchFreeServiceTypes";

// Class: TouchlessCursor
// This class is a base class for creating custom Touchless cursors for use with TouchFree Tooling.
//
// Override <HandleInputAction> to react to <TouchFreeInputActions> as they are recieved.
//
// For an example of a reactive cursor, see <DotCursor>.
export abstract class TouchlessCursor {
  // Group: Variables

  // Variable: cursor
  // The HTMLElement that represents this cursor
  cursor: HTMLElement | SVGElement | undefined;

  // Group: Functions

  // Function: constructor
  // Registers the Cursor for updates from the <InputActionManager>
  //
  // If you intend to make use of the <WebInputController>, make sure that _cursor has the
  // "touchfree-cursor" class. This prevents it blocking other elements from recieving events.
  constructor(_cursor: HTMLElement | SVGElement | undefined) {
    InputActionManager.instance.addEventListener("TransmitInputAction", ((
      e: CustomEvent<TouchFreeInputAction>
    ) => {
      this.HandleInputAction(e.detail);
    }) as EventListener);

    this.cursor = _cursor;
  }

  // Function: UpdateCursor
  // Sets the position of the cursor, should be run after <HandleInputAction>.
  UpdateCursor(_inputAction: TouchFreeInputAction): void {
    if (this.cursor) {
      this.cursor.style.left =
        _inputAction.CursorPosition[0] - this.cursor.clientWidth / 2 + "px";
      this.cursor.style.top =
        _inputAction.CursorPosition[1] - this.cursor.clientHeight / 2 + "px";
    }
  }

  // Function: HandleInputAction
  // The core of the logic for Cursors, this is invoked with each <TouchFreeInputAction> as
  // they are recieved. Override this function to implement cursor behaviour in response.
  //
  // Parameters:
  //    _inputAction - The latest input action recieved from TouchFree Service.
  HandleInputAction(_inputAction: TouchFreeInputAction): void {
    this.UpdateCursor(_inputAction);
  }

  // Function: ShowCursor
  // Used to make the cursor visible
  ShowCursor(): void {
    if (this.cursor) {
      this.cursor.style.opacity = "1";
    }
  }

  // Function: HideCursor
  // Used to make the cursor invisible
  HideCursor(): void {
    if (this.cursor) {
      this.cursor.style.opacity = "0";
    }
  }

  abstract ShowCloseToSwipe(): void;
  abstract HideCloseToSwipe(): void;
  abstract SetSwipeDirection(direction?: SwipeDirection): void;
}
