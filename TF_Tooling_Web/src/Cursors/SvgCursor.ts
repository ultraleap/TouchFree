import { ConnectionManager } from '../Connection/ConnectionManager';
import { InputType, TouchFreeInputAction } from '../TouchFreeToolingTypes';
import { MapRangeToRange } from '../Utilities';
import { TouchlessCursor } from './TouchlessCursor';

export class SVGCursor extends TouchlessCursor {
    private xPositionAttribute = 'cx';
    private yPositionAttribute = 'cy';
    private cursorCanvas: SVGSVGElement;
    private cursorRing: SVGCircleElement;
    private ringSizeMultiplier: number;

    private cursorShowing = false;

    // Group: Functions

    // Function: constructor
    // Constructs a new cursor consisting of a central cursor and a ring.
    // Optionally provide a ringSizeMultiplier to change the size that the <cursorRing> is relative to the _cursor.
    // Optionally provide a darkCursor to change the cursor to be dark to provide better contrast on light coloured
    // UIs.
    constructor(ringSizeMultiplier = 2, darkCursor = false) {
        super(undefined);

        const documentBody = document.querySelector('body');

        const svgElement = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        svgElement.classList.add('touchfree-cursor');
        svgElement.style.opacity = '0';
        svgElement.style.position = 'fixed';
        svgElement.style.top = '0px';
        svgElement.style.left = '0px';
        svgElement.style.zIndex = '1000';
        svgElement.style.pointerEvents = 'none';
        svgElement.setAttribute('width', '100%');
        svgElement.setAttribute('height', '100%');
        svgElement.setAttribute('shape-rendering', 'optimizeSpeed');
        svgElement.id = 'svg-cursor';
        documentBody?.appendChild(svgElement);

        const svgRingElement = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        svgRingElement.classList.add('touchfree-cursor');
        svgRingElement.setAttribute('r', '15');
        svgRingElement.setAttribute('fill-opacity', '0');
        svgRingElement.setAttribute('stroke-width', '5');
        svgRingElement.setAttribute('stroke', darkCursor ? 'black' : 'white');
        svgRingElement.setAttribute(this.xPositionAttribute, '100');
        svgRingElement.setAttribute(this.yPositionAttribute, '100');
        svgRingElement.style.filter = 'drop-shadow(0 0 10px rgba(255, 255, 255, 0.7))';
        svgElement.appendChild(svgRingElement);
        svgRingElement.id = 'svg-cursor-ring';
        this.cursorRing = svgRingElement;

        const svgDotElement = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        svgDotElement.classList.add('touchfree-cursor');
        svgDotElement.setAttribute('r', '15');
        svgDotElement.setAttribute('fill', darkCursor ? 'black' : 'white');
        svgDotElement.setAttribute(this.xPositionAttribute, '100');
        svgDotElement.setAttribute(this.yPositionAttribute, '100');
        svgDotElement.setAttribute('opacity', '1');
        svgDotElement.style.transformBox = 'fill-box';
        svgDotElement.style.transformOrigin = 'center';
        svgDotElement.style.transform = 'scale(1)';
        svgDotElement.style.filter = 'drop-shadow(0 0 10px rgba(255, 255, 255, 0.7))';
        svgDotElement.id = 'svg-cursor-dot';
        svgElement.appendChild(svgDotElement);

        if (!darkCursor) {
            if (this.cursorRing) {
                this.cursorRing.style.filter = 'drop-shadow(0 0 10px rgba(0, 0, 0, 0.7))';
            }
            svgDotElement.style.filter = 'drop-shadow(0 0 10px rgba(0, 0, 0, 0.7))';
        }

        this.cursor = svgDotElement;

        this.cursorCanvas = svgElement;

        this.ringSizeMultiplier = ringSizeMultiplier;

        ConnectionManager.instance.addEventListener('HandFound', this.ShowCursor.bind(this));
        ConnectionManager.instance.addEventListener('HandsLost', this.HideCursor.bind(this));
    }

    // Function: UpdateCursor
    // Used to update the cursor when receiving a "MOVE" <ClientInputAction>. Updates the
    // cursor's position, as well as the size of the ring based on the current ProgressToClick.
    protected UpdateCursor(inputAction: TouchFreeInputAction) {
        if (!this.shouldShow) {
            this.HideCursor();
            return;
        }
        const ringScaler = MapRangeToRange(inputAction.ProgressToClick, 0, 1, this.ringSizeMultiplier, 1);

        this.cursorRing.setAttribute('opacity', inputAction.ProgressToClick.toString());
        this.cursorRing.setAttribute('r', Math.round(this.GetCurrentCursorRadius() * ringScaler).toString());

        let position = inputAction.CursorPosition;

        if (position) {
            position = [Math.round(position[0]), Math.round(position[1])];
            if (!this.cursorShowing && this.enabled) {
                this.ShowCursor();
            }
            this.cursorRing.setAttribute(this.xPositionAttribute, position[0].toString());
            this.cursorRing.setAttribute(this.yPositionAttribute, position[1].toString());

            if (this.cursor) {
                this.cursor.setAttribute(this.xPositionAttribute, position[0].toString());
                this.cursor.setAttribute(this.yPositionAttribute, position[1].toString());
            }
        } else {
            this.HideCursor();
        }
    }

    // Function: HandleInputAction
    // This override replaces the basic functionality of the <TouchlessCursor>, making the
    // cursor's ring scale dynamically with the current ProgressToClick and creating a
    // "shrink" animation when a "DOWN" event is received, and a "grow" animation when an "UP"
    // is received.
    //
    // When a "CANCEL" event is received, the cursor is hidden as it suggests the hand has been lost.
    // When any other event is received and the cursor is hidden, the cursor is shown again.
    protected HandleInputAction(inputData: TouchFreeInputAction) {
        if (this.cursor) {
            switch (inputData.InputType) {
                case InputType.MOVE:
                    this.UpdateCursor(inputData);
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

    private SetCursorSize(newWidth: number, cursorToChange: SVGElement) {
        cursorToChange?.setAttribute('r', Math.round(newWidth).toString());
    }

    // Function: ShowCursor
    // Used to make the cursor visible, fades over time
    ShowCursor() {
        this.shouldShow = true;
        if (this.enabled && !this.cursorShowing) {
            this.cursorShowing = true;
            this.SetCursorOpacity(this.opacityOnHandsLost);
        }
    }

    // Function: HideCursor
    // Used to make the cursor invisible, fades over time
    HideCursor() {
        if (this.shouldShow) {
            this.opacityOnHandsLost = Number(this.cursorCanvas.style.opacity);
        }
        this.shouldShow = false;
        this.cursorShowing = false;
        this.SetCursorOpacity(0);
        if (this.cursor) {
            this.cursor.style.transform = 'scale(1)';
        }
    }

    // Function: SetCursorOpacity
    // Used to set the opacity of the cursor
    SetCursorOpacity(opacity: number): void {
        this.cursorCanvas.style.opacity = opacity.toString();
    }

    private GetCurrentCursorRadius(): number {
        if (this.cursor) {
            const radius = this.cursor.getAttribute('r');
            if (!radius) {
                return 0;
            }

            const radiusAsNumber = parseFloat(radius);

            return radiusAsNumber;
        }
        return 0;
    }
}
