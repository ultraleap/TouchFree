import { Given, When, Then, After } from "@cucumber/cucumber";
import { callbackOnConfigurationErrorResonse, callbackOnConfigurationFileState, callbackOnConfigurationState, callbackOnHandshake, callbackOnServiceStatus, callbackOnTrackingServiceStatus, openWebSocketAndPerformAction, reset, sendHandshake, sendMessage } from './connectionAndSendingMethods';

After(function() {
    const world = this;
    reset(world);
});

Given('the Service is running', function(callback) {
    const world = this;
    openWebSocketAndPerformAction(world, () => {
        callback();
    });
});

Given('the Service is connected', function(callback) {
    const world = this;
    openWebSocketAndPerformAction(world, () => {
        callback();
    });
});

When('a handshake message is sent',  function() {
    const world = this;
    sendHandshake(world);
});

Then('a handshake response is received', function(callback) {
    const world = this;
    callbackOnHandshake(world, callback);
});

Given('the Service is connected with handshake', function(callback) {
    const world = this;
    openWebSocketAndPerformAction(world, () => {
        sendHandshake(world);
        callbackOnHandshake(world, callback);
    });
});

When('service status is requested',  function(callback) {
    const world = this;
    const serviceStatusMessage = {
        action: 'REQUEST_SERVICE_STATUS',
        content: {
            requestID: ''
        },
    };

    setTimeout(() => {
        sendMessage(world, serviceStatusMessage, true);
        callback();
    }, 300);
});

Then('a service status response is received', function(callback) {
    const world = this;
    callbackOnServiceStatus(world, callback);
});

When('configuration is requested',  function() {
    const world = this;
    const configurationRequestMessage = {
        action: 'REQUEST_CONFIGURATION_STATE',
        content: {
            requestID: ''
        },
    };

    sendMessage(world, configurationRequestMessage, true);
});

Then('a configuration response is received', function(callback) {
    const world = this;
    callbackOnConfigurationState(world, callback);
});

When('file configuration is requested',  function() {
    const world = this;
    const configurationRequestMessage = {
        action: 'REQUEST_CONFIGURATION_FILE',
        content: {
            requestID: ''
        },
    };

    sendMessage(world, configurationRequestMessage, true);
});

Then('a configuration file response is received', function(callback) {
    const world = this;
    callbackOnConfigurationFileState(world, callback);
});

When('configuration is requested without a requestID',  function() {
    const world = this;
    const configurationRequestMessage = {
        action: 'REQUEST_CONFIGURATION_STATE',
        content: {
            requestID: ''
        },
    };

    sendMessage(world, configurationRequestMessage, false);
});

Then('a configuration error response is received', function(callback) {
    const world = this;
    callbackOnConfigurationErrorResonse(world, callback);
});

When('tracking service status is requested',  function() {
    const world = this;
    const serviceStatusMessage = {
        action: 'GET_TRACKING_STATE',
        content: {
            requestID: ''
        },
    };

    sendMessage(world, serviceStatusMessage, true);
});

Then('a tracking service status response is received', function(callback) {
    const world = this;
    callbackOnTrackingServiceStatus(world, callback);
});