import TouchFree from '../TouchFree';
import { VersionInfo, WebsocketInputAction } from '../TouchFreeToolingTypes';
import { TrackingState } from '../Tracking/TrackingTypes';
import { ConnectionManager } from './ConnectionManager';
import {
    ActionCode,
    CommunicationWrapper,
    ConfigChangeRequest,
    ConfigState,
    ConfigStateCallback,
    HandPresenceEvent,
    ResponseCallback,
    ServiceStatus,
    ServiceStatusCallback,
    ServiceStatusRequest,
    SimpleRequest,
    TrackingStateCallback,
    TrackingStateRequest,
    TrackingStateResponse,
    VersionHandshakeResponse,
    WebSocketResponse,
} from './TouchFreeServiceTypes';
import { v4 as uuidgen } from 'uuid';

/**
 * Represents a connection to the TouchFree Service.
 * 
 * @remarks
 * Typically only a single instance of this class exists, managed by
 * the {@link ConnectionManager}.
 * 
 * @internal
 */
export class ServiceConnection {
    
    /** The websocket connection object */
    webSocket: WebSocket;

    private handshakeRequested: boolean;
    private handshakeCompleted: boolean;
    private _touchFreeVersion = '';

    /**
     * The version of the connected TouchFree Service
     */
    public get touchFreeVersion(): string {
        return this._touchFreeVersion;
    }

    /**
     * Has the websocket connection handshake completed?
     */
    public get handshakeComplete(): boolean {
        return this.handshakeCompleted;
    }

    /**
     * Sets up {@link WebSocket} connection and adds appropriate listeners for incoming messages.
     * 
     * @remarks
     * Sets up a listener to request a handshake once the websocket has successfully opened.
     * No data will be sent over an open connection until a successful handshake has completed.
     * 
     * @param _ip - Optional override to default websocket ip '127.0.0.1'
     * @param _port - Optional override to default websocket port '9739'
     */
    constructor(_ip = '127.0.0.1', _port = '9739') {
        this.webSocket = new WebSocket(`ws://${_ip}:${_port}/connect`);
        this.webSocket.binaryType = 'arraybuffer';

        this.webSocket.addEventListener('message', this.OnMessage);

        this.handshakeRequested = false;
        this.handshakeCompleted = false;

        this.webSocket.addEventListener('open', this.RequestHandshake, { once: true });
    }

    /**
     * Force close the websocket connection
     */
    Disconnect = (): void => {
        if (this.webSocket !== null) {
            this.webSocket.close();
        }
    };

    private RequestHandshake = () => {
        if (!this.handshakeCompleted) {
            const guid: string = uuidgen();

            // construct message
            const handshakeRequest: CommunicationWrapper<{ [key: string]: string }> = {
                action: ActionCode.VERSION_HANDSHAKE,
                content: {
                    requestID: guid,
                },
            };

            handshakeRequest.content[VersionInfo.API_HEADER_NAME] = VersionInfo.ApiVersion;

            if (!this.handshakeRequested) {
                this.handshakeRequested = true;
                // send message
                this.SendMessage(JSON.stringify(handshakeRequest), guid, this.ConnectionResultCallback);
            }
        }
    };

    /**
     * Passed into {@link SendMessage} as part of connecting to TouchFree Service, handles the
     * result of the Version Checking handshake.
     * 
     * @remarks
     * Dispatches `"OnConnected"` event via {@link TouchFree.DispatchEvent} upon successful handshake response
     * 
     * @param response - VersionHandshakeResponse if connection was successful or another websocket response otherwise
     */
    private ConnectionResultCallback = (response: VersionHandshakeResponse | WebSocketResponse): void => {
        if (response.status === 'Success') {
            console.log('Successful Connection');
            const handshakeResponse = response as VersionHandshakeResponse;
            if (handshakeResponse) {
                this._touchFreeVersion = handshakeResponse.touchFreeVersion;
            }

            this.handshakeCompleted = true;
            TouchFree.DispatchEvent('OnConnected');
        } else {
            console.log(`Connection to Service failed. Details:\n${response.message}`);
        }
    };

    /**
     * The first point of contact for new messages received. Messages are handled differently
     * based on their {@link ActionCode}, typically being sent to a queue or handler in
     * {@link ConnectionManager.messageReceiver}.
     * 
     * @param _message - Message to handle
     */
    OnMessage = (_message: MessageEvent): void => {
        if (typeof _message.data !== 'string') {
            const buffer = _message.data as ArrayBuffer;
            const binaryDataType = new Int32Array(buffer, 0, 4)[0];
            if (binaryDataType === ServiceBinaryDataTypes.HandRenderData) {
                ConnectionManager.messageReceiver.latestHandDataItem = buffer;
            }
            return;
        }

        const looseData: CommunicationWrapper<unknown> = JSON.parse(_message.data);

        switch (looseData.action as ActionCode) {
            case ActionCode.INPUT_ACTION: {
                const wsInput = looseData.content as WebsocketInputAction;
                ConnectionManager.messageReceiver.actionQueue.push(wsInput);
                break;
            }

            case ActionCode.HAND_PRESENCE_EVENT: {
                const handEvent = looseData.content as HandPresenceEvent;
                ConnectionManager.messageReceiver.lastStateUpdate = handEvent.state;
                break;
            }

            case ActionCode.SERVICE_STATUS: {
                const serviceStatus = looseData.content as ServiceStatus;
                ConnectionManager.messageReceiver.serviceStatusQueue.push(serviceStatus);
                break;
            }

            case ActionCode.CONFIGURATION_STATE:
            case ActionCode.CONFIGURATION_FILE_STATE:
            case ActionCode.QUICK_SETUP_CONFIG: {
                const configFileState = looseData.content as ConfigState;
                ConnectionManager.messageReceiver.configStateQueue.push(configFileState);
                break;
            }

            case ActionCode.CONFIGURATION_RESPONSE:
            case ActionCode.VERSION_HANDSHAKE_RESPONSE:
            case ActionCode.SERVICE_STATUS_RESPONSE:
            case ActionCode.CONFIGURATION_FILE_RESPONSE:
            case ActionCode.QUICK_SETUP_RESPONSE: {
                const response = looseData.content as WebSocketResponse;
                ConnectionManager.messageReceiver.responseQueue.push(response);
                break;
            }
            case ActionCode.TRACKING_STATE: {
                const trackingResponse = looseData.content as TrackingStateResponse;
                ConnectionManager.messageReceiver.trackingStateQueue.push(trackingResponse);
                break;
            }
        }
    };

    /**
     * Send or request information from the TouchFree Service via the WebSocket.
     * 
     * @param _message - Content of message
     * @param _requestID - A request ID to identify the response from the Service
     * @param _callback - Callback to handle the response
     */
    SendMessage = <T extends WebSocketResponse>(
        _message: string,
        _requestID: string,
        _callback: ((detail: WebSocketResponse | T) => void) | null
    ): void => {
        if (_requestID === '') {
            if (_callback !== null) {
                const response: WebSocketResponse = new WebSocketResponse(
                    '',
                    'Failure',
                    'Request failed. This is due to a missing or invalid requestID',
                    _message
                );
                _callback(response);
            }

            console.error('Request failed. This is due to a missing or invalid requestID');
            return;
        }

        if (_callback != null) {
            ConnectionManager.messageReceiver.responseCallbacks[_requestID] = new ResponseCallback(
                Date.now(),
                _callback
            );
        }

        this.webSocket.send(_message);
    };

    /**
     * Request updated {@link ConfigState} from the Service
     * 
     * @param _callback - Callback to handle the response from the service
     */
    RequestConfigState = (_callback: (detail: ConfigState) => void): void => {
        if (_callback === null) {
            console.error('Request for config state failed. This is due to a missing callback');
            return;
        }

        const guid: string = uuidgen();
        const request: ConfigChangeRequest = new ConfigChangeRequest(guid);
        const wrapper = new CommunicationWrapper<ConfigChangeRequest>(ActionCode.REQUEST_CONFIGURATION_STATE, request);
        const message: string = JSON.stringify(wrapper);

        ConnectionManager.messageReceiver.configStateCallbacks[guid] = new ConfigStateCallback(Date.now(), _callback);

        this.webSocket.send(message);
    };

    /**
     * Request service status from the Service.
     * 
     * @param _callback - Callback to handle the response from the service
     */
    RequestServiceStatus = (_callback: (detail: ServiceStatus) => void): void => {
        if (_callback === null) {
            console.error('Request for service status failed. This is due to a missing callback');
            return;
        }

        const guid: string = uuidgen();
        const request: ServiceStatusRequest = new ServiceStatusRequest(guid);
        // TODO: Change wrapper generic type - incorrectly using ConfigChangeRequest
        const wrapper = new CommunicationWrapper<ConfigChangeRequest>(ActionCode.REQUEST_SERVICE_STATUS, request);
        const message: string = JSON.stringify(wrapper);

        ConnectionManager.messageReceiver.serviceStatusCallbacks[guid] = new ServiceStatusCallback(
            Date.now(),
            _callback
        );

        this.webSocket.send(message);
    };

    /**
     * Request config state of the config files from the Service
     * 
     * @param _callback - Callback to handle the response from the service
     */
    RequestConfigFile = (_callback: (detail: ConfigState) => void): void => {
        if (_callback === null) {
            console.error('Request for config file failed. This is due to a missing callback');
            return;
        }

        const guid: string = uuidgen();
        const request: ConfigChangeRequest = new ConfigChangeRequest(guid);
        const wrapper = new CommunicationWrapper<ConfigChangeRequest>(ActionCode.REQUEST_CONFIGURATION_FILE, request);
        const message: string = JSON.stringify(wrapper);

        ConnectionManager.messageReceiver.configStateCallbacks[guid] = new ConfigStateCallback(Date.now(), _callback);

        this.webSocket.send(message);
    };

     /**
     * Request a quick setup on the Service
     * 
     * @param atTopTarget - Which quick setup target is being used
     * @param _callback - Callback to handle the response from the service
     * @param _configurationCallback - Callback to handle a response from the service with updated configuration
     */
    QuickSetupRequest = (
        atTopTarget: boolean,
        _callback: (detail: WebSocketResponse) => void,
        _configurationCallback: (detail: ConfigState) => void
    ): void => {
        const position = atTopTarget ? 'Top' : 'Bottom';
        const guid: string = uuidgen();

        const request = {
            requestID: guid,
            position,
        };
        const wrapper = new CommunicationWrapper(ActionCode.QUICK_SETUP, request);
        const message: string = JSON.stringify(wrapper);

        if (_callback !== null) {
            ConnectionManager.messageReceiver.responseCallbacks[guid] = new ResponseCallback(Date.now(), _callback);
        }

        if (_configurationCallback !== null) {
            ConnectionManager.messageReceiver.configStateCallbacks[guid] = new ConfigStateCallback(
                Date.now(),
                _configurationCallback
            );
        }

        this.webSocket.send(message);
    };

    /**
     * Request tracking state update from the Service
     * 
     * @param _callback - Callback to handle the response from the service
     */
    RequestTrackingState = (_callback: (detail: TrackingStateResponse) => void) => {
        if (!_callback) {
            console.error('Request for tracking state failed. This is due to a missing callback');
            return;
        }
        const guid: string = uuidgen();
        const request: SimpleRequest = new SimpleRequest(guid);
        const wrapper = new CommunicationWrapper<SimpleRequest>(ActionCode.GET_TRACKING_STATE, request);
        const message: string = JSON.stringify(wrapper);

        ConnectionManager.messageReceiver.trackingStateCallbacks[guid] = new TrackingStateCallback(
            Date.now(),
            _callback
        );

        this.webSocket.send(message);
    };

    /**
     * Request a change to tracking state on the Service
     * 
     * @param _state - State change to request. Undefined props are not sent
     * @param _callback - Callback to handle the response from the service
     */
    RequestTrackingChange = (
        _state: Partial<TrackingState>,
        _callback: ((detail: TrackingStateResponse) => void) | null
    ) => {
        const requestID = uuidgen();
        const requestContent: Partial<TrackingStateRequest> = {
            requestID,
        };

        if (_state.mask !== undefined) {
            requestContent.mask = _state.mask;
        }

        if (_state.allowImages !== undefined) {
            requestContent.allowImages = _state.allowImages;
        }

        if (_state.cameraReversed !== undefined) {
            requestContent.cameraReversed = _state.cameraReversed;
        }

        if (_state.analyticsEnabled !== undefined) {
            requestContent.analyticsEnabled = _state.analyticsEnabled;
        }

        const wrapper: CommunicationWrapper<Partial<TrackingStateRequest>> = new CommunicationWrapper<
            Partial<TrackingStateRequest>
        >(ActionCode.SET_TRACKING_STATE, requestContent);
        const message: string = JSON.stringify(wrapper);

        if (_callback !== null) {
            ConnectionManager.messageReceiver.trackingStateCallbacks[requestID] = new TrackingStateCallback(
                Date.now(),
                _callback
            );
        }

        this.webSocket.send(message);
    };
}

enum ServiceBinaryDataTypes {
    HandRenderData = 1,
}
