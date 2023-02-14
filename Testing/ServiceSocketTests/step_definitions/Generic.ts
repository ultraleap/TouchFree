import { Given, When, Then, After } from "@cucumber/cucumber";
import {
    callbackOnConfigurationErrorResonse,
    callbackOnConfigurationFileState,
    callbackOnConfigurationState,
    callbackOnConfigurationStateWithInteractionDistance,
    callbackOnHandshake,
    callbackOnHandshakeWithError, 
    callbackOnServiceStatus, 
    majorVersionDecrease, 
    majorVersionIncrease, 
    minorVersionDecrease, 
    minorVersionIncrease, 
    patchVersionChange, 
    openWebSocketAndPerformAction, 
    reset, 
    sendHandshake, 
    sendMessage
} from './connectionAndSendingMethods';

After(function () {
    const world = this;
    reset(world);
});

Given('the Service is running', function (callback) {
    const world = this;
    openWebSocketAndPerformAction(world, () => {
        callback();
    });
});

Given('the Service is connected', function (callback) {
    const world = this;
    openWebSocketAndPerformAction(world, () => {
        callback();
    });
});

When('a handshake message is sent', function () {
    const world = this;
    sendHandshake(world);
});

Then('a handshake response is received', function (callback) {
    const world = this;
    callbackOnHandshake(world, callback);
});

When('a handshake message is sent with a {string} {string} version', function (difference: string, versionType: string) {
    const world = this;
    let version = '';
    if (difference === 'newer') {
        if (versionType === 'major') {
            version = majorVersionIncrease;
        } else if (versionType === 'minor') {
            version = minorVersionIncrease;
        } else {
            version = patchVersionChange;
        }
    } else {
        if (versionType === 'major') {
            version = majorVersionDecrease;
        } else if (versionType === 'minor') {
            version = minorVersionDecrease;
        } else {
            version = patchVersionChange;
        }
    }
    sendHandshake(world, version);
});

Then('a handshake response is received with a warning for versions', function (callback) {
    const world = this;
    callbackOnHandshake(world, callback, 'Handshake Warning:');
});

Then('a handshake response is received with a version error', function (callback) {
    const world = this;
    callbackOnHandshakeWithError(world, callback);
});

Given('the Service is connected with handshake', function (callback) {
    const world = this;
    openWebSocketAndPerformAction(world, () => {
        sendHandshake(world);
        callbackOnHandshake(world, callback);
    });
});

When('service status is requested', function (callback) {
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

Then('a service status response is received', function (callback) {
    const world = this;
    callbackOnServiceStatus(world, callback);
});

When('configuration is requested', function () {
    const world = this;
    const configurationRequestMessage = {
        action: 'REQUEST_CONFIGURATION_STATE',
        content: {
            requestID: ''
        },
    };

    sendMessage(world, configurationRequestMessage, true);
});

Then('a configuration response is received', function (callback) {
    const world = this;
    callbackOnConfigurationState(world, callback);
});

When('file configuration is requested', function () {
    const world = this;
    const configurationRequestMessage = {
        action: 'REQUEST_CONFIGURATION_FILE',
        content: {
            requestID: ''
        },
    };

    sendMessage(world, configurationRequestMessage, true);
});

Then('a configuration file response is received', function (callback) {
    const world = this;
    callbackOnConfigurationFileState(world, callback);
});

When('configuration is requested without a requestID', function () {
    const world = this;
    const configurationRequestMessage = {
        action: 'REQUEST_CONFIGURATION_STATE',
        content: {
            requestID: ''
        },
    };

    sendMessage(world, configurationRequestMessage, false);
});

Then('a configuration error response is received', function (callback) {
    const world = this;
    callbackOnConfigurationErrorResonse(world, callback);
});

When('the configuration is set', function() {
    const world = this;

    const configuredInteractionDistanceCm = Math.random();
    world.configuredInteractionDistanceCm = configuredInteractionDistanceCm;

    const configurationRequestMessage = {
        action: 'SET_CONFIGURATION_STATE',
        content: {
            requestID: '',
            interaction: {
                InteractionMinDistanceCm: configuredInteractionDistanceCm
            }
        },
    };

    sendMessage(world, configurationRequestMessage, true);
})

Then('a configuration response is received with InteractionDistance', function (callback) {
    const world = this;
    callbackOnConfigurationStateWithInteractionDistance(world, () => {
        const configurationRequestMessage = {
            action: 'REQUEST_CONFIGURATION_STATE',
            content: {
                requestID: ''
            },
        };
        sendMessage(world, configurationRequestMessage, true);
        callbackOnConfigurationState(world, () => {
            const differenceInValue = Math.abs(world.messageContent.content.interaction.InteractionMinDistanceCm - world.configuredInteractionDistanceCm);
            if (differenceInValue < 0.00001) {
                callback();
            } else {
                throw new Error("InteractionMinDistanceCm does not match");
            }
        });
    });
});
