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

    constructor(_xPositionAttribute = "cx", _yPositionAttribute = "cy", _ringSizeMultiplier = 2, _darkCursor = false) {
        super(undefined);

        const documentBody = document.querySelector('body');
        
        const svgElement = document.createElementNS('http://www.w3.org/2000/svg','svg');
        svgElement.classList.add('touchfreecursor');
        svgElement.style.opacity = '0';
        svgElement.style.position = 'fixed';
        svgElement.style.top = '0px';
        svgElement.style.left = '0px';
        svgElement.style.zIndex = '1000';
        svgElement.setAttribute('width', '100%');
        svgElement.setAttribute('height', '100%');
        svgElement.id = 'svg-cursor';
        documentBody?.appendChild(svgElement);
        
        const svgRingElement = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        svgRingElement.classList.add('touchfreecursor');
        svgRingElement.setAttribute('r', '15');
        svgRingElement.setAttribute('fill-opacity', '0');
        svgRingElement.setAttribute('stroke-width', '5');
        svgRingElement.setAttribute('stroke', _darkCursor ? 'black' : 'white');
        svgRingElement.setAttribute('cx', '100');
        svgRingElement.setAttribute('cy', '100');
        svgRingElement.style.filter = 'drop-shadow(0 0 10px rgba(255, 255, 255, 0.7))';
        svgElement.appendChild(svgRingElement);
        this.cursorRing = svgRingElement;
        
        const svgDotElement = document.createElementNS('http://www.w3.org/2000/svg','circle');
        svgDotElement.classList.add('touchfreecursor');
        svgDotElement.setAttribute('r', '15');
        svgDotElement.setAttribute('fill', _darkCursor ? 'black' : 'white');
        svgDotElement.setAttribute('cx', '100');
        svgDotElement.setAttribute('cy', '100');
        svgDotElement.setAttribute('opacity', '1');
        svgDotElement.style.transition = 'transform 200ms, opacity 666ms';
        svgDotElement.style.transformBox = 'fill-box';
        svgDotElement.style.transformOrigin = 'center';
        svgDotElement.style.transform = 'scale(1)';
        svgDotElement.style.filter = 'drop-shadow(0 0 10px rgba(255, 255, 255, 0.7))';
        svgElement.appendChild(svgDotElement);

        if (!_darkCursor) {
            if (this.cursorRing !== undefined) {this.cursorRing.style.filter = 'drop-shadow(0 0 10px rgba(0, 0, 0, 0.7))';}
            svgDotElement.style.filter = 'drop-shadow(0 0 10px rgba(0, 0, 0, 0.7))';
        }

        this.cursor = svgDotElement;

        this.cursorCanvas = svgElement;
        this.xPositionAttribute = _xPositionAttribute;
        this.yPositionAttribute = _yPositionAttribute;

        this.ringSizeMultiplier = _ringSizeMultiplier;
        this.cursorStartSize = this.GetCurrentCursorRadius();

        ConnectionManager.instance.addEventListener('HandFound', this.ShowCursor.bind(this));
        ConnectionManager.instance.addEventListener('HandsLost', this.HideCursor.bind(this));
    }

    UpdateCursor(_inputAction: any) {
        let ringScaler = MapRangeToRange(_inputAction.ProgressToClick, 0, 1, this.ringSizeMultiplier, 1);

        this.cursorRing?.setAttribute('opacity', _inputAction.ProgressToClick);
        this.cursorRing?.setAttribute('r', this.GetCurrentCursorRadius() * ringScaler);

        const position = _inputAction.CursorPosition;

        if (position) {
            this.ShowCursor();
            this.cursorRing?.setAttribute(this.xPositionAttribute, position[0]);
            this.cursorRing?.setAttribute(this.yPositionAttribute, position[1]);
    
            if (this.cursor !== undefined) {
                this.cursor.setAttribute(this.xPositionAttribute, position[0].toString());
                this.cursor.setAttribute(this.yPositionAttribute, position[1].toString());
            }
        } else {
            this.HideCursor();
        }
    }

    HandleInputAction(_inputData: any) {
        if (this.cursor !== undefined) {
            switch (_inputData.InputType) {
                case InputType.MOVE:
                    this.UpdateCursor(_inputData);
                    break;
                case InputType.DOWN:
                    this.SetCursorSize(0, this.cursorRing);
                    this.cursor.style.transform = 'scale(0.5)';
                    break;
                case InputType.UP:
                    this.cursor.style.transform = 'scale(1)';
                    break;

                case InputType.CANCEL:
                    break;
            }
        }
    }

    SetCursorSize(_newWidth: any, _cursorToChange: any) {
        _cursorToChange?.setAttribute('r', _newWidth);
    }

    ShowCursor() {
        this.hidingCursor = false;
        this.cursorCanvas.style.opacity = '1';
    }

    HideCursor() {
        this.hidingCursor = true;
        this.cursorCanvas.style.opacity = '0';
        if (this.cursor !== undefined) {
            this.cursor.style.transform = 'scale(1)';
        }
    }

    GetCurrentCursorRadius(): number {
        if (this.cursor !== undefined) {
            const radius = this.cursor.getAttribute('r');
            if (!radius) {
                return 0;
            }

            let radiusAsNumber = parseFloat(radius);

            return radiusAsNumber;
        }
        return 0;
    }
}