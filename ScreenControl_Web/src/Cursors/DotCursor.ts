import {
    ClientInputAction,
    InputType
} from '../ScreenControlTypes';
import { TouchlessCursor } from './TouchlessCursor';
import { MapRangeToRange } from '../Utilities';

 //Class: DotCursor
 //This is an example Touchless Cursor which positions a dot on the screen at the hand location,
 //and reacts to the current ProgressToClick of the action (what determines this depends on the
 //currently active interaction).
export class DotCursor extends TouchlessCursor{
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

    // Group: Functions

    // Function: constructor
    // Constructs a new cursor consisting of a central cursor and a ring.
    // Optionally provide an _animationDuration to change the time it takes for the 'squeeze' confirmation animation to be performed.
    // Optionally provide a _ringSizeMultiplier to change the size that the <cursorRing> is relative to the _cursor.
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
    UpdateCursor(_inputAction: Array<number> | ClientInputAction): void{
        let inputAction = _inputAction as ClientInputAction;

        //progressToClick is between 0 and 1. Click triggered at progressToClick = 1
        let ringScaler = MapRangeToRange(inputAction.ProgressToClick, 0, 1, this.ringSizeMultiplier, 1);

        this.cursorRing.style.opacity = inputAction.ProgressToClick + "";

        this.cursorRing.style.width = this.cursor.clientWidth * ringScaler + "px";
        this.cursorRing.style.height = this.cursor.clientHeight * ringScaler + "px";

        this.cursorRing.style.left = (inputAction.CursorPosition[0] - (this.cursorRing.clientWidth / 2)) + "px";
        this.cursorRing.style.top = (window.innerHeight - (inputAction.CursorPosition[1] + (this.cursorRing.clientHeight / 2))) + "px";

        super.UpdateCursor(inputAction.CursorPosition);
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
                if (this.currentAnimationInterval != -1) {
                    clearInterval(this.currentAnimationInterval);
                }

                this.currentAnimationInterval = setInterval(
                    this.ShrinkCursor.bind(this) as TimerHandler,
                    this.animationUpdateDuration);
                break;
            case InputType.UP:
                if (this.currentAnimationInterval != -1) {
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
                break;
        }
    }

    // Function: ShrinkCursor
    // Shrinks the cursor to half of its original size over time set via the <constructor>.
    ShrinkCursor(): void {
        let cursorPosX = this.cursor.offsetLeft + (this.cursor.clientWidth / 2);
        let cursorPosY = this.cursor.offsetTop + (this.cursor.clientHeight / 2);

        if (this.cursor.clientWidth > this.cursorStartSize[0] / 2) {
            let newWidth = this.cursor.clientWidth - this.animationSpeed[0];

            this.cursor.style.width = newWidth + "px";
            this.cursor.style.left = (cursorPosX - (newWidth / 2)) + "px";
        }

        if (this.cursor.clientHeight > this.cursorStartSize[1] / 2) {
            let newHeight = this.cursor.clientHeight - this.animationSpeed[1];

            this.cursor.style.height = newHeight + "px";
            this.cursor.style.top = (cursorPosY - (newHeight / 2)) + "px";
        }

        if (this.cursor.offsetWidth <= this.cursorStartSize[0] / 2 &&
            this.cursor.offsetHeight <= this.cursorStartSize[1] / 2) {
            clearInterval(this.currentAnimationInterval);

            this.cursor.style.width = this.cursorStartSize[0] / 2 + "px";
            this.cursor.style.left = (cursorPosX - ((this.cursorStartSize[0] / 2) / 2)) + "px";
            this.cursor.style.height = this.cursorStartSize[1] / 2 + "px";
            this.cursor.style.top = (cursorPosY - ((this.cursorStartSize[1] / 2) / 2)) + "px";

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
        let cursorPosX = this.cursor.offsetLeft + (this.cursor.clientWidth / 2);
        let cursorPosY = this.cursor.offsetTop + (this.cursor.clientHeight / 2);

        if (this.cursor.clientWidth < this.cursorStartSize[0]) {
            let newWidth = this.cursor.clientWidth + this.animationSpeed[0];

            this.cursor.style.width = newWidth + "px";
            this.cursor.style.left = (cursorPosX - (newWidth / 2)) + "px";
        }

        if (this.cursor.clientHeight < this.cursorStartSize[1]) {
            let newHeight = this.cursor.clientHeight + this.animationSpeed[1];

            this.cursor.style.height = newHeight + "px";
            this.cursor.style.top = (cursorPosY - (newHeight / 2)) + "px";
        }

        if (this.cursor.offsetWidth >= this.cursorStartSize[0] &&
            this.cursor.offsetHeight >= this.cursorStartSize[1]) {
            clearInterval(this.currentAnimationInterval);

            this.cursor.style.width = this.cursorStartSize[0] + "px";
            this.cursor.style.left = (cursorPosX - (this.cursorStartSize[0] / 2)) + "px";
            this.cursor.style.height = this.cursorStartSize[1] + "px";
            this.cursor.style.top = (cursorPosY - (this.cursorStartSize[1] / 2)) + "px";

            this.currentAnimationInterval = -1;
            this.growQueued = false;
        }
    }
}