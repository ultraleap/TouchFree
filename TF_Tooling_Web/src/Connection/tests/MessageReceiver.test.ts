import TouchFree from '../../TouchFree';
import { intervalTest } from '../../tests/testUtils';
import { ConnectionManager } from '../ConnectionManager';
import { ServiceConnection } from '../ServiceConnection';
import { ActionCode, HandPresenceState, InteractionZoneState } from '../TouchFreeServiceTypes';
import { v4 as uuidgen } from 'uuid';

describe('MessageReceiver', () => {
    let serviceConnection: ServiceConnection | null;
    let message: string;

    const onMessage = (actionCode: ActionCode, content?: { [key: string]: unknown }, guid?: string) => {
        if (!serviceConnection) return;

        serviceConnection.OnMessage(
            new MessageEvent('message', {
                data: JSON.stringify({
                    action: actionCode,
                    content: {
                        requestID: guid ?? JSON.parse(message).content.requestID,
                        originalRequest: message,
                        status: 'Success',
                        message: 'Successful Test',
                        ...content,
                    },
                }),
            })
        );
    };

    beforeAll(() => {
        TouchFree.Init();
        serviceConnection = ConnectionManager.serviceConnection();
        if (serviceConnection) {
            serviceConnection.webSocket.send = jest.fn((msg) => (message = msg as string));
        }
    });

    it('should correctly check for a handshake response', async () => {
        if (!serviceConnection) return;

        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const testFn = jest.spyOn(serviceConnection as any, 'ConnectionResultCallback');
        testFn.mockImplementation(() => {});
        serviceConnection.webSocket.dispatchEvent(new Event('open'));

        onMessage(ActionCode.VERSION_HANDSHAKE_RESPONSE);

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });

    it('should correctly check for a response', async () => {
        if (!serviceConnection) return;

        const testFn = jest.fn();
        serviceConnection.webSocket.dispatchEvent(new Event('open'));
        const guid = uuidgen();
        serviceConnection.SendMessage('test', guid, testFn);

        onMessage(ActionCode.SERVICE_STATUS_RESPONSE, undefined, guid);

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });

    it('should correctly check for a config state', async () => {
        if (!serviceConnection) return;

        const testFn = jest.fn();
        serviceConnection.webSocket.dispatchEvent(new Event('open'));
        serviceConnection.RequestConfigState(testFn);

        onMessage(ActionCode.CONFIGURATION_STATE);

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });

    it('should correctly check for the service status', async () => {
        if (!serviceConnection) return;

        const testFn = jest.fn();
        serviceConnection.webSocket.dispatchEvent(new Event('open'));
        serviceConnection.RequestServiceStatus(testFn);

        onMessage(ActionCode.SERVICE_STATUS);

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });

    it('should correctly check for the tracking state response', async () => {
        if (!serviceConnection) return;

        const testFn = jest.fn();
        serviceConnection.webSocket.dispatchEvent(new Event('open'));
        serviceConnection.RequestTrackingState(testFn);

        onMessage(ActionCode.TRACKING_STATE);

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });

    it('should correctly check for a hand presence event', async () => {
        if (!serviceConnection) return;

        const handPresenceTest = jest.spyOn(ConnectionManager, 'HandleHandPresenceEvent');
        handPresenceTest.mockImplementation(() => {});
        serviceConnection.webSocket.dispatchEvent(new Event('open'));

        onMessage(ActionCode.HAND_PRESENCE_EVENT, { state: HandPresenceState.HAND_FOUND });

        await intervalTest(() => expect(handPresenceTest).toBeCalledTimes(1));
    });

    it('should correctly check for an interaction zone event', async () => {
        if (!serviceConnection) return;

        const interactionZoneTest = jest.spyOn(ConnectionManager, 'HandleInteractionZoneEvent');
        interactionZoneTest.mockImplementation(() => {});
        serviceConnection.webSocket.dispatchEvent(new Event('open'));

        onMessage(ActionCode.INTERACTION_ZONE_EVENT, { state: InteractionZoneState.HAND_ENTERED });

        await intervalTest(() => expect(interactionZoneTest).toBeCalledTimes(1));
    });
});
