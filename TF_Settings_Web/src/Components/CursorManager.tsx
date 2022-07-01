import { SVGCursor } from '../TouchFree/Cursors/SvgCursor';
import { TouchlessCursor } from '../TouchFree/Cursors/TouchlessCursor';

export class CursorManager {
    public cursor: TouchlessCursor;

    constructor() {
        this.cursor = new SVGCursor(undefined, (inputAction: any) => inputAction.CursorPosition, 'cx', 'cy');
        const cursorCount = 10;
        for (let index = 0; index < cursorCount; index++) {
            new SVGCursor(index, (inputAction: any) => this.GetFingerTipElement(inputAction, index), 'cx', 'cy');
        }
    }

    GetFingerTipElement(inputAction: any, index: number) {
        if (inputAction?.FingerTipPositions) {
            return inputAction?.FingerTipPositions[index];
        }
        return null;
    }
}
