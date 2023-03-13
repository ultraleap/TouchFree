import { HandChirality, HandType, InputType, InteractionType, TouchFreeInputAction } from '../../TouchFreeToolingTypes';
import { createInputAction, mockTfPluginInputAction } from '../../tests/testUtils';
import { InputActionManager } from '../InputActionManager';
import { InputActionPlugin } from '../InputActionPlugin';

describe('InputActionManager', () => {
    let currentInputAction: TouchFreeInputAction;
    let currentModifiedAction: TouchFreeInputAction;
    let pluginCalled = false;

    class MockPlugin extends InputActionPlugin {
        RunPlugin(inputAction: TouchFreeInputAction): TouchFreeInputAction | null {
            expect(inputAction).toStrictEqual(currentInputAction);
            const modifiedInputAction = super.RunPlugin(inputAction);
            expect(modifiedInputAction).toStrictEqual(currentModifiedAction);
            pluginCalled = true;
            return modifiedInputAction;
        }

        ModifyInputAction(inputAction: TouchFreeInputAction): TouchFreeInputAction | null {
            currentModifiedAction = { ...inputAction, Timestamp: Date.now() };
            return currentModifiedAction;
        }

        TransmitInputAction(inputAction: TouchFreeInputAction): void {
            expect(inputAction).toStrictEqual(currentModifiedAction);
        }
    }

    beforeAll(() => {
        InputActionManager.SetPlugins([new MockPlugin()]);
    });

    test('Check plugin gets called with the correct data', () => {
        expect(pluginCalled).toBeFalsy();
        currentInputAction = createInputAction();
        mockTfPluginInputAction(currentInputAction);
        expect(pluginCalled).toBeTruthy();
        pluginCalled = false;

        expect(pluginCalled).toBeFalsy();
        currentInputAction = new TouchFreeInputAction(
            477777,
            InteractionType.TOUCHPLANE,
            HandType.SECONDARY,
            HandChirality.RIGHT,
            InputType.DOWN,
            [332, 455],
            20,
            80
        );
        mockTfPluginInputAction(currentInputAction);
        expect(pluginCalled).toBeTruthy();
    });
});
