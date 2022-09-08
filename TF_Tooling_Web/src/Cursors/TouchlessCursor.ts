import { TouchFreeInputAction } from '../TouchFreeToolingTypes';
import { InputActionManager } from '../Plugins/InputActionManager';


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

    // Variable: enabled
    // Whether the cursor should hide and show depending on hand presence
    enabled: boolean;

    // Variable: cursor
    // Whether the cursor should be visible or not after being enabled
    shouldShow: boolean;

    // Group: Functions

    // Function: constructor
    // Registers the Cursor for updates from the <InputActionManager>
    //
    // If you intend to make use of the <WebInputController>, make sure that _cursor has the
    // "touchfree-cursor" class. This prevents it blocking other elements from recieving events.
    constructor(_cursor: HTMLElement | SVGElement | undefined) {
        InputActionManager.instance.addEventListener('TransmitInputAction', ((e: CustomEvent<TouchFreeInputAction>) => {
            this.HandleInputAction(e.detail);
        }) as EventListener);

        this.cursor = _cursor;
        this.enabled = true;
        this.shouldShow = true;
    }

    // Function: UpdateCursor
    // Sets the position of the cursor, should be run after <HandleInputAction>.
    UpdateCursor(_inputAction: TouchFreeInputAction): void {
        if (this.cursor) {
            this.cursor.style.left = (_inputAction.CursorPosition[0] - (this.cursor.clientWidth / 2)) + "px";
            this.cursor.style.top = (_inputAction.CursorPosition[1] - (this.cursor.clientHeight / 2)) + "px";
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
        this.shouldShow = true;
        if (this.cursor && this.enabled) {
            this.cursor.style.opacity = "1";
        }
    }

    // Function: HideCursor
    // Used to make the cursor invisible
    HideCursor(): void {
        this.shouldShow = false;
        if (this.cursor) {
            this.cursor.style.opacity = "0";
        }
    }

    // Function: EnableCursor
    // Used to enable the cursor so that it will show if hands are present
    EnableCursor(): void {
        this.enabled = true;
        if (this.shouldShow) {
            this.ShowCursor();
        }
    }

    // Function: DisableCursor
    // Used to disable the cursor so that it will never show
    DisableCursor(): void {
        this.enabled = false;
        const shouldShowOnEnable = this.shouldShow;
        if (shouldShowOnEnable) {
            this.HideCursor();
        }
        this.shouldShow = shouldShowOnEnable;
    }
}