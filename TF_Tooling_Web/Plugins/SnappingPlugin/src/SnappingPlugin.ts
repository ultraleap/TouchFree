import { InputActionPlugin } from "../../../src/Plugins/InputActionPlugin";
import { TouchFreeInputAction } from "../../../src/TouchFreeToolingTypes";

import { SnappableElement } from "./SnappableElement";
import { Vector2 } from "./Vector2";

export enum SnapMode {
    Magnet,
    Center
}

export class SnappingPlugin extends InputActionPlugin {
    private snapDistance: number = 50;
    private snapMode: SnapMode = SnapMode.Magnet;

    public static MAX_SOFTNESS: number = 1;
    public static MIN_SOFTNESS: number = 0;

    private snapSoftness: number = 0.3;

    ModifyInputAction(_inputAction: TouchFreeInputAction): TouchFreeInputAction | null {

        const cursorPos: { x: number, y: number } = {
            x: _inputAction.CursorPosition[0],
            y: window.innerHeight - _inputAction.CursorPosition[1]
        }

        // Build a list of snappable elements
        let elements: SnappableElement[] = [...document.getElementsByClassName("snappable")].map(
            (value: Element, index: number, array: Element[]) => {
                return SnappableElement.Compute(value, Vector2.FromTuple(cursorPos));
            }
        );

        // Sort them by distance
        elements = elements.sort((a: SnappableElement, b: SnappableElement) => { return a.distance - b.distance; })

        // Let's snap if there is snappable elements
        if (elements.length > 0) {
            if (elements[0].distance < this.snapDistance) {
                if (this.snapMode === SnapMode.Center) {
                    // If snapForce = 1, cursor position inside the shape is the same
                    // If snapForce = 0, cursor position is snapped in the middle
                    let snapForce: number = Number.parseFloat(elements[0].element.getAttribute("data-snapforce") ?? this.snapSoftness.toString());

                    // From center of the shape to the border of the shape, following the direction between the center and the cursor
                    // From what we already have, vector = closest_center - closest_point
                    const centerToBorderVector: Vector2 = new Vector2(
                        elements[0].center.x - elements[0].closest_point.x,
                        elements[0].center.y - elements[0].closest_point.y
                    );

                    const distance: number = Math.sqrt(
                        Math.pow(centerToBorderVector.x, 2) +
                        Math.pow(centerToBorderVector.y, 2)
                    );

                    // Intensity of the lerp between the edge and the center
                    let softSnapT: number = (elements[0].center_distance / distance) * this.lerp(
                        SnappingPlugin.MIN_SOFTNESS,
                        SnappingPlugin.MAX_SOFTNESS,
                        snapForce
                    );

                    softSnapT = Math.max(Math.min(softSnapT, 1), 0);

                    const finalPos: { x: number, y: number } = {
                        x: this.lerp(elements[0].center.x, cursorPos.x, softSnapT),
                        y: this.lerp(elements[0].center.y, cursorPos.y, softSnapT),
                    };

                    _inputAction.CursorPosition[0] = finalPos.x;
                    _inputAction.CursorPosition[1] = (window.innerHeight - finalPos.y);
                } else {
                    if (!elements[0].hovered) {
                        _inputAction.CursorPosition[0] = elements[0].closest_point.x;
                        _inputAction.CursorPosition[1] = (window.innerHeight - elements[0].closest_point.y);
                    }
                }
            }
        }

        return _inputAction;
    }

    private lerp(x: number, y: number, a: number): number {
        return x * (1 - a) + y * a;
    }

    public SetSnapModeToMagnet() {
        this.snapMode = SnapMode.Magnet;
    }

    public SetSnapModeToCenter() {
        this.snapMode = SnapMode.Center;
    }

    public SetSnapDistance(value: number) {
        this.snapDistance = value;
    }

    public SetSnapSoftness(value: number) {
        this.snapSoftness = value;
    }
}