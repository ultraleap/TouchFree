import { TouchFreeInputAction } from "../TouchFreeToolingTypes";

export class InputActionManager extends EventTarget {
    // Event: TransmitInputAction
    // An event for transmitting <TouchFreeInputActions> that are received via the <messageReceiver> to
    // be listened to.

    // Variable: instance
    // The instance of the singleton for referencing the events transmitted
    static _instance: InputActionManager;

    public static _snapDistance: number = 50;
    public static _snapToCenter: boolean = false;

    public static get instance() {
        if (InputActionManager._instance == null) {
            InputActionManager._instance = new InputActionManager();
        }

        return InputActionManager._instance;
    }

    // Function: HandleInputAction
    // Called by the <messageReceiver> to relay a <ClientInputAction> that has been received to any
    // listeners of <TransmitInputAction>.
    public static HandleInputAction(_action: TouchFreeInputAction): void {
        
        const cursorPos: {x: number, y: number} = {
            x: _action.CursorPosition[0],
            y: window.innerHeight - _action.CursorPosition[1]
        }

        // Build a list of snappable elements and their closest distance from the cursor
        let elements: {element: Element, distance: number}[] = [...document.getElementsByClassName("snappable")].map(
            (value: Element, index: number, array: Element[]) => {
                return {element: value, distance: this.getDistance(value, cursorPos)};
            }
        );

        // Sort them by distance
        elements = elements.sort(
            (a: {element: Element, distance: number}, b: {element: Element, distance: number}) => {
            // Negative if a closer than b
            // 0 if a and b equidistant (should not happen by a slight margin)
            // Positif if b closer than a
            return a.distance - b.distance;
        })

        // Let's snap if there is snappable elements
        if (elements.length > 0) {
            let closest_center: {x: number, y: number} = this.getElementCenter(elements[0].element);
            const closest_distance: number = elements[0].distance;

            if (closest_distance < this._snapDistance) {
                if (this._snapToCenter) {
                    _action.CursorPosition[0] = closest_center.x;
                    _action.CursorPosition[1] = (window.innerHeight - closest_center.y);
                }
                else {
                    if (!this.hasSnappableUnder([cursorPos.x, cursorPos.y])) {
                        closest_center = this.raycast(cursorPos, this.getElementCenter(elements[0].element), elements[0].element);

                        _action.CursorPosition[0] = closest_center.x;
                        _action.CursorPosition[1] = (window.innerHeight - closest_center.y);
                    }
                }
            }
        }

        let inputActionEvent: CustomEvent<TouchFreeInputAction> = new CustomEvent<TouchFreeInputAction>(
            'TransmitInputAction',
            { detail: _action }
        );

        InputActionManager.instance.dispatchEvent(inputActionEvent);
    }

    private static getDistance(element: Element, cursorPos: {x: number, y: number}): number {
        let center : {x: number, y: number} = this.getElementCenter(element);

        center = this.raycast({x: cursorPos.x, y: cursorPos.y}, center, element);

        return Math.sqrt(
            Math.pow(cursorPos.x - center.x, 2) +
            Math.pow(cursorPos.y - center.y, 2)
        );
    }

    private static getElementCenter(element: Element): {x: number, y: number} {
        const rect : DOMRect = element.getBoundingClientRect();
        return {
            x: rect.x + (rect.width / 2),
            y: rect.y + (rect.height / 2)
        };
    }

    // Search for an element with "snappable" class under the cursor
    private static getTopSnappableElement(_position: Array<number>): Element | null {
        let elementsAtPos: Element[] | null = document.elementsFromPoint(
            _position[0],
            _position[1]);

        let elementAtPos: Element | undefined = undefined;

        if (elementsAtPos !== null) {
            elementAtPos = elementsAtPos.find((value: Element, index: number, obj: Element[]) => value.classList.contains("snappable"));
        }

        return elementAtPos === undefined ? null : elementAtPos;
    }

    private static hasSnappableUnder(_position: Array<number>): boolean {
        return this.getTopSnappableElement(_position) !== null;
    }

    private static hasSnappableUnderIs(_position: Array<number>, element: Element): boolean {
        return this.getTopSnappableElement(_position) === element;
    }

    private static raycast(
            start: {x: number, y: number},
            end: {x: number, y: number},
            element: Element
    ): {x: number, y: number} {
        const pos: {x: number, y: number} = {
            x: start.x,
            y: start.y
        }

        let hasSnap: boolean = this.hasSnappableUnderIs([pos.x, pos.y], element);
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
            hasSnap = this.hasSnappableUnderIs([pos.x, pos.y], element);

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
}