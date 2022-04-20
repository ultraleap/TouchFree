// Class: VersionInfo
// This class is used when comparing the <ApiVersion> of the Tooling and the Service.
export class VersionInfo
{
    // Group: Variables

    // Variable: ApiVersion
    // The current API version of the Tooling.
    public static readonly ApiVersion: string = "1.2.0";

    // Variable: API_HEADER_NAME
    // The name of the header we wish the Service to compare our version with.
    public static readonly API_HEADER_NAME: string = "TfApiVersion";
}

// Class: TouchFreeInputAction
// A structure representing the Tooling verison of an InputAction. This is used to pass
// key information relating to an action that has happened on the Service.
export class TouchFreeInputAction {
    Timestamp: number;
    InteractionType: InteractionType;
    HandType: HandType;
    Chirality: HandChirality;
    InputType: InputType;
    CursorPosition: Array<number>;
    DistanceFromScreen: number;
    ProgressToClick: number;

    constructor(
        _timestamp: number,
        _interactionType: InteractionType,
        _handType: HandType,
        _handChirality: HandChirality,
        _inputType: InputType,
        _cursorPosition: Array<number>,
        _distanceFromScreen: number,
        _progressToClick: number)
    {
        this.Timestamp = _timestamp;
        this.InteractionType = _interactionType;
        this.HandType = _handType;
        this.Chirality = _handChirality;
        this.InputType = _inputType;
        this.CursorPosition = _cursorPosition;
        this.DistanceFromScreen = _distanceFromScreen;
        this.ProgressToClick = _progressToClick;
    }
}

// Function: ConvertInputAction
// Used to translate the raw actions that come across the websocket (<WebsocketInputActions>) and
// convert them into the Tooling-friendly <TouchFreeInputAction> format.
export function ConvertInputAction(_wsInput: WebsocketInputAction): TouchFreeInputAction {
    const yPosition = window.innerHeight - (_wsInput.CursorPosition.y / window.devicePixelRatio);
    const xPosition = _wsInput.CursorPosition.x / window.devicePixelRatio;

    return new TouchFreeInputAction(
        _wsInput.Timestamp,
        FlagUtilities.GetInteractionTypeFromFlags(_wsInput.InteractionFlags),
        FlagUtilities.GetHandTypeFromFlags(_wsInput.InteractionFlags),
        FlagUtilities.GetChiralityFromFlags(_wsInput.InteractionFlags),
        FlagUtilities.GetInputTypeFromFlags(_wsInput.InteractionFlags),
        [xPosition, yPosition],
        _wsInput.DistanceFromScreen,
        _wsInput.ProgressToClick,
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
// NONE - Used to be ignored by the input system but to still receive information such as distance to screen
// CANCEL - Used to cancel the current input if an issue occurs. Particularly when a DOWN has happened before an UP
// DOWN - Used to begin a 'Touch' or a 'Drag'
// MOVE - Used to move a cursor or to perform a 'Drag' after a DOWN
// UP - Used to complete a 'Touch' or a 'Drag'
export enum InputType {
    NONE,
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
    TOUCHPLANE,
}

// Enum: TrackingServiceState
// UNAVAILABLE - The TouchFree service is not connected to the tracking service
// NO_CAMERA - The TouchFree service is connected to the tracking service but there is not a camera connected
// CONNECTED - The TouchFree service is connected to the tracking service
export enum TrackingServiceState {
    UNAVAILABLE,
    NO_CAMERA,
    CONNECTED,
}

// Enum: ConfigurationState
// NOT_LOADED - The TouchFree configuration has not been loaded
// LOADED - The TouchFree configuration has successfully been loaded
// ERRORED - The TouchFree configuration errored on load
export enum ConfigurationState {
    NOT_LOADED,
    LOADED,
    ERRORED
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
    NONE_INPUT = 16,
    CANCEL = 32,
    DOWN = 64,
    MOVE = 128,
    UP = 256,

    // Interaction Types
    GRAB = 512,
    HOVER = 1024,
    PUSH = 2048,
    TOUCHPLANE = 4096,

    // Adding elements to this list is a breaking change, and should cause at
    // least a minor iteration of the API version UNLESS adding them at the end
}

// Class: WebsocketInputAction
// The version of an InputAction received via the WebSocket. This must be converted into a
// <TouchFreeInputAction> to be used by the Tooling and can be done so via ConvertInputAction.
export class WebsocketInputAction {
    // Variable: Timestamp
    Timestamp: number;
    // Variable: InteractionFlags
    InteractionFlags: BitmaskFlags;
    // Variable: CursorPosition
    CursorPosition: any;
    // Variable: DistanceFromScreen
    DistanceFromScreen: number;
    // Variable: ProgressToClick
    ProgressToClick: number;

    constructor(
        _timestamp: number,
        _interactionFlags: BitmaskFlags,
        _cursorPosition: any,
        _distanceFromScreen: number,
        _progressToClick: number,
    ) {
        this.Timestamp = _timestamp;
        this.InteractionFlags = _interactionFlags;
        this.CursorPosition = _cursorPosition;
        this.DistanceFromScreen = _distanceFromScreen;
        this.ProgressToClick = _progressToClick;
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
        let returnVal: BitmaskFlags = BitmaskFlags.NONE;

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
            case InputType.NONE:
                returnVal ^= BitmaskFlags.NONE_INPUT;
                break;

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

            case InteractionType.TOUCHPLANE:
                returnVal ^= BitmaskFlags.TOUCHPLANE;
                break;
        }

        return returnVal;
    }

    // Function: GetChiralityFromFlags
    // Used to find which <HandChirality> _flags contains. Favours RIGHT if none or both are found.
    static GetChiralityFromFlags(_flags: BitmaskFlags): HandChirality {
        let chirality: HandChirality = HandChirality.RIGHT;

        if (_flags & BitmaskFlags.RIGHT) {
            chirality = HandChirality.RIGHT;
        }
        else if (_flags & BitmaskFlags.LEFT) {
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
        let handType: HandType = HandType.PRIMARY;

        if (_flags & BitmaskFlags.PRIMARY) {
            handType = HandType.PRIMARY;
        }
        else if (_flags & BitmaskFlags.SECONDARY) {
            handType = HandType.SECONDARY;
        }
        else {
            console.error("InputActionData missing: No HandData found. Defaulting to 'PRIMARY'");
        }

        return handType;
    }

    // Function: GetInputTypeFromFlags
    // Used to find which <InputType> _flags contains. Favours NONE if none are found.
    static GetInputTypeFromFlags(_flags: BitmaskFlags): InputType {
        let inputType: InputType = InputType.NONE;

        if (_flags & BitmaskFlags.NONE_INPUT) {
            inputType = InputType.NONE;
        }
        else if (_flags & BitmaskFlags.CANCEL) {
            inputType = InputType.CANCEL;
        }
        else if (_flags & BitmaskFlags.UP) {
            inputType = InputType.UP;
        }
        else if (_flags & BitmaskFlags.DOWN) {
            inputType = InputType.DOWN;
        }
        else if (_flags & BitmaskFlags.MOVE) {
            inputType = InputType.MOVE;
        }
        else {
            console.error("InputActionData missing: No InputType found. Defaulting to 'NONE'");
        }

        return inputType;
    }

    // Function: GetInteractionTypeFromFlags
    // Used to find which <InteractionType> _flags contains. Favours PUSH if none are found.
    static GetInteractionTypeFromFlags(_flags: BitmaskFlags): InteractionType {
        let interactionType: InteractionType = InteractionType.PUSH;

        if (_flags & BitmaskFlags.PUSH) {
            interactionType = InteractionType.PUSH;
        }
        else if (_flags & BitmaskFlags.HOVER) {
            interactionType = InteractionType.HOVER;
        }
        else if (_flags & BitmaskFlags.GRAB) {
            interactionType = InteractionType.GRAB;
        }
        else if (_flags & BitmaskFlags.TOUCHPLANE) {
            interactionType = InteractionType.TOUCHPLANE;
        }
        else {
            console.error("InputActionData missing: No InteractionType found. Defaulting to 'PUSH'");
        }

        return interactionType;
    }
}