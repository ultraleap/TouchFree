import {
    ClientInputAction,
    InputType
} from '../ScreenControlTypes';
import { BaseInputController } from './BaseInputController'

// Class: WebInputController
// Provides web PointerEvents based on the incoming data from ScreenControl Service via a
// <ServiceConnection>.
//
// If you are using cursors with this InputController, ensure they have the "screencontrolcursor"
// class. This allows this class to ignore them when determining which elements should recieve
// new pointer events. If you don't do this, none of the events transmitted here are guaranteed
// to make it to their intended targets, as they will be captured by the cursor.
export class WebInputController extends BaseInputController {
    // Group: Variables

    // Variable: enterLeaveEnabled
    // Can be used to enable/disable the transmission of "pointerenter"/"pointerleave" events
    // Disable this for a minor performance boost, at the cost of no longer sending those events
    // to the UI.
    enterLeaveEnabled: boolean = true;

    distanceSnap : number = 50;

    private lastHoveredElement: Element | null = null;
    private readonly pointerId: number = 0;
    private readonly baseEventProps: PointerEventInit;
    private readonly activeEventProps: PointerEventInit;

    // Group: Methods

    // Function: constructor
    // Sets up the basic event properties for all events transmitted from this InputController.
    protected constructor() {
        super();

        this.baseEventProps = {
            pointerId: this.pointerId,
            bubbles: true,
            width: 10,
            height: 10,
            clientX: 0,
            clientY: 0,
            pointerType: "pen"
        };

        this.activeEventProps = this.baseEventProps;
    }

    // Function: HandleMove
    // Handles the transmission of "pointerout"/"pointerover"/"pointermove" events to appropriate
    // elements, based on the element being hovered over this frame (_element), and the element
    // hovered last frame.
    // Will also optionally send "pointerenter"/"pointerleave" events if enabled via
    // <enterLeaveEnabled>
    //
    // Parameters:
    //     _element - The DOM element under the cursor this frame
    HandleMove(_element: Element | null): void {
        if (_element !== this.lastHoveredElement) {
            // Handle sending pointerover/pointerout to the individual elements
            // These events bubble, so we only have to dispatch them to the element directly under
            // the cursor
            if (this.lastHoveredElement !== null) {
                let outEvent: PointerEvent = new PointerEvent("pointerout", this.activeEventProps);
                this.lastHoveredElement.dispatchEvent(outEvent);
            }

            if (_element !== null) {
                let overEvent: PointerEvent = new PointerEvent("pointerover", this.activeEventProps);
                _element.dispatchEvent(overEvent);
            }

            if (this.enterLeaveEnabled) {
                this.HandleEnterLeaveBehaviour(_element);
            }
        }

        let moveEvent: PointerEvent = new PointerEvent("pointermove", this.activeEventProps);
        _element?.dispatchEvent(moveEvent);

        this.lastHoveredElement = _element;
    }

    // Function: HandleInputAction
    // Called with each <ClientInputAction> as it comes into the <ServiceConnection>. Emits Pointer
    // events (e.g. pointermove/pointerdown) to the objects at the location. Which events are
    // emitted is affected by <enterLeaveEnabled>.
    //
    // Sends the following events by default:
    //
    //     - pointermove
    //     - pointerdown
    //     - pointerup
    //     - pointerover
    //     - pointerout
    //     - pointerenter
    //     - pointerleave
    //
    // Parameters:
    //     _inputData - The latest Action to arrive via the <ServiceConnection>.
    protected HandleInputAction(_inputData: ClientInputAction): void {
        _inputData.CursorPosition[1] = window.innerHeight - _inputData.CursorPosition[1];

        const snaps : HTMLCollectionOf<Element> = document.getElementsByClassName("snappable");
        let closest_distance : number = Infinity;
        let closest : any;
        const coords : {x : number, y : number} = {x: 0, y: 0};
        Array.from(snaps).forEach(snap => {
            const rect : DOMRect = snap.getBoundingClientRect();
            const center : {x : number, y : number} = {
                x: rect.x + (rect.width / 2),
                y: rect.y + (rect.height / 2)
            };
            const distance : number = Math.sqrt(
                Math.pow(_inputData.CursorPosition[0] - center.x, 2) +
                Math.pow((window.innerHeight - _inputData.CursorPosition[1]) - center.y, 2)
            );
            if (distance < closest_distance) {
                closest_distance = distance;
                closest = snap;
                coords.x = center.x;
                coords.y = center.y;
            }
        })

        if (closest_distance < this.distanceSnap && closest !== null) {
            _inputData.CursorPosition[0] = coords.x;
            _inputData.CursorPosition[1] = (window.innerHeight - coords.y);
        }

        super.HandleInputAction(_inputData);

        let elementAtPos: Element | null = this.GetTopNonCursorElement(_inputData.CursorPosition);

        this.activeEventProps.clientX = _inputData.CursorPosition[0];
        this.activeEventProps.clientY = _inputData.CursorPosition[1];

        switch (_inputData.InputType) {
            case InputType.CANCEL:
                let cancelEvent: PointerEvent = new PointerEvent("cancel", this.activeEventProps);

                if (elementAtPos !== null) {
                    let parentTree = this.GetOrderedParents(elementAtPos);

                    parentTree.forEach((parent: Node | null) => {
                        if (parent !== null) {
                            parent.dispatchEvent(cancelEvent);
                        }
                    });
                }
                break;

            case InputType.MOVE:
                this.HandleMove(elementAtPos);
                break;

            case InputType.DOWN:
                let downEvent: PointerEvent = new PointerEvent("pointerdown", this.activeEventProps);
                this.DispatchToTarget(downEvent, elementAtPos);
                break;

            case InputType.UP:
                let upEvent: PointerEvent = new PointerEvent("pointerup", this.activeEventProps);
                this.DispatchToTarget(upEvent, elementAtPos);
                break;
        }
    }

    // Gets the stack of elements (topmost->bottommost) at this position and return the first non-
    // cursor element. Depends on all cursor elements being branded with the "cursor" class.
    private GetTopNonCursorElement(_position: Array<number>): Element | null {
        let elementsAtPos: Element[] | null = document.elementsFromPoint(
            _position[0],
            _position[1]);

        let elementAtPos: Element | null = null;


        if (elementsAtPos !== null) {
            for (let i = 0; i < elementsAtPos.length; i++) {

                if (!elementsAtPos[i].classList.contains("screencontrolcursor")) {
                    elementAtPos = elementsAtPos[i];
                    break;
                }
            }
        }

        return elementAtPos;
    }

    // Handle sending pointerleave/pointerenter events to the parent stacks
    // These events do not bubble, in order to deliver expected behaviour we must consider
    // the entire stack of elements above our current target in the document tree
    private HandleEnterLeaveBehaviour(_element: Element | null) {
        let oldParents: Array<Node | null> = this.GetOrderedParents(this.lastHoveredElement);
        let newParents: Array<Node | null> = this.GetOrderedParents(_element);

        let highestCommonIndex: number | null = this.GetCommonAncestorIndex(oldParents, newParents);

        let leaveEvent = new PointerEvent("pointerleave", this.activeEventProps);
        let enterEvent = new PointerEvent("pointerenter", this.activeEventProps);

        if (highestCommonIndex === null) {
            oldParents.forEach((parentNode) => {
                parentNode?.dispatchEvent(leaveEvent);
            });

            newParents.forEach((parentNode: Node | null) => {
                parentNode?.dispatchEvent(enterEvent);
            });
        } else {
            oldParents.slice(highestCommonIndex).forEach((parentNode) => {
                parentNode?.dispatchEvent(leaveEvent);
            });

            newParents.slice(highestCommonIndex).forEach((parentNode) => {
                parentNode?.dispatchEvent(enterEvent);
            });
        }
    }

    // Collects the stack of parent nodes, ordered from highest (document body) to lowest
    // (the node provided)
    private GetOrderedParents(_node: Node | null): Array<Node | null> {
        let parentStack: Array<Node | null> = [_node];

        for (; _node; _node = _node.parentNode) {
            parentStack.unshift(_node);
        }

        return parentStack;
    }

    // Takes two ordered arrays of Nodes (as produced by GetOrderedParents) and identifies the
    // lowest common ancestor of the two sets. Used in HandleMove for identifying the events to send
    private GetCommonAncestorIndex(oldParents: Array<Node | null>, newParents: Array<Node | null>): number | null {
        if (oldParents[0] !== newParents[0]) {
            return null;
        }

        for (let i = 0; i < oldParents.length; i++) {
            if (oldParents[i] !== newParents[i]) {
                return i;
            }
        }

        return null;
    }

    // Checks if the target element is null and correctly dispatches the provided event to the
    // element or document body appropriately
    private DispatchToTarget(event: PointerEvent, target: Element | null) {
        // console.log(`Target was null? ${target === null}`)
        // console.log(`Dispatching ${event.type} to ${target?.nodeName}`);

        if (target !== null) {
            target.dispatchEvent(event);
        } else {
            document.dispatchEvent(event);
        }
    }
}
