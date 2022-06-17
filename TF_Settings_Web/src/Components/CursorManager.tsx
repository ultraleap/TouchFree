import { SVGCursor } from '../TouchFree/Cursors/SvgCursor';

export class CursorManager {
    constructor() {
        let svgCanvas = document.getElementById('svg-cursor');
        let svgDot = document.getElementById('svg-cursor-dot');
        let svgRing = document.getElementById('svg-cursor-ring');

        const svgCursor = new SVGCursor(svgCanvas, svgDot, svgRing, 'cx', 'cy');
    }
}
