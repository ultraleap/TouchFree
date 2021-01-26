export class MessageReciever {

    actionQueue: Array<ClientInputAction> = [];
    responseQueue: Array<WebSocketResponse> = [];

    CheckForAction(): void {
        // Trim the actionQueue down
        // try send one to ServiceConnection.HandleInputAction
    }
}