import { SVGCursor } from '../TouchFree/Cursors/SvgCursor';

export class CursorManager {
    constructor() {
        let svgDot = document.getElementById('svg-cursor');
        let svgRing = document.getElementById('svg-cursor-ring');

        const svgCursor = new SVGCursor(svgDot, svgRing, 'cx', 'cy');
    }
}
