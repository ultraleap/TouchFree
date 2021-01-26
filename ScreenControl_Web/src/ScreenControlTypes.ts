export class ClientInputAction {
    timestamp: Number;
    interactionType: InteractionType;
    handType: HandType;
    chirality: HandChirality;
    inputType: InputType;
    cursorPosition: Array<Number>;
    distanceFromScreen: Number;
    progressToClick: Number;

    constructor(
        _timestamp: Number,
        _interactionType: InteractionType,
        _handType: HandType,
        _handChirality: HandChirality,
        _inputType: InputType,
        _cursorPosition: Array<Number>,
        _distanceFromScreen: Number,
        _progressToClick: Number)
    {
        this.timestamp = _timestamp;
        this.interactionType = _interactionType;
        this.handType = _handType;
        this.chirality = _handChirality;
        this.inputType = _inputType;
        this.cursorPosition = _cursorPosition;
        this.distanceFromScreen = _distanceFromScreen;
        this.progressToClick = _progressToClick;
    }
}

export function ConvertInputAction(_wsInput: WebsocketInputAction): ClientInputAction {
    return new ClientInputAction(
        _wsInput.timestamp,
        FlagUtilities.GetInteractionTypeFromFlags(_wsInput.interactionFlags),
        FlagUtilities.GetHandTypeFromFlags(_wsInput.interactionFlags),
        FlagUtilities.GetChiralityFromFlags(_wsInput.interactionFlags),
        FlagUtilities.GetInputTypeFromFlags(_wsInput.interactionFlags),
        _wsInput.cursorPosition,
        _wsInput.distanceFromScreen,
        _wsInput.progressToClick,
    );
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

// Class: FlagUtilities
// A collection of Utilities to be used when working with <BitmaskFlags>.
export class FlagUtilities {
    // Group: Functions

    // Function: GetInteractionFlags
    // Used to convert a collection of interaction enums to flags for sending
    // to the Service.
    static GetInteractionFlags(
        _interactionType: InteractionType,
        _handType: HandType,
        _chirality: HandChirality,
        _inputType: InputType): BitmaskFlags {
        var returnVal: BitmaskFlags = BitmaskFlags.NONE;

        switch (_handType) {
            case HandType.PRIMARY:
                returnVal ^= BitmaskFlags.PRIMARY;
                break;

            case HandType.SECONDARY:
                returnVal ^= BitmaskFlags.SECONDARY;
                break;
        }

        switch (_chirality) {
            case HandChirality.LEFT:
                returnVal ^= BitmaskFlags.LEFT;
                break;

            case HandChirality.RIGHT:
                returnVal ^= BitmaskFlags.RIGHT;
                break;
        }

        switch (_inputType) {
            case InputType.CANCEL:
                returnVal ^= BitmaskFlags.CANCEL;
                break;

            case InputType.MOVE:
                returnVal ^= BitmaskFlags.MOVE;
                break;

            case InputType.UP:
                returnVal ^= BitmaskFlags.UP;
                break;

            case InputType.DOWN:
                returnVal ^= BitmaskFlags.DOWN;
                break;
        }

        switch (_interactionType) {
            case InteractionType.PUSH:
                returnVal ^= BitmaskFlags.PUSH;
                break;

            case InteractionType.HOVER:
                returnVal ^= BitmaskFlags.HOVER;
                break;

            case InteractionType.GRAB:
                returnVal ^= BitmaskFlags.GRAB;
                break;
        }

        return returnVal;
    }

    // Function: GetChiralityFromFlags
    // Used to find which <HandChirality> _flags contains. Favours RIGHT if none or both are found.
    static GetChiralityFromFlags(_flags: BitmaskFlags): HandChirality {
        var chirality: HandChirality = HandChirality.RIGHT;

        if (_flags && BitmaskFlags.RIGHT) {
            chirality = HandChirality.RIGHT;
        }
        else if (_flags && BitmaskFlags.LEFT) {
            chirality = HandChirality.LEFT;
        }
        else {
            console.error("InputActionData missing: No Chirality found. Defaulting to 'RIGHT'");
        }

        return chirality;
    }

    // Function: GetHandTypeFromFlags
    // Used to find which <HandType> _flags contains. Favours PRIMARY if none or both are found.
    static GetHandTypeFromFlags(_flags: BitmaskFlags): HandType {
        var handType: HandType = HandType.PRIMARY;

        if (_flags && BitmaskFlags.PRIMARY) {
            handType = HandType.PRIMARY;
        }
        else if (_flags && BitmaskFlags.SECONDARY) {
            handType = HandType.SECONDARY;
        }
        else {
            console.error("InputActionData missing: No HandData found. Defaulting to 'PRIMARY'");
        }

        return handType;
    }

    // Function: GetInputTypeFromFlags
    // Used to find which <InputType> _flags contains. Favours CANCEL if none are found.
    static GetInputTypeFromFlags(_flags: BitmaskFlags): InputType {
        var inputType: InputType = InputType.CANCEL;

        if (_flags && BitmaskFlags.CANCEL) {
            inputType = InputType.CANCEL;
        }
        else if (_flags && BitmaskFlags.UP) {
            inputType = InputType.UP;
        }
        else if (_flags && BitmaskFlags.DOWN) {
            inputType = InputType.DOWN;
        }
        else if (_flags && BitmaskFlags.MOVE) {
            inputType = InputType.MOVE;
        }
        else {
            console.error("InputActionData missing: No InputType found. Defaulting to 'CANCEL'");
        }

        return inputType;
    }

    // Function: GetInteractionTypeFromFlags
    // Used to find which <InteractionType> _flags contains. Favours PUSH if none are found.
    static GetInteractionTypeFromFlags(_flags: BitmaskFlags): InteractionType {
        var interactionType: InteractionType = InteractionType.PUSH;

        if (_flags && BitmaskFlags.PUSH) {
            interactionType = InteractionType.PUSH;
        }
        else if (_flags && BitmaskFlags.HOVER) {
            interactionType = InteractionType.HOVER;
        }
        else if (_flags && BitmaskFlags.GRAB) {
            interactionType = InteractionType.GRAB;
        }
        else {
            console.error("InputActionData missing: No InteractionType found. Defaulting to 'PUSH'");
        }

        return interactionType;
    }
}