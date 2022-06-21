import { SVGCursor } from '../TouchFree/Cursors/SvgCursor';
import { TouchlessCursor } from '../TouchFree/Cursors/TouchlessCursor';

export class CursorManager {
    public cursor: TouchlessCursor;

    constructor() {
        let svgCanvas = document.getElementById('svg-cursor');
        let svgDot = document.getElementById('svg-cursor-dot');
        let svgRing = document.getElementById('svg-cursor-ring');

        this.cursor = new SVGCursor(svgCanvas, svgDot, svgRing, 'cx', 'cy');
    }
}
