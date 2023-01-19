import { Given, When, Then, AfterAll } from '@cucumber/cucumber';
import { expect } from 'chai';
import { callbackify } from 'util';

var WebSocket = require('ws');

Given('the Service is running', function(callback) {
    // attempt to connect via WS.
    // if connection unsuccessful, fail

    let serviceConnection: WebSocket = new WebSocket("ws://127.0.0.1:9739/connect");

    serviceConnection.addEventListener('open', function () {
        serviceConnection.close();

        callback();
    });
});

let connectedWebSocket: WebSocket | undefined = undefined;

Given('the Service is connected', function(callback) {
    // attempt to connect via WS.
    // if connection unsuccessful, fail

    connectedWebSocket = new WebSocket("ws://127.0.0.1:9739/connect");

    if (connectedWebSocket) {
        connectedWebSocket.addEventListener('open', function () {
            callback();
        });
    }
});

let responses: MessageEvent[] = [];

When('a handshake message is sent',  (callback) => {
    const handshakeMessage = {
        action: 'VERSION_HANDSHAKE',
        content: {
            requestID: '6423d82e-3266-4830-82d8-c46cc17fc645',
            TfApiVersion: '1.3.0'
        },
    };

    if (connectedWebSocket) {
        connectedWebSocket.addEventListener('message', (_message: MessageEvent) => {
            responses.push(_message);
        });

        connectedWebSocket.send(JSON.stringify(handshakeMessage));
    }

    callback();
});

Then('a handshake response is received', (callback) => {
    let checkTime = 0;
    const interval = 10;

    const intervalId = setInterval(() => {
        checkTime += interval;

        const response = responses.shift();
        if (response) {
            const content = JSON.parse(response.data);

            clearInterval(intervalId);

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

            if (content.action === expectedResponse.action &&
                content.content.requestID === expectedResponse.content.requestID &&
                content.content.status === expectedResponse.content.status &&
                content.content.message === expectedResponse.content.message) {
                callback();
            } else {
                console.log('');
                console.log('Received:');
                console.log(content);
                console.log('');
                console.log('Expected:');
                console.log(expectedResponse);
                throw Error('Handshake message does not match expected');
            }
        }

        if (checkTime > 3000) {
            clearInterval(intervalId);
            throw Error('No message received');
        }
    }, interval);
});

AfterAll(() => {
    if (connectedWebSocket && connectedWebSocket.readyState === connectedWebSocket.OPEN) {
        connectedWebSocket.close();
    }
});