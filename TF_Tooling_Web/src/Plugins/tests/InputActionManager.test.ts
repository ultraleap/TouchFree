import { HandChirality, HandType, InputType, InteractionType, TouchFreeInputAction } from '../../TouchFreeToolingTypes';
import { createInputAction, mockTfPluginInputAction } from '../../tests/testUtils';
import { InputActionManager } from '../InputActionManager';
import { InputActionPlugin } from '../InputActionPlugin';

describe('InputActionManager', () => {
    test('Check plugin gets called with the correct data', () => {
        let currentInputAction: TouchFreeInputAction;
        let currentModifiedAction: TouchFreeInputAction;
        let pluginCallCount = 0;

        class MockPlugin extends InputActionPlugin {
            RunPlugin(inputAction: TouchFreeInputAction): TouchFreeInputAction | null {
                expect(inputAction).toStrictEqual(currentInputAction);
                const modifiedInputAction = super.RunPlugin(inputAction);
                expect(modifiedInputAction).toStrictEqual(currentModifiedAction);
                pluginCallCount++;
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
        InputActionManager.SetPlugins([new MockPlugin()]);
        expect(pluginCallCount).toBe(0);
        currentInputAction = createInputAction();
        mockTfPluginInputAction(currentInputAction);
        expect(pluginCallCount).toBe(1);

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
        expect(pluginCallCount).toBe(2);
    });

    test('Check plugins gets called in the correct order', () => {
        let pluginCallCount = 0;

        class MockOrderPlugin extends InputActionPlugin {
            private orderNumber: number;

            constructor(orderNumber: number) {
                super();
                this.orderNumber = orderNumber;
            }

            RunPlugin(inputAction: TouchFreeInputAction): TouchFreeInputAction | null {
                expect(pluginCallCount).toBe(this.orderNumber);
                pluginCallCount++;
                return inputAction;
            }
        }

        InputActionManager.SetPlugins([
            new MockOrderPlugin(0),
            new MockOrderPlugin(1),
            new MockOrderPlugin(2),
            new MockOrderPlugin(3),
        ]);
        expect(pluginCallCount).toBe(0);
        mockTfPluginInputAction();
        expect(pluginCallCount).toBe(4);
    });
});
