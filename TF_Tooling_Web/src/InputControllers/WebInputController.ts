import { SwipeDirection } from '../Connection/TouchFreeServiceTypes';
import {
    TouchFreeInputAction,
    InputType
} from '../TouchFreeToolingTypes';
import { BaseInputController } from './BaseInputController'

// Class: WebInputController
// Provides web PointerEvents based on the incoming data from TouchFree Service via a
// <ServiceConnection>.
//
// If you are using cursors with this InputController, ensure they have the "touchfree-cursor"
// class. This allows this class to ignore them when determining which elements should recieve
// new pointer events. If you don't do this, none of the events transmitted here are guaranteed
// to make it to their intended targets, as they will be captured by the cursor.

type MarginProperty = 'marginTop' | 'marginBottom' | 'marginLeft' | 'marginRight';
type ScrollProperty = 'scrollTop' | 'scrollLeft';

export class WebInputController extends BaseInputController {
    // Group: Variables

    // Variable: enterLeaveEnabled
    // Can be used to enable/disable the transmission of "pointerenter"/"pointerleave" events
    // Disable this for a minor performance boost, at the cost of no longer sending those events
    // to the UI.
    enterLeaveEnabled: boolean = true;

    private lastHoveredElement: Element | null = null;
    private readonly pointerId: number = 0;
    private readonly baseEventProps: PointerEventInit;
    private readonly activeEventProps: PointerEventInit;
    private elementsOnDown: HTMLElement[] | null = null;
    private handlingCloseToSwipe: boolean = false;
    private currentPosition: Array<number> = [];
    private lastPosition: Array<number> | null = null;
    private scrollDirection?: SwipeDirection = undefined;
    private elementToScroll?: HTMLElement = undefined;

    // Variables for Bump on almost swipe
    private bumpTotalDurationMS = 1200;
    private bumpTransitionDurationMS = this.bumpTotalDurationMS / 6;
    private bumpDistancePx = 100;

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

        this.currentPosition = _inputData.CursorPosition;

        let elementAtPos: Element | null = this.GetTopNonCursorElement(_inputData.CursorPosition);

        this.activeEventProps.clientX = _inputData.CursorPosition[0];
        this.activeEventProps.clientY = _inputData.CursorPosition[1];

        if (elementAtPos !== null) {
            let inputEvent: CustomEvent = new CustomEvent(`InputAction`, {detail: _inputData})
            elementAtPos.dispatchEvent(inputEvent);
        }

        switch (_inputData.InputType) {
            case InputType.CANCEL:
                this.ResetScrollData();
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

                this.HandleScroll(_inputData.CursorPosition);
                break;

            case InputType.DOWN:
                this.ResetScrollData();
                this.elementsOnDown = this.GetElementsOnDown(_inputData.CursorPosition);

                this.lastPosition = _inputData.CursorPosition;

                let downEvent: PointerEvent = new PointerEvent("pointerdown", this.activeEventProps);
                this.DispatchToTarget(downEvent, elementAtPos);
                break;

            case InputType.UP:
                this.ResetScrollData();

                let upEvent: PointerEvent = new PointerEvent("pointerup", this.activeEventProps);
                this.DispatchToTarget(upEvent, elementAtPos);
                break;
        }
    }

    private GetElementsOnDown(position: number[]): HTMLElement[] {
        return document.elementsFromPoint(position[0],position[1])
            .map(e => e as HTMLElement)
            .filter(
                e => e && !e.classList.contains("touchfreecursor") 
                && !e.classList.contains("touchfree-cursor") 
                && !e.classList.contains("touchfree-no-scroll")
            );
    }

    private ResetScrollData(): void {
        this.elementsOnDown = null;
        this.scrollDirection = undefined;
        this.elementToScroll = undefined;
    }
    
    protected HandleCloseToSwipe(direction?: SwipeDirection): void {
        if(this.handlingCloseToSwipe || direction === undefined) return;

        let element: HTMLElement | undefined; 
        this.elementsOnDown = this.GetElementsOnDown(this.currentPosition);
        switch (direction) {
            case SwipeDirection.UP:
                element = this.GetElementToScroll(
                    (e:HTMLElement)=> e.scrollHeight > e.clientHeight && e.scrollTop >= 0,
                    (e:HTMLElement, p:HTMLElement)=> e.offsetHeight === p.offsetHeight && e.scrollHeight === p.scrollHeight);
                case SwipeDirection.DOWN:
                element = this.GetElementToScroll(
                    (e:HTMLElement)=> e.scrollHeight > e.clientHeight && e.scrollTop + e.clientHeight < e.scrollHeight,
                    (e:HTMLElement, p:HTMLElement)=> e.offsetHeight === p.offsetHeight && e.scrollHeight === p.scrollHeight);
            case SwipeDirection.LEFT:
                element = this.GetElementToScroll(
                    (e:HTMLElement)=> e.scrollWidth > e.clientWidth && e.scrollLeft >= 0,
                    (e:HTMLElement, p:HTMLElement)=> e.offsetWidth === p.offsetWidth && e.scrollWidth === p.scrollWidth);
            case SwipeDirection.RIGHT:
                element = this.GetElementToScroll(
                    (e:HTMLElement)=> e.scrollWidth > e.clientWidth && e.scrollLeft + e.clientWidth < e.scrollWidth,
                    (e:HTMLElement, p:HTMLElement)=> e.offsetWidth === p.offsetWidth && e.scrollWidth === p.scrollWidth);
        }
        this.elementsOnDown = null;

        if(!element) return;

        this.handlingCloseToSwipe = true;

        this.BumpPage(element, direction);
    }

    protected BumpPage(element: HTMLElement, direction: SwipeDirection): void {
        let marginProperty: MarginProperty = 'marginTop';
        let scrollProperty: ScrollProperty = 'scrollTop';
        let elemScrollMax = 0; 
        let extraBumpDistance = 0;
        let swipePolarity: -1 | 1 = 1;

        switch (direction) {
            case SwipeDirection.UP:
                marginProperty = 'marginBottom';
                elemScrollMax = element.scrollHeight - element.clientHeight;
                extraBumpDistance = this.bumpDistancePx - (element.scrollHeight - element.clientHeight - element[scrollProperty]);
                swipePolarity = -1;
                break;
            case SwipeDirection.DOWN:
                extraBumpDistance = this.bumpDistancePx - element[scrollProperty];
                break;
            case SwipeDirection.LEFT:
                marginProperty = 'marginRight';
                scrollProperty = 'scrollLeft';
                elemScrollMax = element.scrollWidth - element.clientWidth;
                extraBumpDistance = this.bumpDistancePx - (element.scrollWidth - element.clientWidth - element[scrollProperty]);
                swipePolarity = -1;
                break;
            case SwipeDirection.RIGHT:
                marginProperty = 'marginLeft';
                scrollProperty = 'scrollLeft';
                extraBumpDistance = this.bumpDistancePx - element[scrollProperty];
                break;
        }

        const elemMargin = element.style[marginProperty];
        const elemScroll = element[scrollProperty];

        const elemScrollBehaviour = element.style.scrollBehavior;
        element.style.scrollBehavior = 'smooth';
        const elemTransition = element.style.transition;
        element.style.transition += `margin ${this.bumpTransitionDurationMS}ms`;

        if(extraBumpDistance >= 0){
            element[scrollProperty] = elemScrollMax;
            element.style[marginProperty] =  `${extraBumpDistance}px`;
            
            setTimeout(() => {
                element.style[marginProperty] = elemMargin;
                element[scrollProperty] = elemScroll;
            }, this.bumpTotalDurationMS - this.bumpTransitionDurationMS);

        } else {
            element[scrollProperty] = elemScroll - (this.bumpDistancePx * swipePolarity);

            setTimeout(() => {
                element[scrollProperty] = elemScroll;
            }, this.bumpTotalDurationMS - this.bumpTransitionDurationMS);
        }

        setTimeout(() => {
            element.style.transition = elemTransition;
            element.style.scrollBehavior = elemScrollBehaviour;
            this.handlingCloseToSwipe = false;
        }, this.bumpTotalDurationMS)
    }

    private HandleScroll(_position: Array<number>): void {
        if (this.elementsOnDown && this.lastPosition) {
            const changeInPositionX = this.lastPosition[0] - _position[0];
            const changeInPositionY = this.lastPosition[1] - _position[1];

            if (!this.scrollDirection && (Math.abs(changeInPositionX) > 5 || Math.abs(changeInPositionY) > 5)) {
                if (Math.abs(changeInPositionX) > Math.abs(changeInPositionY)) {
                    this.scrollDirection = changeInPositionX > 0 ? SwipeDirection.RIGHT : SwipeDirection.LEFT;
                } else {
                    this.scrollDirection = changeInPositionY > 0 ? SwipeDirection.DOWN : SwipeDirection.UP;
                }
            }
            
            this.lastPosition = _position;

            if (changeInPositionY > 0 && (this.scrollDirection === undefined || this.scrollDirection === SwipeDirection.DOWN)) {
                const element = this.GetElementToScroll(
                    (e:HTMLElement)=> e.scrollHeight > e.clientHeight && e.scrollTop + e.clientHeight < e.scrollHeight,
                    (e:HTMLElement, p:HTMLElement)=> e.offsetHeight === p.offsetHeight && e.scrollHeight === p.scrollHeight);
                    
                if (element) {
                    this.elementToScroll = element;
                    element.scrollTop = Math.min(element.scrollHeight - element.clientHeight, element.scrollTop + changeInPositionY);
                }
            }

            if (changeInPositionY < 0 && (this.scrollDirection === undefined || this.scrollDirection === SwipeDirection.UP)) {
                const element = this.GetElementToScroll(
                    (e:HTMLElement)=> e.scrollHeight > e.clientHeight && e.scrollTop > 0,
                    (e:HTMLElement, p:HTMLElement)=> e.offsetHeight === p.offsetHeight && e.scrollHeight === p.scrollHeight);

                if (element) {
                    this.elementToScroll = element;
                    element.scrollTop = Math.max(0, element.scrollTop + changeInPositionY);
                }
            }
            
            if (changeInPositionX > 0 && (this.scrollDirection === undefined || this.scrollDirection === SwipeDirection.RIGHT)) {
                const element = this.GetElementToScroll(
                    (e:HTMLElement)=> e.scrollWidth > e.clientWidth && e.scrollLeft + e.clientWidth < e.scrollWidth,
                    (e:HTMLElement, p:HTMLElement)=> e.offsetWidth === p.offsetWidth && e.scrollWidth === p.scrollWidth);
                    
                if (element) {
                    this.elementToScroll = element;
                    element.scrollLeft = Math.min(element.scrollWidth - element.clientWidth, element.scrollLeft + changeInPositionX);
                }
            }

            if (changeInPositionX < 0 && (this.scrollDirection === undefined || this.scrollDirection === SwipeDirection.LEFT)) {
                const element = this.GetElementToScroll(
                    (e:HTMLElement)=> e.scrollWidth > e.clientWidth && e.scrollLeft > 0,
                    (e:HTMLElement, p:HTMLElement)=> e.offsetWidth === p.offsetWidth && e.scrollWidth === p.scrollWidth);
                    
                if (element) {
                    this.elementToScroll = element;
                    element.scrollLeft = Math.max(0, element.scrollLeft + changeInPositionX);
                }
            }
        }
    }

    private GetElementToScroll = (
        scrollValidation: (element: HTMLElement) => boolean,
        parentScrollValidation: (element: HTMLElement, parentElement: HTMLElement) => boolean): HTMLElement | undefined => {

        if (this.elementToScroll) return this.elementToScroll;
        if (!this.elementsOnDown) return;
        
        for (let i = 0; i < this.elementsOnDown.length; i++) {
            let elementToCheckScroll = this.elementsOnDown[i];
            if (!scrollValidation(elementToCheckScroll)) continue;

            let parentSelected = false;
            let parentAsHtmlElement = elementToCheckScroll.parentElement as HTMLElement;
            while (parentAsHtmlElement) {
                if (!parentScrollValidation(elementToCheckScroll, parentAsHtmlElement)) {
                    break;
                }

                parentSelected = true;
                elementToCheckScroll = parentAsHtmlElement;
                parentAsHtmlElement = elementToCheckScroll.parentElement as HTMLElement;
            }

            if (parentSelected && !scrollValidation(elementToCheckScroll)) continue;

            return elementToCheckScroll;
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

                if (!elementsAtPos[i].classList.contains("touchfreecursor") && !elementsAtPos[i].classList.contains("touchfree-cursor")) {
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
        if (target !== null) {
            target.dispatchEvent(event);
        } else {
            document.dispatchEvent(event);
        }
    }
}
