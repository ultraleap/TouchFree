import { ConnectionManager } from '../Connection/ConnectionManager';
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
            TouchFree.Init();
            const newKey = key as TouchFreeEvent;
            TouchFree.RegisterEventCallback(newKey, fn);
            TouchFree.DispatchEvent(newKey);
            expect(fn).toBeCalled();
        });
    }

    it('Should pass a given address to the ConnectionManager', () => {
        const newAddress = { ip: '192.168.0.1', port: '8080' };
        TouchFree.Init({ address: newAddress });
        expect(ConnectionManager.iPAddress).toBe(newAddress.ip);
        expect(ConnectionManager.port).toBe(newAddress.port);
    });
});
