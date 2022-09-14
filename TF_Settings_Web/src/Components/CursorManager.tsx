import { SVGCursor } from 'TouchFree/Cursors/SvgCursor';
import { TouchlessCursor } from 'TouchFree/Cursors/TouchlessCursor';

export class CursorManager {
    public static cursor: TouchlessCursor;

    constructor() {
        CursorManager.cursor = new SVGCursor();
    }
}
