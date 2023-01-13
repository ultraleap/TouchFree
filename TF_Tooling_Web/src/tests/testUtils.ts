import TouchFree from '../TouchFree';
import { TouchFreeInputAction, InteractionType, HandType, HandChirality, InputType } from '../TouchFreeToolingTypes';

export const mockTfInputAction = (input?: Partial<TouchFreeInputAction>) =>
    TouchFree.DispatchEvent(
        'TransmitInputAction',
        new TouchFreeInputAction(
            input?.Timestamp ?? Date.now(),
            input?.InteractionType ?? InteractionType.PUSH,
            input?.HandType ?? HandType.PRIMARY,
            input?.Chirality ?? HandChirality.RIGHT,
            input?.InputType ?? InputType.MOVE,
            input?.CursorPosition ?? [0, 0],
            input?.DistanceFromScreen ?? 5,
            input?.ProgressToClick ?? 0
        )
    );
