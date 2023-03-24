import TouchFree from '../../TouchFree';
import { intervalTest } from '../../tests/testUtils';
import { ConnectionManager } from '../ConnectionManager';
import { ServiceConnection } from '../ServiceConnection';
import { ActionCode } from '../TouchFreeServiceTypes';

describe('MessageReceiver', () => {
    let serviceConnection: ServiceConnection | null;
    let message: string;
    beforeAll(() => {
        TouchFree.Init();
        serviceConnection = ConnectionManager.serviceConnection();
        if (serviceConnection) {
            serviceConnection.webSocket.send = jest.fn((msg) => (message = msg as string));
        }
    });
    it('should correctly check for handshake response', async () => {
        if (!serviceConnection) return;

        const testFn = jest.spyOn(serviceConnection as any, 'ConnectionResultCallback');
        testFn.mockImplementation(() => {});
        serviceConnection.webSocket.dispatchEvent(new Event('open'));

        const { requestID } = JSON.parse(message).content;

        serviceConnection.OnMessage(
            new MessageEvent('message', {
                data: JSON.stringify({
                    action: ActionCode.VERSION_HANDSHAKE_RESPONSE,
                    content: {
                        requestID: requestID,
                        originalRequest: message,
                        status: 'Success',
                        message: 'Successful Test',
                    },
                }),
            })
        );

        await intervalTest(() => expect(testFn).toBeCalledTimes(1));
    });
});
