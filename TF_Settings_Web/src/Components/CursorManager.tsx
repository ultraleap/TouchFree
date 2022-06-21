import { DotCursor } from "../TouchFree/Cursors/DotCursor";

import dotImage from "../Images/Cursor/Dot.png";
import ringImage from "../Images/Cursor/Ring.png";

export class CursorManager {
    private ringImage: HTMLImageElement;
    private dotImage: HTMLImageElement;

    constructor() {
        this.ringImage = this.constructCursorImg(
            ringImage,
            "absolute",
            30,
            "1000",
            true,
        );
        this.dotImage = this.constructCursorImg(
            dotImage,
            "absolute",
            30,
            "1001",
            false,
        );

        new DotCursor(this.dotImage, this.ringImage);
    }

    setElement(element_: HTMLDivElement) {
        if (!element_.contains(this.ringImage)) {
            element_.append(this.ringImage);
        }

        if (!element_.contains(this.dotImage)) {
            element_.append(this.dotImage);
        }
    }

    constructCursorImg(
        src_: string,
        position_: string,
        size_: number,
        zIndex_: string,
        dropShadowColor_: boolean,
    ): HTMLImageElement {
        const returnImage = document.createElement("img");

        returnImage.src = src_;
        returnImage.style.position = position_;
        returnImage.width = size_;
        returnImage.height = size_;
        returnImage.style.zIndex = zIndex_;

        let filter = "drop-shadow(0px 0px 5px white)";

        if (dropShadowColor_) {
            filter = "drop-shadow(0px 0px 5px white) invert(1)";
        }

        returnImage.style.filter = filter;

        // This style makes the images that make up the cursor ignore pointerevents and also
        // makes them invisible to the getElement(s)FromPoint, and as such is required by TouchFree
        // to ensure events are correctly sent to the elements _under_ the cursor.
        returnImage.style.pointerEvents = "none";

        return returnImage;
    }
}
