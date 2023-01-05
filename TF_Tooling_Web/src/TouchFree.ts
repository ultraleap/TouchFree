import { ConnectionManager } from './Connection/ConnectionManager';
import { SVGCursor } from './Cursors/SvgCursor';
import { TouchlessCursor } from './Cursors/TouchlessCursor';
import { WebInputController } from './InputControllers/WebInputController';
import { HandDataManager } from './Plugins/HandDataManager';
import { InputActionManager } from './Plugins/InputActionManager';
import { TouchFreeEvent, TouchFreeEventSignatures } from './TouchFreeToolingTypes';

/**
 * Global input controller initialized by {@link Init}
 * @public
 */
let InputController: WebInputController | undefined;

/**
 * Global cursor initialized by {@link Init}
 * @public
 */
let CurrentCursor: TouchlessCursor | undefined;

/**
 * Extra options for initializing TouchFree
 * @public
 */
export interface TfInitParams {
    initialiseCursor?: boolean;
}

/**
 * Initializes TouchFree - must be called before any functionality requiring a TouchFree service connection.
 * 
 * @param _tfInitParams - Optional extra initialization parameters
 * @public
 */
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

/**
 * Are we connected to the TouchFree service?
 * 
 * @returns Whether connected to TouchFree service or not.
 * @public
 */
const IsConnected = ():boolean => ConnectionManager.IsConnected;

/**
 * Object that can unregister a callback from an event
 * @public
 */
export interface EventHandle {
    /**
     * Unregister the callback represented by this object
     */
    UnregisterEventCallback(): void;
}

/**
 * Turns a callback with an argument into a {@link CustomEvent<T>} Event Listener
 * 
 * @param callback - The callback to wrap
 * @returns EventListener with the wrapper callback
 */
const MakeCustomEventWrapper = <T>(callback: (arg: T) => void): EventListener => {
    return ((evt: CustomEvent<T>) => {
        callback(evt.detail);
    }) as EventListener;
};

/**
 * Signature required for RegisterEvent functions
 */
type RegisterEventFunc = (target:EventTarget, eventType:TouchFreeEvent, listener:EventListener) => EventHandle;

/**
 * Default implementation of RegisterEvent
 */
const DefaultRegisterEventFunc:RegisterEventFunc = (target, eventType, listener) => {
    target.addEventListener(eventType, listener);
    return { UnregisterEventCallback: () => target.removeEventListener(eventType, listener) };
};

/**
 * Interface for each individual event's implementation details
 */
interface EventImpl<T extends TouchFreeEvent> {
    Target: EventTarget;
    WithCallback: (callback: TouchFreeEventSignatures[T]) => {
        Listener: EventListener,
        RegisterEventFunc: RegisterEventFunc
    }
}

/**
 * Mapped type of Event implementations for each {@link TouchFreeEvent}
 */
type EventImpls = {
    [T in TouchFreeEvent]: EventImpl<T>;
};

/**
 * Backing field to cache object creation
 */
let EventImplementationsBackingField: EventImpls | undefined;

/**
 * Implementation details for all events
 * 
 * @remarks
 * Any new events added to TouchFreeEvent require a new entry here to function
 * 
 * @returns A function that returns all event implementations 
 */
const EventImplementations: () => EventImpls = () => EventImplementationsBackingField ??= ({
    OnConnected: {
        Target: ConnectionManager.instance,
        WithCallback: (callback) => ({
            Listener: callback, // Void callback can be returned directly
            RegisterEventFunc: DefaultRegisterEventFunc
        })
    },
    WhenConnected: {
        Target: ConnectionManager.instance,
        WithCallback: (callback) => ({
            Listener: callback, // Void callback can be returned directly
            RegisterEventFunc: (_target, _eventType, _listener) => {
                // If we're already connected then run the callback
                if (IsConnected())
                {
                    callback();
                }

                // Piggyback OnConnected
                return RegisterEventCallback('OnConnected', callback);
            }
        })
    },
    OnTrackingServiceStateChange: {
        Target: ConnectionManager.instance,
        WithCallback: (callback) => ({
            Listener: MakeCustomEventWrapper(callback),
            RegisterEventFunc: DefaultRegisterEventFunc
        })
    },
    HandFound: {
        Target: ConnectionManager.instance,
        WithCallback: (callback) => ({
            Listener: callback, // Void callback can be returned directly
            RegisterEventFunc: DefaultRegisterEventFunc
        })
    },
    HandsLost: {
        Target: ConnectionManager.instance,
        WithCallback: (callback) => ({
            Listener: callback, // Void callback can be returned directly
            RegisterEventFunc: DefaultRegisterEventFunc
        })
    },
    InputAction: {
        Target: InputActionManager.instance,
        WithCallback: (callback) => ({
            Listener: MakeCustomEventWrapper(callback),
            RegisterEventFunc: DefaultRegisterEventFunc
        })
    },
    TransmitHandData: {
        Target: HandDataManager.instance,
        WithCallback: (callback) => ({
            Listener: MakeCustomEventWrapper(callback),
            RegisterEventFunc: DefaultRegisterEventFunc
        })
    },
    TransmitInputAction: {
        Target: InputActionManager.instance,
        WithCallback: (callback) => ({
            Listener: MakeCustomEventWrapper(callback),
            RegisterEventFunc: DefaultRegisterEventFunc
        })
    },
    TransmitInputActionRaw: {
        Target: InputActionManager.instance,
        WithCallback: (callback) => ({
            Listener: MakeCustomEventWrapper(callback),
            RegisterEventFunc: DefaultRegisterEventFunc
        })
    }
});

/**
 * Registers a callback function to be called when a specific event occurs
 * 
 * @remarks
 * Events and expected callback signatures:
 * 
 * OnConnected: () => void;
 * Event dispatched when connecting to the TouchFree service
 * 
 * WhenConnected: () => void;
 * Same as OnConnected but calls callback when already connected.
 * Note this event piggybacks as an "OnConnected" event on event targets.
 * 
 * OnTrackingServiceStateChange: (state: TrackingServiceState) => void;
 * Event dispatched when the connection between TouchFreeService and Ultraleap Tracking Service changes
 * 
 * HandFound: () => void;
 * Event dispatched when the first hand has started tracking
 * 
 * HandsLost: () => void;
 * Event dispatched when the last hand has stopped tracking
 * 
 * TransmitHandData: (data: HandFrame) => void;
 * Event dispatched when new hand data is available
 * 
 * InputAction: (inputAction: TouchFreeInputAction) => void;
 * Event dispatched when any input action is received from the TouchFree service
 * 
 * TransmitInputActionRaw: (inputAction: TouchFreeInputAction) => void;
 * Event dispatched directly from the <InputActionManager> without any proxying
 * 
 * TransmitInputAction: (inputAction: TouchFreeInputAction) => void;
 * Event dispatched from the <InputActionManager> to each registered Plugin
 * 
 * @param event - The event to register a callback to. See {@link TouchFreeEvent}
 * @param callback - The callback to register. Callback signature depends on event being registered. See {@link TouchFreeEventSignatures}
 * @returns An {@link EventHandle} that can be used to unregister the callback
 * 
 * @public
 */
const RegisterEventCallback = <TEvent extends TouchFreeEvent>(
    event: TEvent,
    callback: TouchFreeEventSignatures[TEvent]
): EventHandle => {
    const eventImpl = EventImplementations()[event];
    const target = eventImpl.Target;
    const callbackImpl = eventImpl.WithCallback(callback);
    const listener = callbackImpl.Listener;
    return callbackImpl.RegisterEventFunc(target, event, listener);
};

/**
 * Dispatches an event of the specific type with arguments if the event requires any.
 * 
 * @remarks
 * For details of events and their expected arguments see comment above {@link RegisterEventCallback}
 * 
 * @param eventType - The event to register a callback to. See {@link TouchFreeEvent}
 * @param args - Arguments for the event. Depends on the event being dispatched. See {@link TouchFreeEventSignatures}
 * 
 * @public
 */
export const DispatchEvent = <TEvent extends TouchFreeEvent>(
    eventType: TEvent,
    ...args: Parameters<TouchFreeEventSignatures[TEvent]>
) => {
    let event: Event;
    if (args.length === 0) {
        event = new Event(eventType);
    } else {
        event = new CustomEvent(eventType, { detail: args[0] });
    }

    const target = EventImplementations()[eventType].Target;
    target.dispatchEvent(event);
};

// Bundle all our exports into a default object
// Benefit to this is IDE autocomplete for "TouchFree" will find this object
/**
 * Top level TouchFree object - an entry point for using TouchFree
 * 
 * @public
 */
export const TouchFree = {
    CurrentCursor,
    InputController,
    Init,
    IsConnected,
    RegisterEventCallback,
};
export default TouchFree;