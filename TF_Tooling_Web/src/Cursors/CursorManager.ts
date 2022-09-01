import { SVGCursor } from './SvgCursor';
import { TouchlessCursor } from './TouchlessCursor';

export class CursorManager {
    public cursor: TouchlessCursor;

    public static instance: CursorManager;

    constructor() {
        this.cursor = new SVGCursor();

        CursorManager.instance = this;
    }
}
