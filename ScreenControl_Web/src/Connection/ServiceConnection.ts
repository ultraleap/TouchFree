import {
    ClientInputAction,
    VersionInfo,
    WebsocketInputAction
} from '../ScreenControlTypes';
import {
    ActionCode,
    CommunicationWrapper,
    ConfigChangeRequest,
    ConfigState,
    ConfigStateCallback,
    ResponseCallback,
    WebSocketResponse
} from './ScreenControlServiceTypes';
import { ConnectionManager } from './ConnectionManager';
import { v4 as uuidgen } from 'uuid';
import { Guid } from 'guid-typescript';

// Class: ServiceConnection
// This represents a connection to a ScreenControl Service. It should be created by a
// <ConnectionManager> to ensure there is only one active connection at a time. The sending
// and receiving of data to the client is handled here as well as the creation of a
// <MessageReceiver> to ensure the data is handled properly.
export class ServiceConnection {
    // Group: Variables

    // Variable: webSocket
    // A reference to the websocket we are connected to.
    webSocket: WebSocket;

    private handshakeCompleted: boolean;

    // Group: Functions

    // Function: constructor
    // The constructor for <ServiceConnection> that can be given a different IP Address and Port
    // to connect to on construction. This constructor also sets up the redirects of incoming
    // messages to <OnMessage>. Puts a listener on the websocket so that once it opens, a handshake
    // request is sent with this Client's API version number. The service will not send data over
    // an open connection until this handshake is completed succesfully.
    constructor(_ip: string = "127.0.0.1", _port: string = "9739") {
        this.webSocket = new WebSocket(`ws://${_ip}:${_port}/connect`);

        this.webSocket.addEventListener('message', this.OnMessage);

        this.handshakeCompleted = false;

        this.webSocket.addEventListener('open', (event) => {
            let guid: string = uuidgen();

            // construct message
            let handshakeRequest: CommunicationWrapper<any> = {
                "action": ActionCode.VERSION_HANDSHAKE,
                "content": {
                    "requestID": guid
                }
            };

            handshakeRequest.content[VersionInfo.API_HEADER_NAME] = VersionInfo.ApiVersion;

            console.log("Trying to send Handshake Request");
            // send message
            this.SendMessage(JSON.stringify(handshakeRequest), guid,
                this.ConnectionResultCallback);
        });
    }

    // Function: Disconnect
    // Can be used to force the connection to the <webSocket> to be closed.
    Disconnect(): void {
        if (this.webSocket !== null) {
            this.webSocket.close();
        }
    }

    // Function: ConnectionResultCallback
    // Passed into <SendMessage> as part of connecting to ScreenControl Service, handles the
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

            case ActionCode.VERSION_HANDSHAKE_RESPONSE:
            case ActionCode.CONFIGURATION_RESPONSE:
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
    // If your _callBack requires context it should be bound to that context via .bind()
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

    // Function: SendConfigStateRequest
    // Used to request information from the Service via the <webSocket>. Provides an asynchronous
    // <ConfigState> via the _callback parameter.
    //
    // If your _callBack requires context it should be bound to that context via .bind()
    SendConfigStateRequest(_callback: (detail: ConfigState) => void): void {
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
}