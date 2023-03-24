import { HandDataManager } from '../../Plugins/HandDataManager';
import TouchFree from '../../TouchFree';
import { BitmaskFlags, WebsocketInputAction } from '../../TouchFreeToolingTypes';
import { intervalTest } from '../../tests/testUtils';
import { ConnectionManager } from '../ConnectionManager';
import { ServiceConnection } from '../ServiceConnection';
import { ActionCode, HandPresenceState, InteractionZoneState } from '../TouchFreeServiceTypes';
import { v4 as uuidgen } from 'uuid';

describe('MessageReceiver', () => {
    let serviceConnection: ServiceConnection | null;
    let message: string;

    const onMessage = (actionCode: ActionCode, content?: { [key: string]: unknown }, guid?: string) => {
        let requestID: string;
        if (guid) {
            requestID = guid;
        } else if (message) {
            requestID = JSON.parse(message).content.requestID;
        } else {
            requestID = uuidgen();
        }
        const messageContent = {
            requestID: requestID,
            originalRequest: message,
            status: 'Success',
            message: 'Successful Test',
            ...content,
        };
        serviceConnection?.OnMessage(
            new MessageEvent('message', {
                data: JSON.stringify({
                    action: actionCode,
                    content: messageContent,
                }),
            })
        );
        return messageContent;
    };

    const mockOpen = () => serviceConnection?.webSocket.dispatchEvent(new Event('open'));

    afterEach(() => {
        // Reset service after each test to completely reset mocks
        TouchFree.Init();
        serviceConnection = ConnectionManager.serviceConnection();
        if (serviceConnection) {
            serviceConnection.webSocket.send = jest.fn((msg) => (message = msg as string));
        }
        message = '';
        jest.restoreAllMocks();
    });

    it('should correctly handle a handshake warning', async () => {
        if (!serviceConnection) return;

        const consoleTestFn = jest.spyOn(console, 'warn');
        consoleTestFn.mockImplementation(() => {});
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const requestMock = jest.spyOn(serviceConnection as any, 'RequestHandshake');
        requestMock.mockImplementation(() => {});

        mockOpen();

        onMessage(ActionCode.VERSION_HANDSHAKE_RESPONSE, { message: 'Handshake Warning' });

        await intervalTest(() => {
            expect(consoleTestFn).toBeCalledWith('Received Handshake Warning from TouchFree:\n' + 'Handshake Warning');
        });
    });

    it('should correctly check for a handshake response without a callback', async () => {
        if (!serviceConnection) return;

        const consoleTestFn = jest.spyOn(console, 'warn');
        consoleTestFn.mockImplementation(() => {});
        const consoleMessage = message;
        const content = onMessage(ActionCode.VERSION_HANDSHAKE_RESPONSE);

        await intervalTest(() => {
            expect(consoleTestFn).toBeCalledWith(
                'Received a Handshake Response that did not match a callback.' +
                    'This is the content of the response: \n Response ID: ' +
                    content.requestID +
                    '\n Status: ' +
                    content.status +
                    '\n Message: ' +
                    content.message +
                    '\n Original request - ' +
                    consoleMessage
            );
        });
    });

    it('should correctly check for a handshake response', async () => {
        if (!serviceConnection) return;

        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const testFn = jest.spyOn(serviceConnection as any, 'ConnectionResultCallback');
        testFn.mockImplementation(() => {});
        mockOpen();

        onMessage(ActionCode.VERSION_HANDSHAKE_RESPONSE);

        await intervalTest(() => {
            expect(testFn).toBeCalledTimes(1);
        });
    });

    it('should correctly check for a response', async () => {
        if (!serviceConnection) return;
        const guid = uuidgen();

        // Test without callback
        const consoleTestFn = jest.spyOn(console, 'warn');
        consoleTestFn.mockImplementation(() => {});

        const content = onMessage(ActionCode.SERVICE_STATUS_RESPONSE, undefined, guid);
        const consoleMessage = message;

        const testFn = jest.fn();
        mockOpen();
        const guid2 = uuidgen();
        serviceConnection.SendMessage('test', guid2, testFn);

        onMessage(ActionCode.SERVICE_STATUS_RESPONSE, undefined, guid2);

        await intervalTest(() => {
            expect(testFn).toBeCalledTimes(1);
            expect(consoleTestFn).toBeCalledWith(
                'Received a Handshake Response that did not match a callback.' +
                    'This is the content of the response: \n Response ID: ' +
                    guid +
                    '\n Status: ' +
                    content.status +
                    '\n Message: ' +
                    content.message +
                    '\n Original request - ' +
                    consoleMessage
            );
        });
    });

    it('should correctly check for a config state', async () => {
        if (!serviceConnection) return;

        // Test without callback
        const consoleTestFn = jest.spyOn(console, 'warn');
        consoleTestFn.mockImplementation(() => {});

        onMessage(ActionCode.CONFIGURATION_STATE);

        // Test with callback
        const testFn = jest.fn();
        mockOpen();
        serviceConnection.RequestConfigState(testFn);

        onMessage(ActionCode.CONFIGURATION_STATE);

        await intervalTest(() => {
            expect(testFn).toBeCalledTimes(1);
            expect(consoleTestFn).toBeCalledWith('Received a ConfigState message that did not match a callback.');
        });
    });

    it('should correctly check for the service status', async () => {
        if (!serviceConnection) return;

        mockOpen();

        // Test without callback
        const eventTestFn = jest.fn();
        TouchFree.RegisterEventCallback('OnServiceStatusChange', eventTestFn);

        onMessage(ActionCode.SERVICE_STATUS);

        // Test with callback
        const testFn = jest.fn();
        serviceConnection.RequestServiceStatus(testFn);

        onMessage(ActionCode.SERVICE_STATUS);

        await intervalTest(() => {
            expect(testFn).toBeCalledTimes(1);
            expect(eventTestFn).toBeCalledTimes(1);
        });
    });

    it('should correctly check for the tracking state response', async () => {
        if (!serviceConnection) return;

        const testFn = jest.fn();
        mockOpen();
        serviceConnection.RequestTrackingState(testFn);

        onMessage(ActionCode.TRACKING_STATE);

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });

    it('should correctly check for a hand presence event', async () => {
        if (!serviceConnection) return;

        const testFn = jest.spyOn(ConnectionManager, 'HandleHandPresenceEvent');
        testFn.mockImplementation(() => {});
        mockOpen();

        onMessage(ActionCode.HAND_PRESENCE_EVENT, { state: HandPresenceState.HAND_FOUND });

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });

    it('should correctly check for an interaction zone event', async () => {
        if (!serviceConnection) return;

        const testFn = jest.spyOn(ConnectionManager, 'HandleInteractionZoneEvent');
        testFn.mockImplementation(() => {});
        mockOpen();

        onMessage(ActionCode.INTERACTION_ZONE_EVENT, { state: InteractionZoneState.HAND_ENTERED });

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });

    it('should correctly check for an input action', async () => {
        if (!serviceConnection) return;

        // const interactionZoneTest = jest.spyOn(ConnectionManager, 'HandleInteractionZoneEvent');
        // interactionZoneTest.mockImplementation(() => {});
        const testFn = jest.fn();
        TouchFree.RegisterEventCallback('TransmitInputAction', testFn);
        mockOpen();

        const action = new WebsocketInputAction(
            Date.now(),
            BitmaskFlags.LEFT + BitmaskFlags.PRIMARY + BitmaskFlags.MOVE + BitmaskFlags.PUSH,
            { x: 0, y: 0 },
            0,
            0
        );

        onMessage(ActionCode.INPUT_ACTION, { ...action });

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });

    it('should correctly check for hand data', async () => {
        const testFn = jest.spyOn(HandDataManager, 'HandleHandFrame');
        testFn.mockImplementation(() => {});
        mockOpen();

        serviceConnection?.OnMessage(new MessageEvent('message', { data: [1, 0, 0, 0] }));

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });
});
