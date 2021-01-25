export class ClientInputAction {
    timestamp: Number;
    interactionType: InteractionType;
    handType: HandType;
    chirality: HandChirality;
    inputType: InputType;
    cursorPosition: Vector2;
    distanceFromScreen: float;
    progressToClick: float;

    constructor(
        _timestamp: Number,
        _interactionType: InteractionType,
        _handType: HandType,
        _handChirality: HandChirality,
        _inputType: InputType,
        _cursorPosition: Array<Number>,
        _distanceFromScreen: Number,
        _progressToClick: Number) {
        this.timestamp = _timestamp;
    }

    constructor(_wsInput: WebSocketInputAction) {
        this.timestamp = _wsInput.Timestamp;
        this.interactionType = TypeUtilities.GetInteractionTypeFromFlags(_wsInput.InteractionFlags);
        this.handType = TypeUtilities.GetHandTypeFromFlags(_wsInput.InteractionFlags);
        this.chirality = TypeUtilities.GetChiralityFromFlags(_wsInput.InteractionFlags);
        this.inputType = TypeUtilities.GetInputTypeFromFlags(_wsInput.InteractionFlags);
        this.cursorPosition = _wsInput.CursorPosition;
        this.distanceFromScreen = _wsInput.DistanceFromScreen;
        this.progressToClick = _wsInput.ProgressToClick;
    }
}

// Enum: HandChirality
// LEFT - The left hand
// RIGHT - The right hand
export enum HandChirality {
    LEFT,
    RIGHT
}

// Enum: HandType
// PRIMARY - The first hand found
// SECONDARY - The second hand found
export enum HandType {
    PRIMARY,
    SECONDARY,
}

// Enum: InputType
// CANCEL - Used to cancel the current input if an issue occurs. Particularly when a DOWN has happened before an UP
// DOWN - Used to begin a 'Touch' or a 'Drag'
// MOVE - Used to move a cursor or to perform a 'Drag' after a DOWN
// UP - Used to complete a 'Touch' or a 'Drag'
export enum InputType {
    CANCEL,
    DOWN,
    MOVE,
    UP,
}

// Enum: InteractionType
// GRAB - The user must perform a GRAB gesture to 'Touch' by bringing their fingers and thumb together
// HOVER - The user must perform a HOVER gesture to 'Touch' by holding their hand still for a fixed time
// PUSH - The user must perform a PUSH gesture to 'Touch' by pushing their hand toward the screen
export enum InteractionType {
    GRAB,
    HOVER,
    PUSH,
}

// Enum: BitmaskFlags
// This is used to request any combination of the <HandChiralities>, <HandTypes>, <InputTypes>,
// and <InteractionTypes> flags from the Service at once.
export enum BitmaskFlags {
    NONE = 0,

    // HandChirality
    LEFT = 1,
    RIGHT = 2,

    // Hand Type
    PRIMARY = 4,
    SECONDARY = 8,

    // Input Types
    CANCEL = 16,
    DOWN = 32,
    MOVE = 64,
    UP = 128,

    // Interaction Types
    GRAB = 256,
    HOVER = 512,
    PUSH = 1024,

    // Adding elements to this list is a breaking change, and should cause at
    // least a minor iteration of the API version UNLESS adding them at the end
}

// Class: WebsocketInputAction
// The version of an InputAction received via the WebSocket. This must be converted into a
// <ClientInputAction> to be used by the client and can be done so via its constructor.
export class WebsocketInputAction {
    timestamp: Number;
    interactionFlags: BitmaskFlags;
    cursorPosition: Array<Number>;
    distanceFromScreen: Number;
    progressToClick: Number;

    constructor(
        _timestamp: Number,
        _interactionFlags: BitmaskFlags,
        _cursorPosition: Array<Number>,
        _distanceFromScreen: Number,
        _progressToClick: Number,
    ) {
        this.timestamp = _timestamp;
        this.interactionFlags = _interactionFlags;
        this.cursorPosition = _cursorPosition;
        this.distanceFromScreen = _distanceFromScreen;
        this.progressToClick = _progressToClick;
    }
}
