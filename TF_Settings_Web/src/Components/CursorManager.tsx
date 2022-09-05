import { SVGCursor } from '../TouchFree/Cursors/SvgCursor';
import { TouchlessCursor } from '../TouchFree/Cursors/TouchlessCursor';

export class CursorManager {
    public cursor: TouchlessCursor;

    public static instance: CursorManager;

    constructor() {
        this.cursor = new SVGCursor();

        CursorManager.instance = this;
    }
}