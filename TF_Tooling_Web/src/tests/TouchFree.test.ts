import TouchFree from '../TouchFree';
import { TouchFreeEventSignatures, TouchFreeEvent } from '../TouchFreeToolingTypes';

const events: TouchFreeEventSignatures = {
    OnConnected: jest.fn(),
    WhenConnected: jest.fn(),
    OnServiceStatusChange: jest.fn(),
    OnTrackingServiceStateChange: jest.fn(),
    HandFound: jest.fn(),
    HandsLost: jest.fn(),
    InputAction: jest.fn(),
    TransmitHandData: jest.fn(),
    TransmitInputAction: jest.fn(),
    TransmitInputActionRaw: jest.fn(),
    HandEntered: jest.fn(),
    HandExited: jest.fn(),
};

describe('TouchFree', () => {
    beforeAll(() => {
        TouchFree.Init();
    });

    for (const [key, fn] of Object.entries(events)) {
        // No service connection, so testing fall-through functionality of WhenConnected instead
        if (key === 'WhenConnected') {
            it('Should pass WhenConnected callback through to OnConnected if there is no current connection', () => {
                TouchFree.RegisterEventCallback('WhenConnected', fn);
                TouchFree.DispatchEvent('OnConnected');
                expect(fn).toBeCalled();
            });
            return;
        }
        it(`Should trigger appropriate callbacks when ${key} event is dispatched`, () => {
            const newKey = key as TouchFreeEvent;
            TouchFree.RegisterEventCallback(newKey, fn);
            TouchFree.DispatchEvent(newKey);
            expect(fn).toBeCalled();
        });
    }
});
