import TouchFree from '../TouchFree';
import { TouchFreeInputAction, InputType } from '../TouchFreeToolingTypes';
import { MapRangeToRange } from '../Utilities';
import { TouchlessCursor } from './TouchlessCursor';

// Class: DotCursor
// This is an example Touchless Cursor which positions a dot on the screen at the hand location,
// and reacts to the current ProgressToClick of the action (what determines this depends on the
// currently active interaction).
export class DotCursor extends TouchlessCursor {
    // Set the update rate of the animation to 30fps.
    readonly animationUpdateDuration: number = (1 / 30) * 1000;

    // Group: Variables

    // Variable: cursorRing
    // The HTMLElement that visually represents the cursors ring.
    cursorRing: HTMLElement;

    // Variable: ringSizeMultiplier
    // The maximum size for the ring to be relative to the size of the dot.
    //
    // e.g. a value of 2 means the ring can be (at largest) twice the scale of the dot.
    ringSizeMultiplier: number;

    private cursorStartSize: Array<number>;
    private animationSpeed: Array<number> = [0, 0];

    private currentAnimationInterval = -1;

    private growQueued = false;

    private currentFadingInterval = -1;

    private dotCursorElement: HTMLElement;

    // Group: Functions

    // Function: constructor
    // Constructs a new cursor consisting of a central cursor and a ring.
    // Optionally provide an animationDuration to change the time it takes for the 'squeeze'
    // confirmation animation to be performed. Optionally provide a ringSizeMultiplier to change
    // the size that the <cursorRing> is relative to the cursor.
    //
    // If you intend to make use of the <WebInputController>, make sure that both cursor and
    // cursorRing have the "touchfree-cursor" class. This prevents them blocking other elements
    // from receiving events.
    constructor(cursor: HTMLElement, cursorRing: HTMLElement, animationDuration = 0.2, ringSizeMultiplier = 2) {
        super(cursor);
        this.dotCursorElement = cursor;
        this.cursorRing = cursorRing;
        this.ringSizeMultiplier = ringSizeMultiplier;
        this.cursorStartSize = this.GetDimensions(this.dotCursorElement);

        this.animationSpeed[0] = this.cursorStartSize[0] / 2 / (animationDuration * 30);
        this.animationSpeed[1] = this.cursorStartSize[1] / 2 / (animationDuration * 30);

        TouchFree.RegisterEventCallback('HandFound', this.ShowCursor.bind(this));
        TouchFree.RegisterEventCallback('HandsLost', this.HideCursor.bind(this));
    }

    // Function: UpdateCursor
    // Used to update the cursor when recieving a "MOVE" <ClientInputAction>. Updates the
    // cursor's position, as well as the size of the ring based on the current ProgressToClick.
    protected UpdateCursor(inputAction: TouchFreeInputAction): void {
        if (!this.enabled) return;
        //progressToClick is between 0 and 1. Click triggered at progressToClick = 1
        const ringScaler = MapRangeToRange(inputAction.ProgressToClick, 0, 1, this.ringSizeMultiplier, 1);

        this.cursorRing.style.opacity = inputAction.ProgressToClick.toString();

        const [cursorWidth, cursorHeight] = this.GetDimensions(this.dotCursorElement);
        const [cursorRingWidth, cursorRingHeight] = this.GetDimensions(this.cursorRing);

        this.cursorRing.style.width = cursorWidth * ringScaler + 'px';
        this.cursorRing.style.height = cursorHeight * ringScaler + 'px';

        this.cursorRing.style.left = inputAction.CursorPosition[0] - cursorRingWidth / 2 + 'px';
        this.cursorRing.style.top = inputAction.CursorPosition[1] - cursorRingHeight / 2 + 'px';

        super.UpdateCursor(inputAction);
    }

    // Function: HandleInputAction
    // This override replaces the basic functionality of the <TouchlessCursor>, making the
    // cursor's ring scale dynamically with the current ProgressToClick and creating a
    // "shrink" animation when a "DOWN" event is received, and a "grow" animation when an "UP"
    // is received.
    //
    // When a "CANCEL" event is received, the cursor is hidden as it suggests the hand has been lost.
    // When any other event is received and the cursor is hidden, the cursor is shown again.
    protected HandleInputAction(inputData: TouchFreeInputAction): void {
        switch (inputData.InputType) {
            case InputType.MOVE:
                this.UpdateCursor(inputData);
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

    // Function: ShrinkCursor
    // Shrinks the cursor to half of its original size.
    // This is performed over a duration set in the <constructor>.
    ShrinkCursor(): void {
        if (!this.enabled) return;
        let [newWidth, newHeight] = this.GetDimensions(this.dotCursorElement);

        if (newWidth > this.cursorStartSize[0] / 2) {
            newWidth -= this.animationSpeed[0];
        }

        if (newHeight > this.cursorStartSize[1] / 2) {
            newHeight -= this.animationSpeed[1];
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

    // Function: GrowCursor
    // Grows the cursor to its original size over time set via the <constructor>.
    GrowCursor(): void {
        if (!this.enabled) return;
        let [newWidth, newHeight] = this.GetDimensions(this.dotCursorElement);

        if (newWidth < this.cursorStartSize[0]) {
            newWidth += this.animationSpeed[0];
        }

        if (newHeight < this.cursorStartSize[1]) {
            newHeight += this.animationSpeed[1];
        }

        this.SetCursorSize(newWidth, newHeight, this.dotCursorElement);

        if (newWidth >= this.cursorStartSize[0] && newHeight >= this.cursorStartSize[1]) {
            clearInterval(this.currentAnimationInterval);

            this.SetCursorSize(this.cursorStartSize[0], this.cursorStartSize[1], this.dotCursorElement);

            this.currentAnimationInterval = -1;
            this.growQueued = false;
        }
    }

    private SetCursorSize(newWidth: number, newHeight: number, cursorToChange: HTMLElement): void {
        const deltaX = Math.round((parseFloat(cursorToChange.style.width) - newWidth) * 5) / 10;
        const deltaY = Math.round((parseFloat(cursorToChange.style.height) - newHeight) * 5) / 10;
        const cursorPosX = cursorToChange.offsetLeft + deltaX;
        const cursorPosY = cursorToChange.offsetTop + deltaY;

        cursorToChange.style.width = newWidth + 'px';
        cursorToChange.style.left = cursorPosX + 'px';

        cursorToChange.style.height = newHeight + 'px';
        cursorToChange.style.top = cursorPosY + 'px';
    }

    // Function: ShowCursor
    // Used to make the cursor visible, fades over time
    ShowCursor(): void {
        this.shouldShow = true;
        if (!this.enabled) return;
        clearInterval(this.currentFadingInterval);

        this.currentFadingInterval = setInterval(
            this.FadeCursorIn.bind(this) as TimerHandler,
            this.animationUpdateDuration
        );
    }

    // Function: HideCursor
    // Used to make the cursor invisible, fades over time
    HideCursor(): void {
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
