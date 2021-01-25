export class MessageReciever {
    // We don't need any of the ResponseCallback stuff in CfU here; Promises got us covered

    actionQueue: Array<ClientInputAction> = [];
    responseQueue: Array<WebSocketResponse> = [];

    CheckForAction(): void {
        // Trim the actionQueue down
        // try send one to ServiceConnection.HandleInputAction
    }
}