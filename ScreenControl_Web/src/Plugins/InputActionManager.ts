import { ClientInputAction } from "../ScreenControlTypes";

export class InputActionManager extends EventTarget {
    // Event: TransmitInputAction
    // An event for transmitting <ClientInputActions> that are received via the <messageReceiver> to
    // be listened to.

    // Variable: instance
    // The instance of the singleton for referencing the events transmitted
    static _instance: InputActionManager;

    static _visualSnapDistance: number = 100;
    static _snapDistance: number = 150;
    static _snapToCenter: boolean = false;

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

        // Get a list of snappable elements sorted by distance to the cursor
        const elements: Element[] = [...document.getElementsByClassName("snappable")].sort((a: Element, b: Element) => {
            // Negative if a closer than b
            // 0 if a and b equidistant (should not happen by a slight margin)
            // Positif if b closer than a
            return this.getDistance(a, _action.CursorPosition) - this.getDistance(b, _action.CursorPosition);
        })

        // Let's snap if there is snappable elements
        if (elements.length > 0) {
            const closest_center: {x: number, y: number} = this.getElementCenter(elements[0]);
            const closest_distance: number = this.getDistance(elements[0], _action.CursorPosition);

            if (closest_distance < this._snapDistance) {
                if (this._snapToCenter) {
                    _action.CursorPosition[0] = closest_center.x;
                    _action.CursorPosition[1] = (window.innerHeight - closest_center.y);
                }
                else {
                    if (closest_distance > this._visualSnapDistance) {
                        const direction: { x: number, y: number } = {
                            x: _action.CursorPosition[0] - closest_center.x,
                            y: (window.innerHeight - closest_center.y) - _action.CursorPosition[1]
                        };
                        closest_center.x += (direction.x / closest_distance) * this._visualSnapDistance;
                        closest_center.y += (direction.y / closest_distance) * this._visualSnapDistance;

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

    private static getDistance(element: Element, cursorPos: number[]): number {
        const center : {x: number, y: number} = this.getElementCenter(element);
        return Math.sqrt(
            Math.pow(cursorPos[0] - center.x, 2) +
            Math.pow((window.innerHeight - cursorPos[1]) - center.y, 2)
        );
    }

    private static getElementCenter(element: Element): {x: number, y: number} {
        const rect : DOMRect = element.getBoundingClientRect();
        return {
            x: rect.x + (rect.width / 2),
            y: rect.y + (rect.height / 2)
        };
    }
}