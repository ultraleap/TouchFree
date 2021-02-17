import {
    ClientInputAction,
    InputType
} from '../ScreenControlTypes';
import { BaseInputController } from './BaseInputController'

// Class: WebInputController
// Provides web PointerEvents based on the incoming data from ScreenControl Service via a
// <ServiceConnection>
export class WebInputController extends BaseInputController {
    // Group: Variables

    enterLeaveEnabled: boolean = true;

    private readonly pointerId: number = 0;
    private readonly baseEventProps: PointerEventInit;
    private readonly activeEventProps: PointerEventInit;
    private lastHoveredElement: Element | null = null;

    // Group: Methods

    // Function: Start
    // Locates the EventSystem and StandaloneInputModule that need to be overridden
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

    // pointerenter / leave do not bubble (but are not per child)
    // pointerover / pointerout do bubble (but are per child)

    HandleMove(_element: Element | null): void {
        if (_element != this.lastHoveredElement) {
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

        let moveEvent: PointerEvent = new PointerEvent("move", this.activeEventProps);
        _element?.dispatchEvent(moveEvent);

        this.lastHoveredElement = _element;
    }

    // Function: HandleInputAction
    // Called with each <ClientInputAction> as it comes into the <ServiceConnection>. Updates the
    // underlying InputModule and EventSystem based on the incoming actions.
    //
    // Parameters:
    //     _inputData - The latest Action to arrive via the <ServiceConnection>.
    protected HandleInputAction(_inputData: ClientInputAction): void {
        super.HandleInputAction(_inputData);

        let elementAtPos: Element | null = document.elementFromPoint(
            _inputData.CursorPosition[0],
            _inputData.CursorPosition[1]);

        this.activeEventProps.clientX = _inputData.CursorPosition[0];
        this.activeEventProps.clientY = _inputData.CursorPosition[1];

        switch (_inputData.InputType) {
            case InputType.CANCEL:
                let cancelEvent: PointerEvent = new PointerEvent("cancel", this.activeEventProps);
                if (elementAtPos != null) {
                    let parentTree = this.GetOrderedParents(elementAtPos);

                    parentTree.forEach((parent: Node | null) => {
                        if (parent != null) {
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
                let upEvent: PointerEvent = new PointerEvent("pointerdown", this.activeEventProps);
                this.DispatchToTarget(upEvent, elementAtPos);
                break;
        }
    }

    // Handle sending pointerleave/pointerenter events to the parent stacks
    // These events do not bubble, in order to deliver expected behaviour we must consider
    // the entire stack of elements above our current target in the document tree
    private HandleEnterLeaveBehaviour(_element: Element|null) {
        let oldParents: Array<Node | null> = this.GetOrderedParents(this.lastHoveredElement);
        let newParents: Array<Node | null> = this.GetOrderedParents(_element);

        let highestCommonIndex: number | null = this.GetCommonAncestorIndex(oldParents, newParents);

        let leaveEvent = new PointerEvent("pointerleave", this.activeEventProps);
        let enterEvent = new PointerEvent("pointerenter", this.activeEventProps);

        if (highestCommonIndex == null) {
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
        var parentStack: Array<Node | null> = [_node];

        for (; _node; _node = _node.parentNode) {
            parentStack.unshift(_node);
        }

        return parentStack;
    }

    // Takes two ordered arrays of Nodes (as produced by GetOrderedParents) and identifies the
    // lowest common ancestor of the two sets. Used in HandleMove for identifying the events to send
    private GetCommonAncestorIndex(oldParents: Array<Node | null>, newParents: Array<Node | null>): number | null {
        if (oldParents[0] != newParents[0]) {
            return null;
        }

        for (var i = 0; i < oldParents.length; i++) {
            if (oldParents[i] != newParents[i]) {
                return i;
            }
        }

        return null;
    }

    // Checks if the target element is null and correctly dispatches the provided event to the
    // element or document body appropriately
    private DispatchToTarget(event: PointerEvent, target: Element | null) {
        if (target != null) {
            target.dispatchEvent(event);
        } else {
            document.dispatchEvent(event);
        }
    }
}
