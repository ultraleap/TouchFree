import { Given, When, Then, After } from '@cucumber/cucumber';

const WebSocket = require('ws');

let connectedWebSocket: WebSocket | undefined = undefined;
const responses: MessageEvent[] = [];
let responsesSetUp = false;

Given('the Service is running', (callback) => {
    openWebSocketAndPerformAction(() => {
        if (connectedWebSocket) {
            connectedWebSocket.close();
        }
        callback();
    });
});

Given('the Service is connected', (callback) => {
    openWebSocketAndPerformAction(() => {
        callback();
    });
});

When('a handshake message is sent',  () => {
    sendHandshake();
});

Then('a handshake response is received', (callback) => {
    callbackOnHandshake(callback);
});

Given('the Service is connected with handshake', (callback) => {
    openWebSocketAndPerformAction(() => {
        sendHandshake();
    });

    callbackOnHandshake(callback);
});

When('service status is requested',  () => {
    const serviceStatusMessage = {
        action: 'REQUEST_SERVICE_STATUS',
        content: {
            requestID: '6423d82e-3266-4830-82d8-c46cc17fc646'
        },
    };

    sendMessage(serviceStatusMessage);
});

Then('a service status response is received', (callback) => {
    callbackOnServiceStatus(callback);
});

When('configuration is requested',  () => {
    const configurationRequestMessage = {
        action: 'REQUEST_CONFIGURATION_STATE',
        content: {
            requestID: '6423d82e-3266-4830-82d8-c46cc17fc647'
        },
    };

    sendMessage(configurationRequestMessage);
});

Then('a configuration response is received', (callback) => {
    callbackOnConfigurationState(callback);
});

When('file configuration is requested',  () => {
    const configurationRequestMessage = {
        action: 'REQUEST_CONFIGURATION_FILE',
        content: {
            requestID: '6423d82e-3266-4830-82d8-c46cc17fc648'
        },
    };

    sendMessage(configurationRequestMessage);
});

Then('a configuration file response is received', (callback) => {
    callbackOnConfigurationFileState(callback);
});

When('configuration is requested without a requestID',  () => {
    const configurationRequestMessage = {
        action: 'REQUEST_CONFIGURATION_STATE',
        content: {
            requestID: ''
        },
    };

    sendMessage(configurationRequestMessage);
});

Then('a configuration error response is received', (callback) => {
    callbackOnConfigurationErrorResonse(callback);
});

When('tracking service status is requested',  () => {
    const serviceStatusMessage = {
        action: 'GET_TRACKING_STATE',
        content: {
            requestID: '6423d82e-3266-4830-82d8-c46cc17fc649'
        },
    };

    sendMessage(serviceStatusMessage);
});

Then('a tracking service status response is received', (callback) => {
    callbackOnTrackingServiceStatus(callback);
});

After(() => {
    if (connectedWebSocket && connectedWebSocket.readyState === connectedWebSocket.OPEN) {
        connectedWebSocket.close();
    }
    responsesSetUp = false;
});

const openWebSocketAndPerformAction = (callback: () => void) => {
    // attempt to connect via WS.
    // if connection unsuccessful, fail

    connectedWebSocket = new WebSocket("ws://127.0.0.1:9739/connect");

    if (connectedWebSocket) {
        connectedWebSocket.addEventListener('open', () => {
            callback();
        });
    }
}

const sendMessage = (message: unknown) => {
    if (connectedWebSocket) {
        if (!responsesSetUp) {
            connectedWebSocket.addEventListener('message', (_message: MessageEvent) => {
                responses.push(_message);
            });
            responsesSetUp = true;
        }

        connectedWebSocket.send(JSON.stringify(message));
    }
}

const sendHandshake = () => {
    const handshakeMessage = {
        action: 'VERSION_HANDSHAKE',
        content: {
            requestID: '6423d82e-3266-4830-82d8-c46cc17fc645',
            TfApiVersion: '1.3.0'
        },
    };

    sendMessage(handshakeMessage);
};

const callbackOnMessage = (callback: () => void, validation: (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => void) => {
    let checkTime = 0;
    const interval = 10;

    const intervalId: NodeJS.Timer = setInterval(() => {
        checkTime += interval;

        const response = responses.shift();
        if (response && typeof response.data === 'string') {
            validation(response.data, intervalId, callback);
        }

        if (checkTime > 3000) {
            clearInterval(intervalId);
            throw Error('No message received');
        }
    }, interval);
};

const callbackOnHandshake = (callback: () => void) => {
    callbackOnMessage(callback, (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => {
        const expectedResponse = { 
            action: 'VERSION_HANDSHAKE_RESPONSE',
            content: {
                requestID: '6423d82e-3266-4830-82d8-c46cc17fc645',
                status: 'Success',
                message: 'Handshake Successful.',
                originalRequest: '{"requestID":"6423d82e-3266-4830-82d8-c46cc17fc645","TfApiVersion":"1.3.0"}',
                touchFreeVersion: '',
                apiVersion: '1.3.0'
            }
        };

        checkActionResponse(responseData, expectedResponse, intervalId, (received: any) => {
            return received.content.status === expectedResponse.content.status &&
                received.content.message === expectedResponse.content.message;
        }, callback, 'Handshake message does not match expected');
    });
};

const callbackOnServiceStatus = (callback: () => void) => {
    callbackOnMessage(callback, (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => {
        const expectedResponse = { 
            action: 'SERVICE_STATUS',
            content: {
                requestID: '6423d82e-3266-4830-82d8-c46cc17fc646'
            }
        };

        checkActionResponse(responseData, expectedResponse, intervalId, () => {
            return true;
        }, callback, 'Service Status message does not match expected');
    });
};

const callbackOnConfigurationState = (callback: () => void) => {
    callbackOnMessage(callback, (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => {
        const expectedResponse = { 
            action: 'CONFIGURATION_STATE',
            content: {
                requestID: '6423d82e-3266-4830-82d8-c46cc17fc647'
            }
        };

        checkActionResponse(responseData, expectedResponse, intervalId, () => {
            return true;
        }, callback, 'Configuration State message does not match expected');
    });
};

const callbackOnConfigurationFileState = (callback: () => void) => {
    callbackOnMessage(callback, (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => {
        const expectedResponse = { 
            action: 'CONFIGURATION_FILE_STATE',
            content: {
                requestID: '6423d82e-3266-4830-82d8-c46cc17fc648'
            }
        };

        checkActionResponse(responseData, expectedResponse, intervalId, () => {
            return true;
        }, callback, 'Configuration File State message does not match expected');
    });
};

const callbackOnConfigurationErrorResonse = (callback: () => void) => {
    callbackOnMessage(callback, (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => {
        const expectedResponse = { 
            action: 'CONFIGURATION_RESPONSE',
            content: {
                status: 'Failure'
            }
        };

        checkActionResponse(responseData, expectedResponse, intervalId, (received: any) => {
            return received.content.status === expectedResponse.content.status;
        }, callback, 'Configuration Response message does not match expected');
    });
};

const callbackOnTrackingServiceStatus = (callback: () => void) => {
    callbackOnMessage(callback, (responseData: string, intervalId: NodeJS.Timer, callback: () => void) => {
        const expectedResponse = { 
            action: 'TRACKING_STATE',
            content: {
                requestID: '6423d82e-3266-4830-82d8-c46cc17fc649'
            }
        };

        checkActionResponse(responseData, expectedResponse, intervalId, (received: any) => {
            return received.content.mask && 
                received.content.allowImages && 
                received.content.cameraReversed && 
                received.content.analyticsEnabled;
        }, callback, 'Tracking Service Status message does not match expected');
    });
};

const checkActionResponse = (
    responseData: string,
    expectedResponse: any,
    intervalId: NodeJS.Timer,
    validation: (received: any) => boolean,
    callback: () => void,
    errorMessage: string) => {

    const content = JSON.parse(responseData);

    if (content.action === expectedResponse.action && content.requestID === expectedResponse.requestID)  {
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