import { InputActionManager } from '../Plugins/InputActionManager';
import TouchFree from '../TouchFree';
import { TouchFreeInputAction, InteractionType, HandType, HandChirality, InputType } from '../TouchFreeToolingTypes';

export const createInputAction = (input?: Partial<TouchFreeInputAction>) =>
    new TouchFreeInputAction(
        input?.Timestamp ?? Date.now(),
        input?.InteractionType ?? InteractionType.PUSH,
        input?.HandType ?? HandType.PRIMARY,
        input?.Chirality ?? HandChirality.RIGHT,
        input?.InputType ?? InputType.MOVE,
        input?.CursorPosition ?? [0, 0],
        input?.DistanceFromScreen ?? 5,
        input?.ProgressToClick ?? 0
    );

export const mockTfInputAction = (input?: Partial<TouchFreeInputAction>) =>
    TouchFree.DispatchEvent('TransmitInputAction', createInputAction(input));

export const mockTfPluginInputAction = (input?: Partial<TouchFreeInputAction>) =>
    InputActionManager.HandleInputAction(createInputAction(input));

export const intervalTest = async (test: () => unknown) => {
    await new Promise<void>((resolve, reject) => {
        let time = 0;
        const interval = setInterval(() => {
            try {
                test();
                clearInterval(interval);
                resolve();
            } catch (e) {
                if (time > 1000) {
                    clearInterval(interval);
                    reject(e);
                }
                time += 20;
            }
        }, 20);
    });
};
