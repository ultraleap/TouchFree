import { SVGCursor } from 'TouchFree/Cursors/SvgCursor';
import { TouchlessCursor } from 'TouchFree/Cursors/TouchlessCursor';

export class CursorManager {
    public cursor: TouchlessCursor;

    constructor() {
        const svgCanvas = document.querySelector('#svg-cursor');
        const svgDot = document.querySelector('#svg-cursor-dot');
        const svgRing = document.querySelector('#svg-cursor-ring');

        this.cursor = new SVGCursor(svgCanvas, svgDot, svgRing, 'cx', 'cy');
    }
}
