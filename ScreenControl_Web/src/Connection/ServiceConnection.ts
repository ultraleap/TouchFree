import {
    ClientInputAction,
    WebsocketInputAction
} from '../ScreenControlTypes';
import {
    ActionCode,
    CommunicationWrapper,
    ResponseCallback,
    WebSocketResponse
} from './ScreenControlServiceTypes';
import { ConnectionManager } from './ConnectionManager';

// Class: ServiceConnection
// This represents a connection to a ScreenControl Service. It should be created by a
// <ConnectionManager> to ensure there is only one active connection at a time. The sending
// and receiving of data to the client is handled here as well as the creation of a
// <MessageReceiver> to ensure the data is handled properly.
export class ServiceConnection {
    // Group: Variables

    // Event: TransmitInputAction
    // An event for transmitting <ClientInputActions> that are received via the <webSocket> to
    // be listened to.
    // 'TransmitInputAction': ClientInputActionEvent;

    // Variable: webSocket
    // A reference to the websocket we are connected to.
    webSocket: WebSocket;

    // Group: Functions

    // Function: ServiceConnection
    // The constructor for <ServiceConnection> that can be given a different IP Address and Port
    // to connect to on construction. This constructor also sets up the <receiver> for future
    // use and redirects incoming messages to <OnMessage>.
    constructor(_ip: string = "127.0.0.1", _port: string = "9739") {
       this.webSocket = new WebSocket(`ws://${_ip}:${_port}/connect`);

        // Websocket Handshake stuff needs to go here, we need to change
        // service to enable that

        this.webSocket.addEventListener('message', this.OnMessage);
    }

    // Function: Disconnect
    // Can be used to force the connection to the <webSocket> to be closed. Also destroys the
    // <receiver>.
    Disconnect(): void {
        if (this.webSocket != null) {
            this.webSocket.close();
        }
    }

    // Function: OnMessage
    // The first point of contact for new messages received, these are sorted into appropriate
    // types based on their <ActionCode> and added to queues on the <receiver>.
    OnMessage(_message: MessageEvent): void {
        let looseData: CommunicationWrapper<any> = JSON.parse(_message.data);

        switch (looseData.action) {
            case ActionCode.INPUT_ACTION:
                let wsInput: WebsocketInputAction = looseData.content;
                ConnectionManager.messageReceiver.actionQueue.push(wsInput);
                break;

            case ActionCode.CONFIGURATION_STATE:
                break;

            case ActionCode.CONFIGURATION_RESPONSE:
                let response: WebSocketResponse = looseData.content;
                ConnectionManager.messageReceiver.responseQueue.push(response);
                break;
        }
    }

    // Function: HandleInputAction
    // Called by the <receiver> to relay a <ClientInputAction> that has ben received to any
    // listeners of <TransmitInputAction>.
    HandleInputAction(_action: ClientInputAction): void {
        let event: CustomEvent<ClientInputAction> = new CustomEvent<ClientInputAction>(
            'TransmitInputAction',
            { detail: _action });
        ConnectionManager.instance.dispatchEvent(event);
    }

    // Function: SendMessage
    // Used internally to send or request information from the Service via the <webSocket>. To
    // be given a pre-made _message and _requestID. Provides an asynchronous <WebSocketResponse>
    // via the _callback parameter.
    SendMessage(
        _message: string, _requestID: string,
        _callback: (detail: WebSocketResponse) => void): void {
        if (_requestID == "") {
            if (_callback != null) {
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
}