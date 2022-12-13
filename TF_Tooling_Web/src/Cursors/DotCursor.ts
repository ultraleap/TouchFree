import TouchFree from '../TouchFree';
import { TouchFreeInputAction, InputType } from '../TouchFreeToolingTypes';
import { MapRangeToRange } from '../Utilities';
import { TouchlessCursor } from './TouchlessCursor';

/**
 * {@link TouchlessCursor} which positions a dot on the screen at the hand location,
 * reacting to the current {@link TouchFreeInputAction.ProgressToClick}.
 * 
 * @remarks
 * {@link TouchFreeInputAction.ProgressToClick} behaviour depends on the active interaction.
 * 
 * @public
 */
export class DotCursor extends TouchlessCursor {
    /**
     * Update duration (in milliseconds) of the animation
     * @defaultValue 30fps (33.33ms)
     */
    readonly animationUpdateDuration: number = (1 / 30) * 1000;

    /**
     * The HTMLElement that visually represents the cursors ring.
     */
    cursorRing: HTMLElement;

    /**
     * The maximum size for the ring to be relative to the size of the dot.
     * 
     * @remarks
     * e.g. a value of 2 means the ring can be (at largest) twice the scale of the dot.
     */
    ringSizeMultiplier: number;

    private cursorStartSize: Array<number>;
    private animationSpeed: Array<number> = [0, 0];

    private currentAnimationInterval = -1;

    private growQueued = false;

    private currentFadingInterval = -1;

    private dotCursorElement: HTMLElement;

    /**
     * Constructs a new cursor consisting of a central cursor and a ring.
     * @remarks
     * If you intend to make use of `WebInputController`, make sure both {@link _cursor} and {@link _cursorRing}
     * elements have the `touchfree-cursor` class. This prevents them from blocking other elements from
     * receiving events.
     * @param _cursor - Cursor HTML element
     * @param _cursorRing - Cursor ring HTML element
     * @param _animationDuration -
     * Optional duration changing the time it takes for 'squeeze'
     * confirmation animation to be performed.
     * @param _ringSizeMultiplier - Optional multiplier to the size the ring can be relative to the main cursor element.
     */
    constructor(_cursor: HTMLElement, _cursorRing: HTMLElement, _animationDuration = 0.2, _ringSizeMultiplier = 2) {
        super(_cursor);
        this.dotCursorElement = _cursor;
        this.cursorRing = _cursorRing;
        this.ringSizeMultiplier = _ringSizeMultiplier;
        this.cursorStartSize = [_cursor.clientWidth.valueOf(), _cursor.clientHeight.valueOf()];

        this.animationSpeed[0] = this.cursorStartSize[0] / 2 / (_animationDuration * 30);
        this.animationSpeed[1] = this.cursorStartSize[1] / 2 / (_animationDuration * 30);

        TouchFree.RegisterEventCallback('HandFound', this.ShowCursor.bind(this));
        TouchFree.RegisterEventCallback('HandsLost', this.HideCursor.bind(this));
    }

    /**
     * Updates the cursor position as well as the size of the ring based on {@link TouchFreeInputAction.ProgressToClick}
     * @param _inputAction - Input action to use when updating cursor
     * @see {@link TouchlessCursor.UpdateCursor}
     * @internal
     */
    override UpdateCursor(_inputAction: TouchFreeInputAction): void {
        if (!this.enabled) return;
        //progressToClick is between 0 and 1. Click triggered at progressToClick = 1
        const ringScaler = MapRangeToRange(_inputAction.ProgressToClick, 0, 1, this.ringSizeMultiplier, 1);

        this.cursorRing.style.opacity = _inputAction.ProgressToClick + '';

        this.cursorRing.style.width = this.dotCursorElement.clientWidth * ringScaler + 'px';
        this.cursorRing.style.height = this.dotCursorElement.clientHeight * ringScaler + 'px';

        this.cursorRing.style.left = _inputAction.CursorPosition[0] - this.cursorRing.clientWidth / 2 + 'px';
        this.cursorRing.style.top = _inputAction.CursorPosition[1] - this.cursorRing.clientHeight / 2 + 'px';

        super.UpdateCursor(_inputAction);
    }

    /**
     * Replaces the basic functionality of {@link TouchlessCursor}
     * 
     * @remarks
     * Makes the cursor ring scale dynamically with {@link TouchFreeInputAction.ProgressToClick};
     * creates a 'shrink' animation when a {@link InputType.DOWN} event is received;
     * creates a 'grow' animation when a {@link InputType.UP} event is received.
     * 
     * When a {@link InputType.CANCEL} event is received the cursor is hidden as it suggests the hand
     * has been lost. When hidden and any other event is received, the cursor is shown again.
     * @param _inputData - Input action to handle this update
     * @internal
     */
    override HandleInputAction(_inputData: TouchFreeInputAction): void {
        switch (_inputData.InputType) {
            case InputType.MOVE:
                this.UpdateCursor(_inputData);
                break;
            case InputType.DOWN:
                this.SetCursorSize(0, 0, this.cursorRing);

                if (this.currentAnimationInterval !== -1) {
                    clearInterval(this.currentAnimationInterval);
                }

                this.currentAnimationInterval = setInterval(
                    this.ShrinkCursor.bind(this) as TimerHandler,
                    this.animationUpdateDuration
                );
                break;
            case InputType.UP:
                if (this.currentAnimationInterval !== -1) {
                    this.growQueued = true;
                } else {
                    this.growQueued = false;
                    this.currentAnimationInterval = setInterval(
                        this.GrowCursor.bind(this) as TimerHandler,
                        this.animationUpdateDuration
                    );
                }
                break;

            case InputType.CANCEL:
                break;
        }
    }

    /**
     * Shrinks the cursor to half of its original size over the animation duration set in the `constructor`.
     * @internal
     */
    ShrinkCursor(): void {
        if (!this.enabled) return;
        let newWidth = this.dotCursorElement.clientWidth;
        let newHeight = this.dotCursorElement.clientHeight;

        if (this.dotCursorElement.clientWidth > this.cursorStartSize[0] / 2) {
            newWidth = this.dotCursorElement.clientWidth - this.animationSpeed[0];
        }

        if (this.dotCursorElement.clientHeight > this.cursorStartSize[1] / 2) {
            newHeight = this.dotCursorElement.clientHeight - this.animationSpeed[1];
        }

        this.SetCursorSize(newWidth, newHeight, this.dotCursorElement);

        if (newWidth <= this.cursorStartSize[0] / 2 && newHeight <= this.cursorStartSize[1] / 2) {
            clearInterval(this.currentAnimationInterval);

            newWidth = this.cursorStartSize[0] / 2;
            newHeight = this.cursorStartSize[1] / 2;

            this.SetCursorSize(newWidth, newHeight, this.dotCursorElement);

            if (this.growQueued) {
                this.growQueued = false;
                this.currentAnimationInterval = setInterval(
                    this.GrowCursor.bind(this) as TimerHandler,
                    this.animationUpdateDuration
                );
            } else {
                this.currentAnimationInterval = -1;
            }
        }
    }

    /**
     * Grows the cursor to its original size the animation duration set in the `constructor`.
     * @internal
     */
    GrowCursor(): void {
        if (!this.enabled) return;
        let newWidth = this.dotCursorElement.clientWidth;
        let newHeight = this.dotCursorElement.clientHeight;

        if (this.dotCursorElement.clientWidth < this.cursorStartSize[0]) {
            newWidth = this.dotCursorElement.clientWidth + this.animationSpeed[0];
        }

        if (this.dotCursorElement.clientHeight < this.cursorStartSize[1]) {
            newHeight = this.dotCursorElement.clientHeight + this.animationSpeed[1];
        }

        this.SetCursorSize(newWidth, newHeight, this.dotCursorElement);

        if (newWidth >= this.cursorStartSize[0] && newHeight >= this.cursorStartSize[1]) {
            clearInterval(this.currentAnimationInterval);

            this.SetCursorSize(this.cursorStartSize[0], this.cursorStartSize[1], this.dotCursorElement);

            this.currentAnimationInterval = -1;
            this.growQueued = false;
        }
    }

    private SetCursorSize(_newWidth: number, _newHeight: number, _cursorToChange: HTMLElement): void {
        const deltaX = Math.round((_cursorToChange.clientWidth - _newWidth) * 5) / 10;
        const deltaY = Math.round((_cursorToChange.clientHeight - _newHeight) * 5) / 10;
        const cursorPosX = _cursorToChange.offsetLeft + deltaX;
        const cursorPosY = _cursorToChange.offsetTop + deltaY;

        _cursorToChange.style.width = _newWidth + 'px';
        _cursorToChange.style.left = cursorPosX + 'px';

        _cursorToChange.style.height = _newHeight + 'px';
        _cursorToChange.style.top = cursorPosY + 'px';
    }

    /**
     * Make the cursor visible. Fades over time.
     */
     override ShowCursor(): void {
        this.shouldShow = true;
        if (!this.enabled) return;
        clearInterval(this.currentFadingInterval);
        this.currentFadingInterval = setInterval(
            this.FadeCursorIn.bind(this) as TimerHandler,
            this.animationUpdateDuration
        );
    }

    /**
     * Make the cursor invisible. Fades over time.
     */
     override HideCursor(): void {
        this.shouldShow = false;
        if (parseFloat(this.dotCursorElement.style.opacity) !== 0) {
            clearInterval(this.currentFadingInterval);
            this.currentFadingInterval = setInterval(
                this.FadeCursorOut.bind(this) as TimerHandler,
                this.animationUpdateDuration
            );
        }
    }

    private FadeCursorIn(): void {
        let currentOpacity = parseFloat(this.dotCursorElement.style.opacity);
        currentOpacity = currentOpacity ? currentOpacity : 0;
        currentOpacity += 0.05;

        this.dotCursorElement.style.opacity = currentOpacity.toString();

        if (currentOpacity >= 1) {
            clearInterval(this.currentFadingInterval);
            this.dotCursorElement.style.opacity = '1.0';
            this.currentFadingInterval = -1;
        }
    }

    private FadeCursorOut(): void {
        let currentOpacity = parseFloat(this.dotCursorElement.style.opacity);
        currentOpacity = currentOpacity ? currentOpacity : 1;
        currentOpacity -= 0.05;

        this.dotCursorElement.style.opacity = currentOpacity.toString();

        if (parseFloat(this.cursorRing.style.opacity) > 0) {
            this.cursorRing.style.opacity = currentOpacity.toString();
        }

        if (currentOpacity <= 0) {
            clearInterval(this.currentFadingInterval);
            this.dotCursorElement.style.opacity = '0.0';
            this.currentFadingInterval = -1;
        }
    }
}
