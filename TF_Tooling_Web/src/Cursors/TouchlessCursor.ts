import TouchFree from '../TouchFree';
import { TouchFreeInputAction } from '../TouchFreeToolingTypes';

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

    // Variable: opacityOnHandsLost
    // The opacity of the cursor when hands are lost
    protected opacityOnHandsLost = 1;

    // Group: Functions

    // Function: constructor
    // Registers the Cursor for updates from the <InputActionManager>
    //
    // If you intend to make use of the <WebInputController>, make sure that _cursor has the
    // "touchfree-cursor" class. This prevents it blocking other elements from recieving events.
    constructor(_cursor: HTMLElement | SVGElement | undefined) {
        TouchFree.RegisterEventCallback('TransmitInputAction', this.HandleInputAction.bind(this));

        this.cursor = _cursor;
        this.enabled = true;
        this.shouldShow = true;
    }

    // Function: UpdateCursor
    // Sets the position of the cursor, should be run after <HandleInputAction>.
    protected UpdateCursor(_inputAction: TouchFreeInputAction): void {
        if (this.cursor) {
            let width = this.cursor.clientWidth;
            let height = this.cursor.clientHeight;
            if (this.cursor instanceof HTMLElement) {
                [width, height] = this.GetDimensions(this.cursor);
            }

            this.cursor.style.left = _inputAction.CursorPosition[0] - width / 2 + 'px';
            this.cursor.style.top = _inputAction.CursorPosition[1] - height / 2 + 'px';
        }
    }

    protected GetDimensions(cursor: HTMLElement): [number, number] {
        if (cursor.style.width && cursor.style.height) {
            const getFloat = (dimension: string) => parseFloat(dimension.replace('px', ''));
            return [getFloat(cursor.style.width), getFloat(cursor.style.height)];
        }

        const newCursor = cursor as HTMLImageElement;
        return [newCursor.width, newCursor.height];
    }

    // Function: HandleInputAction
    // The core of the logic for Cursors, this is invoked with each <TouchFreeInputAction> as
    // they are received. Override this function to implement cursor behaviour in response.
    //
    // Parameters:
    //    _inputAction - The latest input action received from TouchFree Service.
    protected HandleInputAction(_inputAction: TouchFreeInputAction): void {
        this.UpdateCursor(_inputAction);
    }

    // Function: ShowCursor
    // Used to make the cursor visible
    ShowCursor(): void {
        this.shouldShow = true;
        if (this.enabled) {
            this.SetCursorOpacity(this.opacityOnHandsLost);
        }
    }

    // Function: HideCursor
    // Used to make the cursor invisible
    HideCursor(): void {
        if (this.shouldShow) {
            // If opacity is NaN or 0 then set it to be 1
            this.opacityOnHandsLost = Number(this.cursor?.style.opacity) || 1;
        }
        this.shouldShow = false;
        this.SetCursorOpacity(0);
    }

    // Function: EnableCursor
    // Used to enable the cursor so that it will show if hands are present
    EnableCursor(): void {
        this.enabled = true;
        if (this.shouldShow) {
            this.opacityOnHandsLost = 1;
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

    // Function: SetCursorOpacity
    // Used to set the opacity of the cursor
    SetCursorOpacity(opacity: number): void {
        if (this.cursor) {
            this.cursor.style.opacity = opacity.toString();
        }
    }
}
