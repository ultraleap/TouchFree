import { ConnectionManager } from "./ConnectionManager";
import { MessageReceiver } from "./MessageReceiver";
import * as TouchFreeServiceTypes from "./TouchFreeServiceTypes";
import { ServiceConnection } from "./ServiceConnection";

let Connection = {
    ConnectionManager: ConnectionManager,
    MessageReceiver: MessageReceiver,
    TouchFreeServiceTypes: TouchFreeServiceTypes,
    ServiceConnection: ServiceConnection
}

export default Connection;