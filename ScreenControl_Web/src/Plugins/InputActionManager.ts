import { ClientInputAction } from "../ScreenControlTypes";

export class InputActionManager extends EventTarget {
    // Event: TransmitInputAction
    // An event for transmitting <ClientInputActions> that are received via the <messageReceiver> to
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
    public static HandleInputAction(_action: ClientInputAction): void {
        
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
                        closest_center = this.raycast(cursorPos, this.getElementCenter(elements[0].element));

                        _action.CursorPosition[0] = closest_center.x;
                        _action.CursorPosition[1] = (window.innerHeight - closest_center.y);
                    }
                }
            }
        }

        let inputActionEvent: CustomEvent<ClientInputAction> = new CustomEvent<ClientInputAction>(
            'TransmitInputAction',
            { detail: _action }
        );

        InputActionManager.instance.dispatchEvent(inputActionEvent);
    }

    private static getDistance(element: Element, cursorPos: {x: number, y: number}): number {
        let center : {x: number, y: number} = this.getElementCenter(element);

        // UNCOMMENT THIS LINE IF YOU WANT TO TRY OUT PERFECT SNAPPING BASED ON
        // DISTANCE FROM THE EXACT BORDER OF THE OBJECT
        // VERY CPU INTENSIVE, MUST BE IMPROVED
        //center = this.raycast({x: cursorPos.x, y: cursorPos.y}, center);

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

    private static raycast(start: {x: number, y: number}, end: {x: number, y: number}): {x: number, y: number} {
        const vector: {x: number, y: number} = {
            x: end.x - start.x,
            y: end.y - start.y
        };

        const length: number = Math.sqrt(Math.pow(vector.x, 2) + Math.pow(vector.y, 2));

        let distance: number = length;
        let pos: {x: number, y: number} = {
            x: start.x,
            y: start.y
        }

        // THIS HERE IS VERY CPU CONSUMING, INCREASING THE FACTOR MAKES IT GO
        // FASTER BUT ALSO MAKES IT JITTERY WHEN SNAPPING
        // TODO: Make it faster
        while (!this.hasSnappableUnder([pos.x, pos.y]) && distance > 10) {
            pos.x += (vector.x / length) * 1;
            pos.y += (vector.y / length) * 1;

            distance = Math.sqrt(Math.pow(end.x - pos.x, 2) + Math.pow(end.y - pos.y, 2));
        }

        return pos;
    }
}