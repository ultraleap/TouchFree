import {
    BitmaskFlags,
    VersionInfo,
    WebsocketInputAction
} from '../TouchFreeToolingTypes';
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
    WebSocketResponse
} from './TouchFreeServiceTypes';
import { ConnectionManager } from './ConnectionManager';
import { v4 as uuidgen } from 'uuid';

// Class: ServiceConnection
// This represents a connection to a TouchFree Service. It should be created by a
// <ConnectionManager> to ensure there is only one active connection at a time. The sending
// and receiving of data to the Tooling is handled here as well as the creation of a
// <MessageReceiver> to ensure the data is handled properly.
export class ServiceConnection {
    // Group: Variables

    // Variable: webSocket
    // A reference to the websocket we are connected to.
    webSocket: WebSocket;

    private handshakeRequested: boolean;
    private handshakeCompleted: boolean;

    // Group: Functions

    // Function: constructor
    // The constructor for <ServiceConnection> that can be given a different IP Address and Port
    // to connect to on construction. This constructor also sets up the redirects of incoming
    // messages to <OnMessage>. Puts a listener on the websocket so that once it opens, a handshake
    // request is sent with this Tooling's API version number. The service will not send data over
    // an open connection until this handshake is completed succesfully.
    constructor(_ip: string = "127.0.0.1", _port: string = "9739") {
        this.webSocket = new WebSocket(`ws://${_ip}:${_port}/connect`);

        this.webSocket.addEventListener('message', this.OnMessage.bind(this));

        this.handshakeRequested = false;
        this.handshakeCompleted = false;

        this.webSocket.addEventListener('open', this.RequestHandshake.bind(this), {once: true});
    }

    // Function: Disconnect
    // Can be used to force the connection to the <webSocket> to be closed.
    Disconnect(): void {
        if (this.webSocket !== null) {
            this.webSocket.close();
        }
    }

    private RequestHandshake() {
        if (!this.handshakeCompleted) {
            let guid: string = uuidgen();

            // construct message
            let handshakeRequest: CommunicationWrapper<any> = {
                "action": ActionCode.VERSION_HANDSHAKE,
                "content": {
                    "requestID": guid
                }
            };

            handshakeRequest.content[VersionInfo.API_HEADER_NAME] = VersionInfo.ApiVersion;

            if (!this.handshakeRequested) {
                this.handshakeRequested = true;
                // send message
                this.SendMessage(JSON.stringify(handshakeRequest), guid,
                    this.ConnectionResultCallback);
            }
        }
    }

    // Function: ConnectionResultCallback
    // Passed into <SendMessage> as part of connecting to TouchFree Service, handles the
    // result of the Version Checking handshake.
    private ConnectionResultCallback(response: WebSocketResponse): void {
        if (response.status === "Success") {
            console.log("Successful Connection");

            this.handshakeCompleted = true;
            ConnectionManager.instance.dispatchEvent(new Event('OnConnected'));
        }
        else {
            console.log(`Connection to Service failed. Details:\n${response.message}`);
        }
    }

    // Function: OnMessage
    // The first point of contact for new messages received, these are sorted into appropriate
    // types based on their <ActionCode> and added to queues on the <ConnectionManager's>
    // <MessageReceiver>.
    OnMessage(_message: MessageEvent): void {
        let looseData: CommunicationWrapper<any> = JSON.parse(_message.data);

        switch (looseData.action as ActionCode) {
            case ActionCode.INPUT_ACTION:
                let wsInput: WebsocketInputAction = looseData.content;
                ConnectionManager.messageReceiver.actionQueue.push(wsInput);
                break;

            case ActionCode.CONFIGURATION_STATE:
                let configState: ConfigState = looseData.content;
                ConnectionManager.messageReceiver.configStateQueue.push(configState);
                break;

            case ActionCode.HAND_PRESENCE_EVENT:
                let handEvent: HandPresenceEvent = looseData.content;
                ConnectionManager.messageReceiver.lastStateUpdate = handEvent.state;
                break;

            case ActionCode.SERVICE_STATUS:
                let serviceStatus: ServiceStatus = looseData.content;
                ConnectionManager.messageReceiver.serviceStatusQueue.push(serviceStatus);
                break;

            case ActionCode.CONFIGURATION_FILE_STATE:
                let configFileState: ConfigState = looseData.content;
                ConnectionManager.messageReceiver.configStateQueue.push(configFileState);
                break;

            case ActionCode.CONFIGURATION_RESPONSE:
            case ActionCode.VERSION_HANDSHAKE_RESPONSE:
            case ActionCode.SERVICE_STATUS_RESPONSE:
            case ActionCode.CONFIGURATION_FILE_RESPONSE:
                let response: WebSocketResponse = looseData.content;
                ConnectionManager.messageReceiver.responseQueue.push(response);
                break;
        }
    }

    // Function: SendMessage
    // Used internally to send or request information from the Service via the <webSocket>. To
    // be given a pre-made _message and _requestID. Provides an asynchronous <WebSocketResponse>
    // via the _callback parameter.
    //
    // If your _callback requires context it should be bound to that context via .bind()
    SendMessage(
        _message: string, _requestID: string,
        _callback: (detail: WebSocketResponse) => void): void {
        if (_requestID === "") {
            if (_callback !== null) {
                let response: WebSocketResponse = new WebSocketResponse(
                    "",
                    "Failure",
                    "Request failed. This is due to a missing or invalid requestID", _message);
                _callback(response);
            }

            console.error("Request failed. This is due to a missing or invalid requestID");
            return;
        }

        if (_callback != null) {
            ConnectionManager.messageReceiver.responseCallbacks[_requestID] =
                new ResponseCallback(Date.now(), _callback);
        }

        this.webSocket.send(_message);
    }

    // Function: RequestConfigState
    // Used internally to request information from the Service via the <webSocket>.
    // Provides an asynchronous <ConfigState> via the _callback parameter.
    //
    // If your _callback requires context it should be bound to that context via .bind()
    RequestConfigState(_callback: (detail: ConfigState) => void): void {
        if (_callback === null) {
            console.error("Request failed. This is due to a missing callback");
            return;
        }

        let guid: string = uuidgen();
        let request: ConfigChangeRequest = new ConfigChangeRequest(guid);
        let wrapper: CommunicationWrapper<any> = new CommunicationWrapper<ConfigChangeRequest>(ActionCode.REQUEST_CONFIGURATION_STATE, request);
        let message: string = JSON.stringify(wrapper);

        ConnectionManager.messageReceiver.configStateCallbacks[guid] =
            new ConfigStateCallback(Date.now(), _callback);

        this.webSocket.send(message);
    }

    // Function: RequestServiceStatus
    // Used internally to request information from the Service via the <webSocket>.
    // Provides an asynchronous <ServiceStatus> via the _callback parameter.
    //
    // If your _callback requires context it should be bound to that context via .bind()
    RequestServiceStatus(_callback: (detail: ServiceStatus) => void): void {
        if (_callback === null) {
            console.error("Request failed. This is due to a missing callback");
            return;
        }

        let guid: string = uuidgen();
        let request: ServiceStatusRequest = new ServiceStatusRequest(guid);
        let wrapper: CommunicationWrapper<any> = new CommunicationWrapper<ConfigChangeRequest>(ActionCode.REQUEST_SERVICE_STATUS, request);
        let message: string = JSON.stringify(wrapper);

        ConnectionManager.messageReceiver.serviceStatusCallbacks[guid] =
            new ServiceStatusCallback(Date.now(), _callback);

        this.webSocket.send(message);
    }

    // Function: RequestConfigFile
    // Used internally to request information from the Service via the <webSocket>.
    // Provides an asynchronous <ServiceStatus> via the _callback parameter.
    //
    // If your _callback requires context it should be bound to that context via .bind()
    RequestConfigFile(_callback: (detail: ConfigState) => void): void {
        if (_callback === null) {
            console.error("Request failed. This is due to a missing callback");
            return;
        }

        let guid: string = uuidgen();
        let request: ConfigChangeRequest = new ConfigChangeRequest(guid);
        let wrapper: CommunicationWrapper<any> = new CommunicationWrapper<ConfigChangeRequest>(ActionCode.REQUEST_CONFIGURATION_FILE, request);
        let message: string = JSON.stringify(wrapper);

        ConnectionManager.messageReceiver.configStateCallbacks[guid] =
            new ConfigStateCallback(Date.now(), _callback);

        this.webSocket.send(message);
    }
}