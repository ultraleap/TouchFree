import TouchFree from '../TouchFree';
import { TouchFreeInputAction } from '../TouchFreeToolingTypes';

/**
 * Base class for creating touchless cursors.
 * 
 * @remarks
 * Override {@link HandleInputAction} to react to {@link TouchFreeInputAction}s
 * @public
 */
export abstract class TouchlessCursor {
    /**
     * The {@link HTMLElement} or {@link SVGElement} that represents this cursor
     */
    cursor: HTMLElement | SVGElement | undefined;

    /**
     * Whether the cursor should hide and show depending on hand presence
     */
    enabled: boolean;

    /**
     * Whether the cursor should be visible or not after being enabled
     */
    shouldShow: boolean;

    /**
     * Registers the Cursor for updates via the `'TransmitInputAction'` TouchFree event
     * 
     * @remarks
     * If you intend to make use of `WebInputController`, make sure both {@link _cursor} has
     * the `touchfree-cursor` class. This prevents them from blocking other elements from
     * receiving events.
     * @param _cursor - Cursor element
     */
    constructor(_cursor: HTMLElement | SVGElement | undefined) {
        TouchFree.RegisterEventCallback('TransmitInputAction', this.HandleInputAction.bind(this));

        this.cursor = _cursor;
        this.enabled = true;
        this.shouldShow = true;
    }

    /**
     * Sets the position of the cursor, should be run after {@link HandleInputAction}.
     * @param _inputAction - Input action to use when updating cursor
     */
    UpdateCursor(_inputAction: TouchFreeInputAction): void {
        if (this.cursor) {
            this.cursor.style.left = _inputAction.CursorPosition[0] - this.cursor.clientWidth / 2 + 'px';
            this.cursor.style.top = _inputAction.CursorPosition[1] - this.cursor.clientHeight / 2 + 'px';
        }
    }

    /**
     * Invoked when new {@link TouchFreeInputAction}s are received.
     * Override to implement cursor behaviour.
     * @param _inputAction - The latest input action received from TouchFree Service.
     */
    HandleInputAction(_inputAction: TouchFreeInputAction): void {
        this.UpdateCursor(_inputAction);
    }

    /**
     * Make the cursor visible. Fades over time.
     */
    ShowCursor(): void {
        this.shouldShow = true;
        if (this.cursor && this.enabled) {
            this.cursor.style.opacity = '1';
        }
    }

    /**
     * Make the cursor invisible. Fades over time.
     */
    HideCursor(): void {
        this.shouldShow = false;
        if (this.cursor) {
            this.cursor.style.opacity = '0';
        }
    }

    /**
     * Used to enable the cursor so that it will show if hands are present
     */
    EnableCursor(): void {
        this.enabled = true;
        if (this.shouldShow) {
            this.ShowCursor();
        }
    }

    /**
     * Used to disable the cursor so that it will never show
     */
    DisableCursor(): void {
        this.enabled = false;
        const shouldShowOnEnable = this.shouldShow;
        if (shouldShowOnEnable) {
            this.HideCursor();
        }
        this.shouldShow = shouldShowOnEnable;
    }
}
