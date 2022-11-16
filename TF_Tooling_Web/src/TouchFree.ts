import { ConnectionManager } from './Connection/ConnectionManager';
import { SVGCursor } from './Cursors/SvgCursor';
import { TouchlessCursor } from './Cursors/TouchlessCursor';
import { WebInputController } from './InputControllers/WebInputController';
import { HandDataManager } from './Plugins/HandDataManager';
import { InputActionManager } from './Plugins/InputActionManager';
import { TouchFreeEvent, TouchFreeEventSignatures } from './TouchFreeToolingTypes';

let InputController: WebInputController | undefined;
let CurrentCursor: TouchlessCursor | undefined;

// Class: TfInitParams
// Extra options for use when initializing TouchFree
export interface TfInitParams {
    initialiseCursor?: boolean;
}

// Function: Init
// Initializes TouchFree - must be called before any functionality requiring a TouchFree service connection.
const Init = (_tfInitParams?: TfInitParams): void => {
    ConnectionManager.init();

    ConnectionManager.AddConnectionListener(() => {
        InputController = new WebInputController();

        if (_tfInitParams === undefined) {
            CurrentCursor = new SVGCursor();
        } else {
            if (_tfInitParams.initialiseCursor === undefined || _tfInitParams.initialiseCursor === true) {
                CurrentCursor = new SVGCursor();
            }
        }
    });
};

// Class: EventHandle
// Object that can unregister a callback from an event
// Returned when registering a callback to an event
export interface EventHandle {
    UnregisterEventCallback(): void;
}

const MakeCustomEventWrapper = <T>(callback: (arg: T) => void): EventListener => {
    return ((evt: CustomEvent<T>) => {
        callback(evt.detail);
    }) as EventListener;
};

const EventTargets: { [T in TouchFreeEvent]: () => EventTarget } = {
    HandFound: () => ConnectionManager.instance,
    OnConnected: () => ConnectionManager.instance,
    OnTrackingServiceStateChange: () => ConnectionManager.instance,
    HandsLost: () => ConnectionManager.instance,
    TransmitHandData: () => HandDataManager.instance,
    InputAction: () => InputActionManager.instance,
    TransmitInputActionRaw: () => InputActionManager.instance,
    TransmitInputAction: () => InputActionManager.instance,
};

const EventListeners: { [T in TouchFreeEvent]: (callback: TouchFreeEventSignatures[T]) => EventListener } = {
    // Map void functions, they can just be returned directly
    OnConnected: (callback) => callback,
    HandFound: (callback) => callback,
    HandsLost: (callback) => callback,

    // Callbacks with an argument need to be transformed to a function taking CustomEvent<T>
    OnTrackingServiceStateChange: (callback) => MakeCustomEventWrapper(callback),
    InputAction: (callback) => MakeCustomEventWrapper(callback),
    TransmitHandData: (callback) => MakeCustomEventWrapper(callback),
    TransmitInputActionRaw: (callback) => MakeCustomEventWrapper(callback),
    TransmitInputAction: (callback) => MakeCustomEventWrapper(callback),
};

// Function: RegisterEventCallback
// Registers a callback function to be called when a specific event occurs
// Returns an `EventHandle` that can be used to unregister the callback
//
// Events and expected callback signatures:
//
// OnConnected: () => void;
// Event dispatched when connecting to the TouchFree service
//
// OnTrackingServiceStateChange: (state: TrackingServiceState) => void;
// Event dispatched when the connection between TouchFreeService and Ultraleap Tracking Service changes
//
// HandFound: () => void;
// Event dispatched when the first hand has started tracking
//
// HandsLost: () => void;
// Event dispatched when the last hand has stopped tracking
//
// TransmitHandData: (data: HandFrame) => void;
// Event dispatched when new hand data is available
//
// InputAction: (inputAction: TouchFreeInputAction) => void;
// Event dispatched when any input action is received from the TouchFree service
//
// TransmitInputActionRaw: (inputAction: TouchFreeInputAction) => void;
// Event dispatched directly from the <InputActionManager> without any proxying
//
// TransmitInputAction: (inputAction: TouchFreeInputAction) => void;
// Event dispatched from the <InputActionManager> to each registered Plugin
const RegisterEventCallback = <TEvent extends TouchFreeEvent>(
    event: TEvent,
    callback: TouchFreeEventSignatures[TEvent]
): EventHandle => {
    const eventType = event as TouchFreeEvent;
    const target = EventTargets[event]();
    const listener = EventListeners[event](callback);
    target.addEventListener(eventType, listener);
    return { UnregisterEventCallback: () => target.removeEventListener(eventType, listener) };
};

// Function: Dispatch Event
// Dispatches an event of the specific type with arguments if the event requires any.
// Events and expected arguments:
//
// OnConnected: () => void;
// Event dispatched when connecting to the TouchFree service
//
// OnTrackingServiceStateChange: (state: TrackingServiceState) => void;
// Event dispatched when the connection between TouchFreeService and Ultraleap Tracking Service changes
//
// HandFound: () => void;
// Event dispatched when the first hand has started tracking
//
// HandsLost: () => void;
// Event dispatched when the last hand has stopped tracking
//
// TransmitHandData: (data: HandFrame) => void;
// Event dispatched when new hand data is available
//
// InputAction: (inputAction: TouchFreeInputAction) => void;
// Event dispatched when any input action is received from the TouchFree service
//
// TransmitInputActionRaw: (inputAction: TouchFreeInputAction) => void;
// Event dispatched directly from the <InputActionManager> without any proxying
//
// TransmitInputAction: (inputAction: TouchFreeInputAction) => void;
// Event dispatched from the <InputActionManager> to each registered Plugin
const DispatchEvent = <TEvent extends TouchFreeEvent>(
    eventType: TEvent,
    ...args: Parameters<TouchFreeEventSignatures[TEvent]>
) => {
    let event: Event;
    if (args.length === 0) {
        event = new Event(eventType);
    } else {
        event = new CustomEvent(eventType, { detail: args[0] });
    }

    const target = EventTargets[eventType]();
    target.dispatchEvent(event);
};

// Bundle all our exports into a default object
// Benefit to this is IDE autocomplete for "TouchFree" will find this object
export default {
    RegisterEventCallback,
    DispatchEvent,
    Init,
    CurrentCursor,
    InputController,
};