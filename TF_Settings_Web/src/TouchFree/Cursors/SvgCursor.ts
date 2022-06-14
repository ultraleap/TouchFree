import { ConnectionManager } from "../Connection/ConnectionManager";
import { InputType } from "../TouchFreeToolingTypes";
import { TouchlessCursor } from "./TouchlessCursor";

export class SVGCursor extends TouchlessCursor {
    xPositionAttribute: string;
    yPositionAttribute: string;
    cursorRing: any;
    ringSizeMultiplier: number;
    cursorStartSize: number;
    animationSpeed: number;
    currentAnimationInterval: NodeJS.Timeout | undefined = undefined;
    animationUpdateDuration: number | undefined;
    growQueued: boolean = false;
    hidingCursor: boolean = false;
    currentFadingInterval: NodeJS.Timeout | undefined = undefined;

    constructor(_cursor: any, _cursorRing: any, _xPositionAttribute = "cx", _yPositionAttribute = "cx", _animationDuration = 0.2, _ringSizeMultiplier = 2) {
        super(_cursor);

        this.xPositionAttribute = _xPositionAttribute;
        this.yPositionAttribute = _yPositionAttribute;

        this.cursorRing = _cursorRing;
        this.ringSizeMultiplier = _ringSizeMultiplier;
        this.cursorStartSize = this.GetCurrentCursorRadius();

        this.animationSpeed = (this.cursorStartSize / 2) / (_animationDuration * 30);

        ConnectionManager.instance.addEventListener('HandFound', this.ShowCursor.bind(this));
        ConnectionManager.instance.addEventListener('HandsLost', this.HideCursor.bind(this));
    }

    UpdateCursor(_inputAction: any) {
        let ringScaler = this.MapRangeToRange(_inputAction.ProgressToClick, 0, 1, this.ringSizeMultiplier, 1);

        this.cursorRing.setAttribute('opacity', _inputAction.ProgressToClick);

        this.cursorRing.setAttribute('r', this.GetCurrentCursorRadius() * ringScaler);

        this.cursorRing.setAttribute(this.xPositionAttribute, _inputAction.CursorPosition[0]);
        this.cursorRing.setAttribute(this.yPositionAttribute, _inputAction.CursorPosition[1]);

        this.cursor.setAttribute(this.xPositionAttribute, _inputAction.CursorPosition[0]);
        this.cursor.setAttribute(this.yPositionAttribute, _inputAction.CursorPosition[1]);
    }

    HandleInputAction(_inputData: any) {
        switch (_inputData.InputType) {
            case InputType.MOVE:
                this.UpdateCursor(_inputData);
                break;
            case InputType.DOWN:
                this.SetCursorSize(0, this.cursorRing);

                if (this.currentAnimationInterval !== undefined) {
                    clearInterval(this.currentAnimationInterval);
                }

                this.currentAnimationInterval = setInterval(
                    this.ShrinkCursor.bind(this),
                    this.animationUpdateDuration);
                break;
            case InputType.UP:
                if (this.currentAnimationInterval !== undefined) {
                    this.growQueued = true;
                }
                else {
                    this.growQueued = false;
                    this.currentAnimationInterval = setInterval(
                        this.GrowCursor.bind(this),
                        this.animationUpdateDuration);
                }
                break;

            case InputType.CANCEL:
                break;
        }
    }

    ShrinkCursor() {
        let newWidth = this.GetCurrentCursorRadius();

        if (newWidth > this.cursorStartSize / 2) {
            newWidth = newWidth - this.animationSpeed;
        }

        if (newWidth > this.cursorStartSize / 2) {
            this.SetCursorSize(newWidth, this.cursor);
        } else {
            if (this.currentAnimationInterval !== undefined) {
                clearInterval(this.currentAnimationInterval);
            }

            newWidth = this.cursorStartSize / 2;

            this.SetCursorSize(newWidth, this.cursor);

            if (this.growQueued) {
                this.growQueued = false;
                this.currentAnimationInterval = setInterval(
                    this.GrowCursor.bind(this),
                    this.animationUpdateDuration);
            } else {
                this.currentAnimationInterval = undefined;
            }
        }
    }

    GrowCursor() {
        let newWidth = this.GetCurrentCursorRadius();

        if (newWidth < this.cursorStartSize) {
            newWidth = newWidth + this.animationSpeed;
        }

        if (newWidth < this.cursorStartSize) {
            this.SetCursorSize(newWidth, this.cursor);
        } else {
            if (this.currentAnimationInterval !== undefined) {
                clearInterval(this.currentAnimationInterval);
            }

            this.SetCursorSize(this.cursorStartSize, this.cursor);

            this.currentAnimationInterval = undefined;
            this.growQueued = false;
        }
    }

    SetCursorSize(_newWidth: any, _cursorToChange: any) {
        _cursorToChange.setAttribute('r', _newWidth);
    }

    ShowCursor() {
        this.hidingCursor = false;
        if (this.currentFadingInterval !== undefined) {
            clearInterval(this.currentFadingInterval);
        }
        this.currentFadingInterval = setInterval(
            this.FadeCursorIn.bind(this),
            this.animationUpdateDuration);
    }

    HideCursor() {
        this.hidingCursor = true;
        if (this.currentFadingInterval !== undefined) {
            clearInterval(this.currentFadingInterval);
        }
        this.currentFadingInterval = setInterval(
            this.FadeCursorOut.bind(this),
            this.animationUpdateDuration);
    }

    FadeCursorIn() {
        let currentOpacity = this.GetCurrentCursorOpacity(0);
        currentOpacity += 0.05;

        if (currentOpacity < 1) {
            this.cursor.setAttribute('opacity', currentOpacity.toString());
        } else {
            if (this.currentFadingInterval !== undefined) {
                clearInterval(this.currentFadingInterval);
            }
            this.cursor.setAttribute('opacity', '1');
            this.currentFadingInterval = undefined;
        }
    }

    FadeCursorOut() {
        let currentOpacity = this.GetCurrentCursorOpacity(1);
        currentOpacity -= 0.05;


        if (currentOpacity > 0) {
            this.cursor.setAttribute('opacity', currentOpacity.toString());
        } else {
            if (this.currentFadingInterval !== undefined) {
                clearInterval(this.currentFadingInterval);
            }
            this.cursor.setAttribute('opacity', '0');
            this.currentFadingInterval = undefined;
        }
    }

    MapRangeToRange(_value: number, _oldMin: number, _oldMax: number, _newMin: number, _newMax: number): number {
        let oldRange = (_oldMax - _oldMin);
        let newValue;
        if (oldRange === 0) {
            newValue = _newMin;
        }
        else {
            let newRange = (_newMax - _newMin);
            newValue = (((_value - _oldMin) * newRange) / oldRange) + _newMin;
        }
        return newValue;
    }

    GetCurrentCursorRadius(): number {
        const radius = this.cursor.getAttribute('r');
        if (!radius) {
            return 0;
        }

        let radiusAsNumber = parseFloat(radius);

        return radiusAsNumber;
    }

    GetCurrentCursorOpacity(defaultValue: number): number {
        const opacity = this.cursor.getAttribute('opacity');
        if (!opacity) {
            return defaultValue;
        }

        let opacityAsNumber = parseFloat(opacity);

        return opacityAsNumber;
    }
}
