import { ConnectionManager } from "./Connection/ConnectionManager";
import { HandDataManager } from "./Plugins/HandDataManager";
import { InputActionManager } from "./Plugins/InputActionManager";
import { TouchFreeEvent, TouchFreeEventSignatures } from "./TouchFreeToolingTypes";

// Interface: EventHandle
// Object that can unregister a callback from an event
// Returned when registering a callback to an event
export interface EventHandle {
    UnregisterEventCallback():void;
}

const MakeCustomEventWrapper = <T>(callback:(arg:T)=>void):EventListener => {
    return ((evt:CustomEvent<T>) => {
        callback(evt.detail);
    }) as EventListener;
}

const EventTargets: { [T in TouchFreeEvent]: () => EventTarget} = {
    HandFound: () => ConnectionManager.instance,
    OnConnected: () => ConnectionManager.instance,
    OnTrackingServiceStateChange: () => ConnectionManager.instance,
    HandsLost: () => ConnectionManager.instance,
    TransmitHandData: () => HandDataManager.instance,
    InputAction: () => InputActionManager.instance,
    TransmitInputActionRaw: () => InputActionManager.instance,
    TransmitInputAction: () => InputActionManager.instance
}

const EventListeners: { [T in TouchFreeEvent]: (callback: TouchFreeEventSignatures[T]) => EventListener} = {
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
}

const RegisterEventCallback = <TEvent extends TouchFreeEvent>(event:TEvent, callback: TouchFreeEventSignatures[TEvent]):EventHandle => {
    const eventType = event as TouchFreeEvent;
    const target = EventTargets[event]();
    const listener = EventListeners[event](callback);
    target.addEventListener(eventType, listener);
    return { UnregisterEventCallback: () => target.removeEventListener(eventType, listener) };
}

const DispatchEvent = <TEvent extends TouchFreeEvent>(eventType:TEvent, ...args: Parameters<TouchFreeEventSignatures[TEvent]>) => {
    let event:Event;
    if (args.length === 0)
    {
        event = new Event(eventType);
    }
    else
    {
        event = new CustomEvent(eventType, {detail: args[0]});
    }

    const target = EventTargets[eventType]();
    target.dispatchEvent(event);
}

// Bundle all our exports into a default object
// Benefit to this is IDE autocomplete for "TouchFree" will find this object
export default {
    RegisterEventCallback,
    DispatchEvent
};