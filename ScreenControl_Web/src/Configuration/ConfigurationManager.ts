import {
    InteractionConfig,
    PhysicalConfig,
    HoverAndHoldInteractionSettings
} from "./ConfigurationTypes";
import {
    ActionCode,
    CommunicationWrapper,
    ConfigState,
    ResponseCallback,
    WebSocketResponse
} from '../Connection/ScreenControlServiceTypes';
import { ConnectionManager } from '../Connection/ConnectionManager';
import { Guid } from "guid-typescript";

// Class: ConfigurationManager
// This class provides a method for changing the configuration of the ScreenControl
// Service. Makes use of the static <ConnectionManager> for communication with the Service.
export class ConfigurationManager {

    // Function: SetConfigState
    // Optionally takes in an <InteractionConfig> or a <PhysicalConfig> and sends them through the <ConnectionManager>
    // 
    // Provide a _callBack if you require confirmation that your settings were used correctly.
    // If your _callBack requires context it should be bound to that context via .bind().
    public static SetConfigState(
        _interaction: Partial<InteractionConfig> | null,
        _physical: Partial<PhysicalConfig> | null,
        _callback: (detail: WebSocketResponse) => void): void {

        let action = ActionCode.SET_CONFIGURATION_STATE;
        let requestID = Guid.create().toString();

        let content = new ConfigState(requestID, _interaction, _physical);
        let request = new CommunicationWrapper(action, content);

        let jsonContent = JSON.stringify(request);

        ConnectionManager.serviceConnection()?.SendMessage(jsonContent, requestID, _callback);
    }
}