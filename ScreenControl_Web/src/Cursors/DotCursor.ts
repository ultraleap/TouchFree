import {
    ClientInputAction,
    InputType
} from '../ScreenControlTypes';
import { TouchlessCursor } from './TouchlessCursor';
import { MapRangeToRange } from '../Utilities';

// Class: DotCursor
// This is an example Touchless Cursor which positions a dot on the screen at the hand location,
// and reacts to the current ProgressToClick of the action (what determines this depends on the
// currently active interaction).
export class DotCursor extends TouchlessCursor{

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
    private animationSpeed: Array<number> = [0,0];

    private currentAnimationInterval: number = -1;

    private growQueued: boolean = false;

    private hidingCursor: boolean = true;
    private currentFadingInterval: number = -1;

    // Group: Functions

    // Function: constructor
    // Constructs a new cursor consisting of a central cursor and a ring.
    // Optionally provide an _animationDuration to change the time it takes for the 'squeeze'
    // confirmation animation to be performed. Optionally provide a _ringSizeMultiplier to change
    // the size that the <cursorRing> is relative to the _cursor.
    //
    // If you intend to make use of the <WebInputController>, make sure that both _cursor and
    // _cursorRing have the "screencontrolcursor" class. This prevents them blocking other elements
    // from recieving events.
    constructor(_cursor: HTMLElement, _cursorRing: HTMLElement, _animationDuration: number = 0.2, _ringSizeMultiplier: number = 2) {
        super(_cursor);
        this.cursorRing = _cursorRing;
        this.ringSizeMultiplier = _ringSizeMultiplier;
        this.cursorStartSize = [_cursor.clientWidth.valueOf(), _cursor.clientHeight.valueOf()];

        this.animationSpeed[0] = (this.cursorStartSize[0] / 2) / (_animationDuration * 30);
        this.animationSpeed[1] = (this.cursorStartSize[1] / 2) / (_animationDuration * 30);
    }

    // Function: UpdateCursor
    // Used to update the cursor when recieving a "MOVE" <ClientInputAction>. Updates the
    // cursor's position, as well as the size of the ring based on the current ProgressToClick.
    UpdateCursor(_inputAction: ClientInputAction): void{
        //progressToClick is between 0 and 1. Click triggered at progressToClick = 1
        let ringScaler = MapRangeToRange(_inputAction.ProgressToClick, 0, 1, this.ringSizeMultiplier, 1);

        this.cursorRing.style.opacity = _inputAction.ProgressToClick + "";

        this.cursorRing.style.width = this.cursor.clientWidth * ringScaler + "px";
        this.cursorRing.style.height = this.cursor.clientHeight * ringScaler + "px";

        this.cursorRing.style.left = (_inputAction.CursorPosition[0] - (this.cursorRing.clientWidth / 2)) + "px";
        this.cursorRing.style.top = (window.innerHeight - (_inputAction.CursorPosition[1] + (this.cursorRing.clientHeight / 2))) + "px";

        super.UpdateCursor(_inputAction);
    }

    // Function: HandleInputAction
    // This override replaces the basic functionality of the <TouchlessCursor>, making the
    // cursor's ring scale dynamically with the current ProgressToClick and creating a
    // "shrink" animation when a "DOWN" event is recieved, and a "grow" animation when an "UP"
    // is recieved.
    HandleInputAction(_inputData: ClientInputAction): void {
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
                    this.animationUpdateDuration);
                break;
            case InputType.UP:
                if (this.currentAnimationInterval !== -1) {
                    this.growQueued = true;
                }
                else {
                    this.growQueued = false;
                    this.currentAnimationInterval = setInterval(
                        this.GrowCursor.bind(this) as TimerHandler,
                        this.animationUpdateDuration);
                }
                break;

            case InputType.CANCEL:
                this.HideCursor();
                break;
        }

        if (this.hidingCursor && _inputData.InputType !== InputType.CANCEL) {
            this.ShowCursor();
        }
    }

    // Function: ShrinkCursor
    // Shrinks the cursor to half of its original size.
    // This is performed over a duration set in the <constructor>.
    ShrinkCursor(): void {
        let newWidth = this.cursor.clientWidth;
        let newHeight = this.cursor.clientHeight;

        if (this.cursor.clientWidth > this.cursorStartSize[0] / 2) {
            newWidth = this.cursor.clientWidth - this.animationSpeed[0];
        }

        if (this.cursor.clientHeight > this.cursorStartSize[1] / 2) {
            newHeight = this.cursor.clientHeight - this.animationSpeed[1];
        }

        this.SetCursorSize(newWidth, newHeight, this.cursor);

        if (newWidth <= this.cursorStartSize[0] / 2 && newHeight <= this.cursorStartSize[1] / 2) {
            clearInterval(this.currentAnimationInterval);

            newWidth = this.cursorStartSize[0] / 2;
            newHeight = this.cursorStartSize[1] / 2;

            this.SetCursorSize(newWidth, newHeight, this.cursor);

            if (this.growQueued) {
                this.growQueued = false;
                this.currentAnimationInterval = setInterval(
                    this.GrowCursor.bind(this) as TimerHandler,
                    this.animationUpdateDuration);
            }
            else {
                this.currentAnimationInterval = -1;
            }
        }
    }

    // Function: GrowCursor
    // Grows the cursor to its original size over time set via the <constructor>.
    GrowCursor(): void {
        let newWidth = this.cursor.clientWidth;
        let newHeight = this.cursor.clientHeight;

        if (this.cursor.clientWidth < this.cursorStartSize[0]) {
            newWidth = this.cursor.clientWidth + this.animationSpeed[0];
        }

        if (this.cursor.clientHeight < this.cursorStartSize[1]) {
            newHeight = this.cursor.clientHeight + this.animationSpeed[1];
        }

        this.SetCursorSize(newWidth, newHeight, this.cursor);

        if (newWidth >= this.cursorStartSize[0] && newHeight >= this.cursorStartSize[1]) {
            clearInterval(this.currentAnimationInterval);

            this.SetCursorSize(this.cursorStartSize[0], this.cursorStartSize[1], this.cursor);

            this.currentAnimationInterval = -1;
            this.growQueued = false;
        }
    }

    private SetCursorSize(_newWidth: number, _newHeight: number, _cursorToChange: HTMLElement): void {
        let cursorPosX = _cursorToChange.offsetLeft + (_cursorToChange.clientWidth / 2);
        let cursorPosY = _cursorToChange.offsetTop + (_cursorToChange.clientHeight / 2);

        _cursorToChange.style.width = _newWidth + "px";
        _cursorToChange.style.left = (cursorPosX - (_newWidth / 2)) + "px";

        _cursorToChange.style.height = _newHeight + "px";
        _cursorToChange.style.top = (cursorPosY - (_newHeight / 2)) + "px";
    }

    // Function: ShowCursor
    // Used to make the cursor visible, fades over time
    ShowCursor(): void {
        this.hidingCursor = false;
        this.currentFadingInterval = setInterval(
            this.FadeCursorIn.bind(this) as TimerHandler,
            this.animationUpdateDuration);
    }

    // Function: HideCursor
    // Used to make the cursor invisible, fades over time
    HideCursor(): void {
        this.hidingCursor = true;
        this.currentFadingInterval = setInterval(
            this.FadeCursorOut.bind(this) as TimerHandler,
            this.animationUpdateDuration);
    }

    private FadeCursorIn(): void {
        let currentOpacity = parseFloat(this.cursor.style.opacity);
        currentOpacity = currentOpacity ? currentOpacity : 0;
        currentOpacity += 0.05;

        this.cursor.style.opacity = currentOpacity.toString();
        this.cursorRing.style.opacity = currentOpacity.toString();

        if (currentOpacity >= 1) {
            clearInterval(this.currentFadingInterval);
            this.cursor.style.opacity = "1.0";
            this.currentFadingInterval = -1;
        }
    }

    private FadeCursorOut(): void {
        let currentOpacity = parseFloat(this.cursor.style.opacity);
        currentOpacity = currentOpacity ? currentOpacity : 1;
        currentOpacity -= 0.05;

        this.cursor.style.opacity = currentOpacity.toString();
        this.cursorRing.style.opacity = currentOpacity.toString();

        if (currentOpacity <= 0) {
            clearInterval(this.currentFadingInterval);
            this.cursor.style.opacity = "0.0";
            this.currentFadingInterval = -1;
        }
    }
}