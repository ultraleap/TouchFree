import {
    BitmaskFlags,
    FlagUtilities,
    HandChirality,
    HandType,
    InputType,
    InteractionType,
} from '../TouchFreeToolingTypes';

// TODO: Generate enum values from types themselves?

const interactionTypes: InteractionType[] = [
    InteractionType.GRAB,
    InteractionType.HOVER,
    InteractionType.PUSH,
    InteractionType.TOUCHPLANE,
    InteractionType.VELOCITYSWIPE,
];
const handTypes: HandType[] = [HandType.PRIMARY, HandType.SECONDARY];
const handChiralities: HandChirality[] = [HandChirality.LEFT, HandChirality.RIGHT];
const inputTypes: InputType[] = [InputType.CANCEL, InputType.DOWN, InputType.MOVE, InputType.NONE, InputType.UP];

const allHandChiralityBitmask: BitmaskFlags = BitmaskFlags.LEFT | BitmaskFlags.RIGHT;
const allHandTypeBitmask: BitmaskFlags = BitmaskFlags.PRIMARY | BitmaskFlags.SECONDARY;
const allInputTypeBitmask: BitmaskFlags =
    BitmaskFlags.NONE_INPUT | BitmaskFlags.CANCEL | BitmaskFlags.DOWN | BitmaskFlags.MOVE | BitmaskFlags.UP;
const allInteractionTypeBitmask: BitmaskFlags =
    BitmaskFlags.GRAB | BitmaskFlags.HOVER | BitmaskFlags.PUSH | BitmaskFlags.TOUCHPLANE | BitmaskFlags.VELOCITYSWIPE;
const bitmaskFlagParams: BitmaskFlags[] = [
    // The parameters here are hand-written and non-exhaustive as the cost
    // of exhaustive combinations in maintenance and performance is excessive

    // All set
    allHandChiralityBitmask | allHandTypeBitmask | allInputTypeBitmask | allInteractionTypeBitmask,
    // Individual values
    BitmaskFlags.LEFT,
    BitmaskFlags.RIGHT,

    BitmaskFlags.PRIMARY,
    BitmaskFlags.SECONDARY,

    BitmaskFlags.NONE_INPUT,
    BitmaskFlags.CANCEL,
    BitmaskFlags.DOWN,
    BitmaskFlags.MOVE,
    BitmaskFlags.UP,

    BitmaskFlags.GRAB,
    BitmaskFlags.HOVER,
    BitmaskFlags.PUSH,
    BitmaskFlags.TOUCHPLANE,
    BitmaskFlags.VELOCITYSWIPE,
    // One of each enum
    BitmaskFlags.LEFT | BitmaskFlags.PRIMARY | BitmaskFlags.DOWN | BitmaskFlags.GRAB,
    BitmaskFlags.RIGHT | BitmaskFlags.SECONDARY | BitmaskFlags.NONE_INPUT | BitmaskFlags.TOUCHPLANE,
    BitmaskFlags.LEFT | BitmaskFlags.SECONDARY | BitmaskFlags.UP | BitmaskFlags.VELOCITYSWIPE,
    BitmaskFlags.RIGHT | BitmaskFlags.PRIMARY | BitmaskFlags.HOVER | BitmaskFlags.PUSH,
    // Multiple of same enum set
    allHandChiralityBitmask,
    allHandTypeBitmask,
    allInputTypeBitmask,
    allInteractionTypeBitmask,
    // Nothing set
    BitmaskFlags.NONE,
];

describe('BitmaskFlag', () => {
    // Suppress errors from console and store them in an array which we print only if a test fails
    const errors: string[] = [];
    const consoleErrorMock = jest.spyOn(console, 'error').mockImplementation((msg: string) => errors.push(msg));

    it('should combine the same as before', () => {
        expect(FlagUtilities.GetInteractionFlags).toVerifyAllCombinations(
            interactionTypes,
            handTypes,
            handChiralities,
            inputTypes
        );
    });

    it('should deserialize hand chirality the same as before', () => {
        expect(FlagUtilities.GetChiralityFromFlags).toVerifyAllCombinations(bitmaskFlagParams);
    });

    it('should deserialize hand type the same as before', () => {
        expect(FlagUtilities.GetHandTypeFromFlags).toVerifyAllCombinations(bitmaskFlagParams);
    });

    it('should deserialize input type the same as before', () => {
        expect(FlagUtilities.GetInputTypeFromFlags).toVerifyAllCombinations(bitmaskFlagParams);
    });

    it('should deserialize interaction type the same as before', () => {
        expect(FlagUtilities.GetInteractionTypeFromFlags).toVerifyAllCombinations(bitmaskFlagParams);
    });

    afterAll(() => {
        consoleErrorMock.mockRestore();
        // If not all tests pass then log the errors
        const JEST_MATCHER = Symbol.for('$$jest-matchers-object');
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const numberOfNonMatches: number | undefined = (global as any)[JEST_MATCHER]?.state?.snapshotState?.unmatched;
        if (numberOfNonMatches && numberOfNonMatches > 0) {
            console.log(errors);
        }
    });
});
