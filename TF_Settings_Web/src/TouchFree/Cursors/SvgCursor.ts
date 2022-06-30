import { ConnectionManager } from "../Connection/ConnectionManager";
import { InputType } from "../TouchFreeToolingTypes";
import { MapRangeToRange } from "../Utilities";
import { TouchlessCursor } from "./TouchlessCursor";

export class SVGCursor extends TouchlessCursor {
    xPositionAttribute: string;
    yPositionAttribute: string;
    cursorCanvas: any;
    cursorRing: any;
    ringSizeMultiplier: number;
    cursorStartSize: number;
    currentAnimationInterval: NodeJS.Timeout | undefined = undefined;
    animationUpdateDuration: number | undefined;
    growQueued: boolean = false;
    hidingCursor: boolean = false;
    currentFadingInterval: NodeJS.Timeout | undefined = undefined;

    constructor(_cursorCanvas: any, _cursorDot: any, _cursorRing: any, _xPositionAttribute = "cx", _yPositionAttribute = "cx", _ringSizeMultiplier = 2, _darkCursor = false) {
        super(_cursorDot);

        this.cursorCanvas = _cursorCanvas;
        this.xPositionAttribute = _xPositionAttribute;
        this.yPositionAttribute = _yPositionAttribute;

        this.cursorRing = _cursorRing;
        this.ringSizeMultiplier = _ringSizeMultiplier;
        this.cursorStartSize = this.GetCurrentCursorRadius();

        if (!_darkCursor) {
            _cursorCanvas.classList.add('light');
        }

        ConnectionManager.instance.addEventListener('HandFound', this.ShowCursor.bind(this));
        ConnectionManager.instance.addEventListener('HandsLost', this.HideCursor.bind(this));
    }

    UpdateCursor(_inputAction: any) {
        let ringScaler = MapRangeToRange(_inputAction.ProgressToClick, 0, 1, this.ringSizeMultiplier, 1);

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
                this.cursor.classList.add('clicked');
                break;
            case InputType.UP:
                this.cursor.classList.remove('clicked');
                break;

            case InputType.CANCEL:
                break;
        }
    }

    SetCursorSize(_newWidth: any, _cursorToChange: any) {
        _cursorToChange.setAttribute('r', _newWidth);
    }

    ShowCursor() {
        this.hidingCursor = false;
        this.cursorCanvas.classList.remove('hidden');
    }

    HideCursor() {
        this.hidingCursor = true;
        this.cursorCanvas.classList.add('hidden');
        this.cursor.classList.remove('clicked');
    }

    GetCurrentCursorRadius(): number {
        const radius = this.cursor.getAttribute('r');
        if (!radius) {
            return 0;
        }

        let radiusAsNumber = parseFloat(radius);

        return radiusAsNumber;
    }
}
