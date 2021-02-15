import { ClientInputAction } from '../ScreenControlTypes';
import { ConnectionManager } from '../Connection/ConnectionManager';

// Class: TouchlessCursor
// This class is a base class for creating custom Touchless cursors for use with ScreenControl.
//
// Override <HandleInputAction> to react to <ClientInputActions> as they are recieved.
//
// For an example of a reactive cursor, see <DotCursor>.
export abstract class TouchlessCursor {
    // Group: Variables

    // Variable: cursor
    // The HTMLElement that represents this cursor
    cursor: HTMLElement;

    // Group: Functions

    // Function: constructor
    // Initialises the cursor at its default state.
    // Also registers the Cursor for updates from the <ConnectionManager>
    constructor(_cursor: HTMLElement) {
        ConnectionManager.instance.addEventListener('TransmitInputAction', ((e: CustomEvent<ClientInputAction>) => {
            this.HandleInputAction(e.detail);
        }) as EventListener);

        this.cursor = _cursor;
    }

    // Function: UpdateCursor
    // Sets the position of the cursor, should be run after <HandleInputAction>.
    UpdateCursor(_targetPos: Array<number>): void {
        this.cursor.style.left = (_targetPos[0] - (this.cursor.clientWidth / 2)) + "px";
        this.cursor.style.top = (window.innerHeight - (_targetPos[1] + (this.cursor.clientHeight / 2))) + "px";
    }

    // Function: HandleInputAction
    // The core of the logic for Cursors, this is invoked with each <ClientInputAction> as
    // they are recieved. Override this function to implement cursor behaviour in response.
    //
    // Parameters:
    //    _inputData - The latest input action recieved from ScreenControl Service.
    HandleInputAction(_inputData: ClientInputAction): void {
        this.UpdateCursor(_inputData.CursorPosition);
    }
}