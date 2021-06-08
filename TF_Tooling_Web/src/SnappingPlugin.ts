import { InputActionPlugin } from "./Plugins/InputActionPlugin";
import { TouchFreeInputAction } from "./TouchFreeToolingTypes";

class SnappableElement {
    public element: Element;
    public distance: number;
    public closest_point: Vector2;
    public center: Vector2;
    public center_distance: number;
    public hovered: boolean

    constructor(
            element: Element,
            distance: number,
            closest_point: Vector2,
            center: Vector2,
            center_distance: number,
            hovered: boolean
    ) {
        this.element = element;
        this.distance = distance;
        this.closest_point = closest_point;
        this.center = center;
        this.center_distance = center_distance;
        this.hovered = hovered;
    }

    public static Compute(element: Element, distant_point: Vector2): SnappableElement {
        const rect : DOMRect = element.getBoundingClientRect();
        const center: Vector2 = new Vector2(rect.x + (rect.width / 2), rect.y + (rect.height / 2));
        const center_distance: number = Math.sqrt(
            Math.pow(distant_point.x - center.x, 2) +
            Math.pow(distant_point.y - center.y, 2)
        );

        const closest_point: Vector2 = Ray.Cast(distant_point, center, element);
        const distance: number = Math.sqrt(
            Math.pow(distant_point.x - closest_point.x, 2) +
            Math.pow(distant_point.y - closest_point.y, 2)
        );

        const hovered: boolean = closest_point === distant_point;

        return new SnappableElement(element, distance, closest_point, center, center_distance, hovered);
    }
}

class Ray {

    public static Cast(start: Vector2, end: Vector2, element: Element): Vector2 {

        const pos: Vector2 = new Vector2(start.x, start.y);

        let hasSnap: boolean = this.hit(pos, element);
        let hadSnap: boolean = hasSnap;

        // If we already have snappable under us, it means we already are in a button
        if (hasSnap) {
            return pos;
        }

        // Store the current segment
        const vector: {x: number, y: number} = {
            x: end.x - start.x,
            y: end.y - start.y
        };

        // Store the previous evaluated pos
        const prevPos: {x: number, y: number} = {
            x: pos.x,
            y: pos.y
        }

        // Store the next end point of the vector
        const nextEnd: {x: number, y: number} = {
            x: end.x,
            y: end.y
        }

        // Store the length of the vector
        let distance: number = Math.sqrt(Math.pow(vector.x, 2) + Math.pow(vector.y, 2));

        // We already checked the starting pos, let's step in the first step
        pos.x = pos.x + (vector.x / 2);
        pos.y = pos.y + (vector.y / 2);

        while (distance > 1) {
            hasSnap = this.hit(pos, element);

            // If we changed state, we reverse the direction
            if ((hasSnap && !hadSnap) || (!hasSnap && hadSnap)) {
                nextEnd.x = prevPos.x;
                nextEnd.y = prevPos.y;
            }

            // We get the new vector along we're moving
            vector.x = nextEnd.x - pos.x;
            vector.y = nextEnd.y - pos.y;

            // We get the new vector length
            distance = Math.sqrt(Math.pow(vector.x, 2) + Math.pow(vector.y, 2));

            // We store the previous position
            prevPos.x = pos.x;
            prevPos.y = pos.y;

            // We get the next position
            pos.x = pos.x + (vector.x / 2);
            pos.y = pos.y + (vector.y / 2);

            hadSnap = hasSnap;
        }

        return pos;
    }

    private static hit(position: Vector2, element: Element): boolean {
        let elementsAtPos: Element[] = document.elementsFromPoint(position.x, position.y);
        return elementsAtPos.find((value: Element, index: number, obj: Element[]) => value === element) !== undefined;
    }
}

class Vector2 {
    public x: number;
    public y: number;

    constructor(x: number, y: number) {
        this.x = x;
        this.y = y;
    }

    public static FromTuple(tuple: {x: number, y: number}): Vector2 {
        return new Vector2(tuple.x, tuple.y);
    }

    public toTuple(): {x: number, y: number} {
        return {x: this.x, y: this.y};
    }
}

export enum SnapMode {
    Magnet,
    Center
}

export class SnappingPlugin extends InputActionPlugin {

    private snapDistance: number = 25;
    private snapMode: SnapMode = SnapMode.Magnet;

    public static MAX_SOFTNESS: number = 1;
    public static MIN_SOFTNESS: number = 0;

    private snapSoftness: number = 0.3;

    ModifyInputAction(_inputAction: TouchFreeInputAction): TouchFreeInputAction | null {

        const cursorPos: {x: number, y: number} = {
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
        elements = elements.sort((a: SnappableElement, b: SnappableElement) => {return a.distance - b.distance;})

        // Let's snap if there is snappable elements
        if (elements.length > 0) {
            if (elements[0].distance < this.snapDistance) {
                if (this.snapMode === SnapMode.Center) {
                    // If snapForce = 1, cursor position inside the shape is the same
                    // If snapForce = 0, cursor position is snapped in the middle
                    let snapForce: number = Number.parseFloat(elements[0].element.getAttribute("data-snapforce") ?? this.snapSoftness.toString());

                    let softSnapT: number = (elements[0].center_distance / 50) * this.lerp(
                        SnappingPlugin.MIN_SOFTNESS,
                        SnappingPlugin.MAX_SOFTNESS,
                        snapForce);

                    softSnapT = Math.max(Math.min(softSnapT, 1), 0);

                    const finalPos: {x: number, y: number} = {x: 0, y: 0};
                    finalPos.x = this.lerp(elements[0].center.x, cursorPos.x, softSnapT);
                    finalPos.y = this.lerp(elements[0].center.y, cursorPos.y, softSnapT);

                    _inputAction.CursorPosition[0] = finalPos.x;
                    _inputAction.CursorPosition[1] = (window.innerHeight - finalPos.y);
                }
                else {
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