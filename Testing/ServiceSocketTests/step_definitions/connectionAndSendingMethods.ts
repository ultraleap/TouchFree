import { IWorld } from '@cucumber/cucumber';
import { v4 as uuidgen } from 'uuid';

const WebSocket = require('ws');

export const expectedApiVersion = '1.3.0';
export const patchVersionChange = '1.3.1';
export const minorVersionDecrease = '1.2.0';
export const minorVersionIncrease = '1.4.0';
export const majorVersionDecrease = '0.3.0';
export const majorVersionIncrease = '2.3.0';

export const reset = (world: IWorld) => {
    if (world?.connectedWebSocket?.readyState === WebSocket.OPEN) {
        world.connectedWebSocket.close();
    }
}

export const openWebSocketAndPerformAction = (world: IWorld, callback: () => void) => {
    // attempt to connect via WS.
    // if connection unsuccessful, fail

    world.connectedWebSocket = new WebSocket("ws://127.0.0.1:9739/connect");

    if (world.connectedWebSocket) {
        world.connectedWebSocket.addEventListener('open', () => {
            callback();
        });
    }
}

export const sendMessage = (world: IWorld, message: any, addRequestID: boolean) => {
    if (world?.connectedWebSocket) {
        if (!world?.responsesSetUp) {
            world.responses = [];
            world.connectedWebSocket.addEventListener('message', (_message: MessageEvent) => {
                world?.responses.push(_message);
            });
            world.responsesSetUp = true;
        }

        world.requestIDSet = addRequestID && message?.content

        if (world.requestIDSet) {
            world.configuredData = {
                requestID: uuidgen().toString()
            };
            message.content.requestID = world.configuredData.requestID;
        } else {
            world.configuredData = {
                requestID: ''
            };
        }

        world.connectedWebSocket.send(JSON.stringify(message));
    }
}

export const sendHandshake = (world: IWorld, apiVersion: string = expectedApiVersion) => {
    const handshakeMessage = {
        action: 'VERSION_HANDSHAKE',
        content: {
            requestID: '',
            TfApiVersion: apiVersion
        },
    };

    sendMessage(world, handshakeMessage, true);
};

export const callbackOnMessage = (world: IWorld, callback: () => void, validation: (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => void, expectedResponse: any) => {
    let checkTime = 0;
    const interval = 10;

    const intervalId: NodeJS.Timer = setInterval(() => {
        checkTime += interval;

        const response = world.responses.shift();
        if (response && typeof response.data === 'string') {
            validation(response.data, intervalId, callback);
        }

        if (checkTime > 4000) {
            clearInterval(intervalId);
            throw Error('No message received');
        }
    }, interval);
};

const callbackOnHandshakeShared = (expectedStatus: string, expectedMessage: string, world: IWorld, callback: () => void) => {
    const expectedResponse = { 
        action: 'VERSION_HANDSHAKE_RESPONSE',
        content: {
            requestID: world?.configuredData?.requestID,
            status: expectedStatus,
            message: expectedMessage,
            originalRequest: '{"requestID":"","TfApiVersion":"1.3.0"}',
            touchFreeVersion: '',
            apiVersion: expectedApiVersion
        }
    };

    callbackOnMessage(world, callback, (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => {
        checkActionResponse(world, responseData, expectedResponse, intervalId, (received: any) => {
            return received.content.status === expectedResponse.content.status &&
                received.content.message.startsWith(expectedResponse.content.message);
        }, callback, 'Handshake message does not match expected');
    }, expectedResponse);
};

export const callbackOnHandshake = (world: IWorld, callback: () => void, successMessage: string = 'Handshake Successful.') => {
    callbackOnHandshakeShared('Success', successMessage, world, callback);
};

export const callbackOnHandshakeWithError = (world: IWorld, callback: () => void, errorMessage: string = 'Handshake Failed:') => {
    callbackOnHandshakeShared('Failure', errorMessage, world, callback);
};

const handleSimpleCallbackOnMessageCase = (action: string, errorMessage: string, world: IWorld, callback: () => void) => {
    const expectedResponse = { 
        action: action,
        content: {
            requestID: world?.configuredData?.requestID
        }
    };

    callbackOnMessage(world, callback, (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => {
        checkActionResponse(world, responseData, expectedResponse, intervalId, () => {
            return true;
        }, callback, errorMessage);
    }, expectedResponse);
};

export const callbackOnServiceStatus = (world: IWorld, callback: () => void) => {
    handleSimpleCallbackOnMessageCase('SERVICE_STATUS', 'Service Status message does not match expected', world, callback);
};

export const callbackOnConfigurationState = (world: IWorld, callback: () => void) => {
    handleSimpleCallbackOnMessageCase('CONFIGURATION_STATE', 'Configuration State message does not match expected', world, callback);
};

export const callbackOnConfigurationFileState = (world: IWorld, callback: () => void) => {
    handleSimpleCallbackOnMessageCase('CONFIGURATION_FILE_STATE', 'Configuration File State message does not match expected', world, callback);
};

export const callbackOnConfigurationErrorResonse = (world: IWorld, callback: () => void) => {
    const expectedResponse = { 
        action: 'CONFIGURATION_RESPONSE',
        content: {
            status: 'Failure'
        }
    };

    callbackOnMessage(world, callback, (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => {
        checkActionResponse(world, responseData, expectedResponse, intervalId, (received: any) => {
            return received.content.status === expectedResponse.content.status;
        }, callback, 'Configuration Response message does not match expected');
    }, expectedResponse);
};

export const callbackOnTrackingServiceStatus = (world: IWorld, callback: () => void) => {
    const expectedResponse = { 
        action: 'TRACKING_STATE',
        content: {
            requestID: world?.configuredData?.requestID
        }
    };

    callbackOnMessage(world, callback, (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => {
        checkActionResponse(world, responseData, expectedResponse, intervalId, (received: any) => {
            return received.content.mask && 
                received.content.allowImages && 
                received.content.cameraReversed && 
                received.content.analyticsEnabled;
        }, callback, 'Tracking Service Status message does not match expected');
    }, expectedResponse);
};

const checkActionResponse = (
    world: IWorld, 
    responseData: string,
    expectedResponse: any,
    intervalId: NodeJS.Timer,
    validation: (received: any) => boolean,
    callback: () => void,
    errorMessage: string) => {

    const content = JSON.parse(responseData);

    if (content.action === expectedResponse.action &&
        (!world?.requestIDSet || content.content.requestID === expectedResponse.content.requestID))  {
        clearInterval(intervalId);

        if (validation(content)) {
            callback();
        } else {
            logMessageComparisonAndThrow(content, expectedResponse, errorMessage);
        }
    }
};

const logMessageComparisonAndThrow = (received: any, expected: any, errorMessage: string) => {
    console.log('');
    console.log('Received:');
    console.log(received);
    console.log('');
    console.log('Expected:');
    console.log(expected);
    throw Error(errorMessage);
};