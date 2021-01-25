export class ServiceConnection {
    constructor() {
        // Connect to WebSocket
        // Invoke onMessage when getting from WS
        // Create a Reciever
    }

    OnMessage(_message: MessageEvent): void {
        // Split _message.data into Action and Content

        // Switch based on Action:
            // INPUT_ACTION:
                // Convert Content to a InputAction
                // pass to reciever.ActionQueue
            // CONFIGURATION_STATE:
                // Leave NYI
            // CONFIGURATION_RESPONSE:
                // Convert to response type
                // Pass to reciever.responseQueue
    }

    // This may be unnecessary; all of the stuff this covers in SC CfU
    SendMessage(_message: string, _requestID: string): Promise<WebSocketResponse> {
        // webSocket.send(_message);
    }
}