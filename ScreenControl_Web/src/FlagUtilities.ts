import {
    BitmaskFlags,
    HandType,
    HandChirality,
    InputType,
    InteractionType
} from './ScreenControlTypes';

// Class: TypeUtilities
// A collection of Utilities to be used when working with <BitmaskFlags>.
export class TypeUtilities {
    // Group: Functions

    // Function: GetInteractionFlags
    // Used to convert a collection of interaction enums to flags for sending
    // to the Service.
    GetInteractionFlags(
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
    GetChiralityFromFlags(_flags: BitmaskFlags): HandChirality {
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
    GetHandTypeFromFlags(_flags: BitmaskFlags): HandType {
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
    GetInputTypeFromFlags(_flags: BitmaskFlags): InputType {
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
    GetInteractionTypeFromFlags(_flags: BitmaskFlags): InteractionType {
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