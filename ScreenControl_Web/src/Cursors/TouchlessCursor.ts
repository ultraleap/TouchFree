import {
    ClientInputAction
} from '../ScreenControlTypes';
import { ConnectionManager } from '../Connection/ConnectionManager';

// Class: TouchlessCursor
// This class is a base class for creating custom Touchless cursors for use with ScreenControl.
//
// Override <HandleInputAction> to react to <ClientInputActions> as they are recieved.
//
// For an example of a reactive cursor, see <DotCursor>.
export abstract class TouchlessCursor {
    // Group: Variables

    // Variable: cursorTransform
    // The transform for the image presented by this cursor
    //public RectTransform cursorTransform;

    cursor: HTMLElement;

    // Function: OnEnable
    // Initialises & displays the cursor to its default state when the scene is fully loaded.
    // Also registers the Cursor for updates from the <ConnectionManager>
    constructor(_cursor: HTMLElement) {
        ConnectionManager.instance.addEventListener('TransmitInputAction', ((e: CustomEvent<ClientInputAction>) => {
            this.HandleInputAction(e.detail);
        }) as EventListener);

        this.cursor = _cursor;

        this.InitialiseCursor();
        this.ShowCursor();
    }

    // Function: UpdateCursor
    // Sets the position of the cursor, should be run after <HandleInputAction>.
    UpdateCursor(_targetPos: Array<number>): void {
        this.cursor.style.left = (_targetPos[0] - (this.cursor.clientWidth / 2)) + "px";
        this.cursor.style.top = (window.innerHeight - (_targetPos[1] + (this.cursor.clientHeight / 2))) + "px";
    }

    // Function: OnDisable
    // Deregister the Cursor so it no longer recieves updates from the
    // <ConnectionManager>
    OnDisable(): void {
    }

    // Group: Functions

    // Function: HandleInputAction
    // The core of the logic for Cursors, this is invoked with each <ClientInputAction> as
    // they are recieved. Override this function to implement cursor behaviour in response.
    //
    // Parameters:
    //    _inputData - The latest input action recieved from ScreenControl Service.
    HandleInputAction(_inputData: ClientInputAction): void {
        this.UpdateCursor(_inputData.CursorPosition);
    }

    // Function: InitialiseCursor
    // Override this function with any intiialisation steps your cursor needs.
    InitialiseCursor(): void {

    }

    // Function: ShowCursor
    // This ensures that all Cursors will have the ability to be shown or hidden. Be sure to
    // override this function to cover the showing of anything an inheriting cursor uses.
    ShowCursor(): void {

    }

    // Function: HideCursor
    // This ensures that all Cursors will have the ability to be shown or hidden. Be sure to
    // override this function to cover the hiding of anything an inheriting cursor uses.
    HideCursor(): void {
        this.cursor.style.left = "-200px";
        this.cursor.style.top = "-200px";
    }
}