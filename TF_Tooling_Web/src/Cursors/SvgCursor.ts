import { ConnectionManager } from '../Connection/ConnectionManager';
import { InputType, TouchFreeInputAction } from '../TouchFreeToolingTypes';
import { MapRangeToRange } from '../Utilities';
import { TouchlessCursor } from './TouchlessCursor';

/**
 * {@link TouchlessCursor} created with SVG elements.
 * @public
 */
export class SVGCursor extends TouchlessCursor {
    private xPositionAttribute = 'cx';
    private yPositionAttribute = 'cy';
    private cursorCanvas: SVGSVGElement;
    private cursorRing: SVGCircleElement;
    private ringSizeMultiplier: number;

    private cursorShowing = false;

    /**
     * Constructs a new cursor consisting of a central cursor and a ring.
     * @param _ringSizeMultiplier - Optional multiplier to change the size of the cursor ring.
     * @param _darkCursor - Optionally darken the cursor to provide better contrast on light coloured UIs.
     */
    constructor(_ringSizeMultiplier = 2, _darkCursor = false) {
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
        svgRingElement.setAttribute('stroke', _darkCursor ? 'black' : 'white');
        svgRingElement.setAttribute(this.xPositionAttribute, '100');
        svgRingElement.setAttribute(this.yPositionAttribute, '100');
        svgRingElement.style.filter = 'drop-shadow(0 0 10px rgba(255, 255, 255, 0.7))';
        svgElement.appendChild(svgRingElement);
        this.cursorRing = svgRingElement;

        const svgDotElement = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        svgDotElement.classList.add('touchfree-cursor');
        svgDotElement.setAttribute('r', '15');
        svgDotElement.setAttribute('fill', _darkCursor ? 'black' : 'white');
        svgDotElement.setAttribute(this.xPositionAttribute, '100');
        svgDotElement.setAttribute(this.yPositionAttribute, '100');
        svgDotElement.setAttribute('opacity', '1');
        svgDotElement.style.transformBox = 'fill-box';
        svgDotElement.style.transformOrigin = 'center';
        svgDotElement.style.transform = 'scale(1)';
        svgDotElement.style.filter = 'drop-shadow(0 0 10px rgba(255, 255, 255, 0.7))';
        svgElement.appendChild(svgDotElement);

        if (!_darkCursor) {
            if (this.cursorRing) {
                this.cursorRing.style.filter = 'drop-shadow(0 0 10px rgba(0, 0, 0, 0.7))';
            }
            svgDotElement.style.filter = 'drop-shadow(0 0 10px rgba(0, 0, 0, 0.7))';
        }

        this.cursor = svgDotElement;

        this.cursorCanvas = svgElement;

        this.ringSizeMultiplier = _ringSizeMultiplier;

        ConnectionManager.instance.addEventListener('HandFound', this.ShowCursor.bind(this));
        ConnectionManager.instance.addEventListener('HandsLost', this.HideCursor.bind(this));
    }

    /**
     * Update the cursor position as well as the size of the ring based on {@link TouchFreeInputAction.ProgressToClick}.
     * @param _inputAction - Input action to use when updating cursor
     * @internal
     */
     override UpdateCursor(_inputAction: TouchFreeInputAction) {
        if (!this.shouldShow) {
            this.HideCursor();
            return;
        }
        const ringScaler = MapRangeToRange(_inputAction.ProgressToClick, 0, 1, this.ringSizeMultiplier, 1);

        this.cursorRing.setAttribute('opacity', _inputAction.ProgressToClick.toString());
        this.cursorRing.setAttribute('r', Math.round(this.GetCurrentCursorRadius() * ringScaler).toString());

        let position = _inputAction.CursorPosition;

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
     override HandleInputAction(_inputData: TouchFreeInputAction) {
        if (this.cursor) {
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

    private SetCursorSize(_newWidth: number, _cursorToChange: SVGElement) {
        _cursorToChange?.setAttribute('r', Math.round(_newWidth).toString());
    }

    /**
     * Make the cursor visible. Fades over time.
     */
     override ShowCursor() {
        this.shouldShow = true;
        if (this.enabled && !this.cursorShowing) {
            this.cursorShowing = true;
            this.cursorCanvas.style.opacity = '1';
        }
    }

    /**
     * Make the cursor invisible. Fades over time.
     */
     override HideCursor() {
        this.shouldShow = false;
        this.cursorShowing = false;
        this.cursorCanvas.style.opacity = '0';
        if (this.cursor) {
            this.cursor.style.transform = 'scale(1)';
        }
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
