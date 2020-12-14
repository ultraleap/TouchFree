import { Given, When, Then } from 'cucumber';
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
})