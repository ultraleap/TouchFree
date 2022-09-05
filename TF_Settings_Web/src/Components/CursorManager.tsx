import { SVGCursor } from 'TouchFree/Cursors/SvgCursor';
import { TouchlessCursor } from 'TouchFree/Cursors/TouchlessCursor';

export class CursorManager {
    public cursor: TouchlessCursor;

    constructor() {
        this.cursor = new SVGCursor();
    }
}
