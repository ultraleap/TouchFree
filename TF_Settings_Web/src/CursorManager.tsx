import { SVGCursor } from 'TouchFree/src/Cursors/SvgCursor';
import { TouchlessCursor } from 'TouchFree/src/Cursors/TouchlessCursor';

export class CursorManager {
    public static cursor: TouchlessCursor;

    constructor() {
        CursorManager.cursor = new SVGCursor();
    }
}
