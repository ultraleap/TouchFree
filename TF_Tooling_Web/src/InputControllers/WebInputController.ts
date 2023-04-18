import TouchFree from '../TouchFree';
import { TouchFreeInputAction, InputType } from '../TouchFreeToolingTypes';
import { BaseInputController } from './BaseInputController';

// Class: WebInputController
// Provides web PointerEvents based on the incoming data from TouchFree Service via a
// <ServiceConnection>.
//
// If you are using cursors with this InputController, ensure they have the "touchfree-cursor"
// class. This allows this class to ignore them when determining which elements should receive
// new pointer events. If you don't do this, none of the events transmitted here are guaranteed
// to make it to their intended targets, as they will be captured by the cursor.
export class WebInputController extends BaseInputController {
    // Group: Variables

    // Variable: enterLeaveEnabled
    // Can be used to enable/disable the transmission of "pointerenter"/"pointerleave" events
    // Disable this for a minor performance boost, at the cost of no longer sending those events
    // to the UI.
    enterLeaveEnabled = true;

    private lastHoveredElement: Element | null = null;
    private readonly pointerId: number = 0;
    private readonly baseEventProps: PointerEventInit;
    private readonly activeEventProps: PointerEventInit;
    private elementsOnDown: HTMLElement[] | null = null;
    private scrollElementsOnDown: HTMLElement[] | null = null;
    private lastPosition: Array<number> | null = null;
    private scrollDirection: ScrollDirection | undefined = undefined;
    private elementToScroll: HTMLElement | undefined = undefined;

    // Constant: noScrollClassName
    // Any element with this class name in its css class list will be ignored when trying to find
    // the correct element for the WebInputController to scroll
    private readonly noScrollClassName: string = 'touchfree-no-scroll';

    // Group: Methods

    // Function: constructor
    // Sets up the basic event properties for all events transmitted from this InputController.
    constructor() {
        super();

        this.baseEventProps = {
            pointerId: this.pointerId,
            bubbles: true,
            isPrimary: true,
            width: 10,
            height: 10,
            clientX: 0,
            clientY: 0,
            pointerType: 'pen',
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
                const outEvent: PointerEvent = new PointerEvent('pointerout', this.activeEventProps);
                this.lastHoveredElement.dispatchEvent(outEvent);
            }

            if (_element !== null) {
                const overEvent: PointerEvent = new PointerEvent('pointerover', this.activeEventProps);
                _element.dispatchEvent(overEvent);
            }

            if (this.enterLeaveEnabled) {
                this.HandleEnterLeaveBehaviour(_element);
            }
        }

        const moveEvent: PointerEvent = new PointerEvent('pointermove', this.activeEventProps);
        _element?.dispatchEvent(moveEvent);

        this.lastHoveredElement = _element;
    }

    // Function: HandleInputAction
    // Called with each <TouchFreeInputAction> as it comes into the <ServiceConnection>. Emits Pointer
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
    protected HandleInputAction(_inputData: TouchFreeInputAction): void {
        super.HandleInputAction(_inputData);

        const elementsAtPoint = document.elementsFromPoint(_inputData.CursorPosition[0], _inputData.CursorPosition[1]);
        const elementAtPos: Element | null = this.GetTopNonCursorElement(elementsAtPoint);

        this.activeEventProps.clientX = _inputData.CursorPosition[0];
        this.activeEventProps.clientY = _inputData.CursorPosition[1];

        if (elementAtPos !== null) {
            TouchFree.DispatchEvent('InputAction', _inputData);
        }

        switch (_inputData.InputType) {
            case InputType.CANCEL: {
                this.ResetScrollData();
                const cancelEvent: PointerEvent = new PointerEvent('pointercancel', this.activeEventProps);
                const outEvent: PointerEvent = new PointerEvent('pointerout', this.activeEventProps);

                if (this.lastHoveredElement !== null) {
                    this.lastHoveredElement.dispatchEvent(cancelEvent);
                    this.lastHoveredElement.dispatchEvent(outEvent);

                    this.lastHoveredElement = null;
                }

                const elementOnDown = this.GetTopNonCursorElement(this.elementsOnDown);
                if (elementOnDown) {
                    elementOnDown.dispatchEvent(cancelEvent);
                    elementOnDown.dispatchEvent(outEvent);
                }

                if (elementAtPos !== null) {
                    const parentTree = this.GetOrderedParents(elementAtPos);

                    parentTree.forEach((parent: Node | null) => {
                        if (parent !== null) {
                            parent.dispatchEvent(cancelEvent);
                            parent.dispatchEvent(outEvent);
                        }
                    });
                }
                break;
            }

            case InputType.MOVE:
                this.HandleMove(elementAtPos);

                this.HandleScroll(_inputData.CursorPosition);
                break;

            case InputType.DOWN: {
                this.ResetScrollData();
                this.elementsOnDown = this.clickableElementsAtPosition(elementsAtPoint);
                this.scrollElementsOnDown = this.elementsOnDown.filter(
                    (e) => !e.classList.contains(this.noScrollClassName)
                );

                this.lastPosition = _inputData.CursorPosition;

                const downEvent: PointerEvent = new PointerEvent('pointerdown', this.activeEventProps);
                this.DispatchToTarget(downEvent, elementAtPos);
                break;
            }

            case InputType.UP: {
                const elementsOnUp = this.clickableElementsAtPosition(elementsAtPoint);

                if (elementsOnUp?.length && this.elementsOnDown?.length) {
                    for (const element of elementsOnUp) {
                        const matchingElement = this.elementsOnDown.find((eod) => eod == element);
                        if (matchingElement) {
                            matchingElement.click();
                            break;
                        }
                    }
                }

                this.ResetScrollData();

                const upEvent: PointerEvent = new PointerEvent('pointerup', this.activeEventProps);
                this.DispatchToTarget(upEvent, elementAtPos);
                break;
            }
        }
    }

    private clickableElementsAtPosition(elements: Element[] | null): HTMLElement[] {
        return (elements ?? [])
            .map((e) => e as HTMLElement)
            .filter((e) => e && !e.classList.contains('touchfreecursor') && !e.classList.contains('touchfree-cursor'));
    }

    // Clears information about the current scroll
    private ResetScrollData(): void {
        this.scrollElementsOnDown = null;
        this.scrollDirection = undefined;
        this.elementToScroll = undefined;
    }

    // Applies scrolling to any elements that should be scrolled
    private HandleScroll(_position: Array<number>): void {
        if (this.scrollElementsOnDown && this.lastPosition) {
            const changeInPositionX = this.lastPosition[0] - _position[0];
            const changeInPositionY = this.lastPosition[1] - _position[1];

            if (!this.scrollDirection && (Math.abs(changeInPositionX) > 5 || Math.abs(changeInPositionY) > 5)) {
                if (Math.abs(changeInPositionX) > Math.abs(changeInPositionY)) {
                    this.scrollDirection = changeInPositionX > 0 ? ScrollDirection.Right : ScrollDirection.Left;
                } else {
                    this.scrollDirection = changeInPositionY > 0 ? ScrollDirection.Down : ScrollDirection.Up;
                }
            }

            this.lastPosition = _position;

            if (
                changeInPositionY > 0 &&
                (this.scrollDirection === undefined || this.scrollDirection === ScrollDirection.Down)
            ) {
                const element = this.GetElementToScroll(
                    (e: HTMLElement) =>
                        e.scrollHeight > e.clientHeight && e.scrollTop + e.clientHeight < e.scrollHeight - 1,
                    (e: HTMLElement, p: HTMLElement) =>
                        e.offsetHeight === p.offsetHeight && e.scrollHeight === p.scrollHeight
                );

                if (element) {
                    this.elementToScroll = element;
                    element.scrollTop = Math.min(
                        element.scrollHeight - element.clientHeight,
                        element.scrollTop + changeInPositionY
                    );
                }
            }

            if (
                changeInPositionY < 0 &&
                (this.scrollDirection === undefined || this.scrollDirection === ScrollDirection.Up)
            ) {
                const element = this.GetElementToScroll(
                    (e: HTMLElement) => e.scrollHeight > e.clientHeight && e.scrollTop > 0,
                    (e: HTMLElement, p: HTMLElement) =>
                        e.offsetHeight === p.offsetHeight && e.scrollHeight === p.scrollHeight
                );

                if (element) {
                    this.elementToScroll = element;
                    element.scrollTop = Math.max(0, element.scrollTop + changeInPositionY);
                }
            }

            if (
                changeInPositionX > 0 &&
                (this.scrollDirection === undefined || this.scrollDirection === ScrollDirection.Right)
            ) {
                const element = this.GetElementToScroll(
                    (e: HTMLElement) => e.scrollWidth > e.clientWidth && e.scrollLeft + e.clientWidth < e.scrollWidth,
                    (e: HTMLElement, p: HTMLElement) =>
                        e.offsetWidth === p.offsetWidth && e.scrollWidth === p.scrollWidth
                );

                if (element) {
                    this.elementToScroll = element;
                    element.scrollLeft = Math.min(
                        element.scrollWidth - element.clientWidth,
                        element.scrollLeft + changeInPositionX
                    );
                }
            }

            if (
                changeInPositionX < 0 &&
                (this.scrollDirection === undefined || this.scrollDirection === ScrollDirection.Left)
            ) {
                const element = this.GetElementToScroll(
                    (e: HTMLElement) => e.scrollWidth > e.clientWidth && e.scrollLeft > 0,
                    (e: HTMLElement, p: HTMLElement) =>
                        e.offsetWidth === p.offsetWidth && e.scrollWidth === p.scrollWidth
                );

                if (element) {
                    this.elementToScroll = element;
                    element.scrollLeft = Math.max(0, element.scrollLeft + changeInPositionX);
                }
            }
        }
    }

    // Gets the element that should have scrolling applied to it.
    // Any elements with the class name listed as noScrollClassName applied will be ignored when
    // finding which element to scroll
    private GetElementToScroll = (
        scrollValidation: (element: HTMLElement) => boolean,
        parentScrollValidation: (element: HTMLElement, parentElement: HTMLElement) => boolean
    ): HTMLElement | undefined => {
        if (this.elementToScroll) return this.elementToScroll;
        if (!this.scrollElementsOnDown) return;

        for (let i = 0; i < this.scrollElementsOnDown.length; i++) {
            let elementToCheckScroll = this.scrollElementsOnDown[i];
            if (!scrollValidation(elementToCheckScroll)) continue;

            let parentSelected = false;
            let parentAsHtmlElement = elementToCheckScroll.parentElement as HTMLElement;
            while (parentAsHtmlElement) {
                const parentIsNoScroll = parentAsHtmlElement.classList.contains(this.noScrollClassName);
                const elementIsNoScroll = elementToCheckScroll.classList.contains(this.noScrollClassName);
                const parentScrollValid = parentScrollValidation(elementToCheckScroll, parentAsHtmlElement);

                if (!parentIsNoScroll && !elementIsNoScroll && !parentScrollValid) {
                    break;
                }

                if (parentIsNoScroll) {
                    parentAsHtmlElement = parentAsHtmlElement.parentElement as HTMLElement;
                } else {
                    parentSelected = true;
                    elementToCheckScroll = parentAsHtmlElement;
                    parentAsHtmlElement = elementToCheckScroll.parentElement as HTMLElement;
                }
            }

            if (parentSelected && !scrollValidation(elementToCheckScroll)) continue;

            return elementToCheckScroll;
        }
    };

    // Gets the stack of elements (topmost->bottommost) at this position and return the first non-
    // cursor element. Depends on all cursor elements being branded with the "cursor" class.
    private GetTopNonCursorElement(elementsAtPos: Element[] | null): Element | null {
        let elementAtPos: Element | null = null;

        if (elementsAtPos !== null) {
            for (let i = 0; i < elementsAtPos.length; i++) {
                if (
                    !elementsAtPos[i].classList.contains('touchfreecursor') &&
                    !elementsAtPos[i].classList.contains('touchfree-cursor')
                ) {
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
        const oldParents: Array<Node | null> = this.GetOrderedParents(this.lastHoveredElement);
        const newParents: Array<Node | null> = this.GetOrderedParents(_element);

        const highestCommonIndex: number | null = this.GetCommonAncestorIndex(oldParents, newParents);

        const leaveEvent = new PointerEvent('pointerleave', this.activeEventProps);
        const enterEvent = new PointerEvent('pointerenter', this.activeEventProps);

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
        const parentStack: Array<Node | null> = [_node];

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
        if (target !== null) {
            target.dispatchEvent(event);
        } else {
            document.dispatchEvent(event);
        }
    }
}

// Enum: ScrollDirection
// The directions that a scroll can be in
enum ScrollDirection {
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}
