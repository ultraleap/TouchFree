import {
    InteractionConfig,
    PhysicalConfig,
} from "./ConfigurationTypes";
import {
    ActionCode,
    CommunicationWrapper,
    ConfigState,
    PartialConfigState,
    WebSocketResponse
} from '../Connection/TouchFreeServiceTypes';
import { ConnectionManager } from '../Connection/ConnectionManager';
import { v4 as uuidgen } from 'uuid';

// Class: ConfigurationManager
// This class provides a method for changing the configuration of the TouchFree
// Service. Makes use of the static <ConnectionManager> for communication with the Service.
export class ConfigurationManager {

    // Function: RequestConfigChange
    // Optionally takes in an <InteractionConfig> or a <PhysicalConfig> and sends them through the <ConnectionManager>
    //
    // Provide a _callback if you require confirmation that your settings were used correctly.
    // If your _callback requires context it should be bound to that context via .bind().
    //
    // WARNING!
    // If a user changes ANY values via the TouchFree Service Settings UI,
    // values set from the Tooling via this function will be discarded.
    public static RequestConfigChange(
        _interaction: Partial<InteractionConfig> | null,
        _physical: Partial<PhysicalConfig> | null,
        _callback: (detail: WebSocketResponse) => void): void {
        ConfigurationManager.BaseConfigChangeRequest(_interaction, _physical, _callback, ActionCode.SET_CONFIGURATION_STATE);
    }

    // Function: RequestConfigState
    // Used to request information from the Service via the <ConnectionManager>. Provides an asynchronous
    // <ConfigState> via the _callback parameter.
    //
    // If your _callback requires context it should be bound to that context via .bind()
    public static RequestConfigState(_callback: (detail: ConfigState) => void): void {
        if (_callback === null) {
            console.error("Config state request failed. This call requires a callback.");
            return;
        }

        ConnectionManager.serviceConnection()?.RequestConfigState(_callback);
    }

    // Function: RequestConfigFileChange
    // Requests a modification to the configuration **files** used by the Service. Takes in an
    // <InteractionConfig> and/or a <PhysicalConfig> representing the desired changes & sends
    // them through the <ConnectionManager>
    //
    // Provide a _callback if you require confirmation that your settings were used correctly.
    // If your _callback requires context it should be bound to that context via .bind().
    //
    // WARNING!
    // Any changes that have been made using <RequestConfigChange> by *any* connected client will be
    // lost when changing these files. The change will be applied **to the current config files directly,**
    // disregarding current active config state, and the config will be loaded from files.
    public static RequestConfigFileChange(
        _interaction: Partial<InteractionConfig> | null,
        _physical: Partial<PhysicalConfig> | null,
        _callback: (detail: WebSocketResponse) => void): void {
        ConfigurationManager.BaseConfigChangeRequest(_interaction, _physical, _callback, ActionCode.SET_CONFIGURATION_FILE);
    }

    private static BaseConfigChangeRequest(
        _interaction: Partial<InteractionConfig> | null,
        _physical: Partial<PhysicalConfig> | null,
        _callback: (detail: WebSocketResponse) => void,
        action: ActionCode): void {

        let requestID = uuidgen();

        let content = new PartialConfigState(requestID, _interaction, _physical);
        let request = new CommunicationWrapper(action, content);

        let jsonContent = JSON.stringify(request);

        ConnectionManager.serviceConnection()?.SendMessage(jsonContent, requestID, _callback);
    }

    // Function: RequestConfigState
    // Used to request a <ConfigState> representing the current state of the Service's config
    // files via the WebSocket.
    // Provides a <ConfigState> asynchronously via the _callback parameter.
    //
    // If your _callback requires context it should be bound to that context via .bind()
    public static RequestConfigFileState(_callback: (detail: ConfigState) => void): void {
        if (_callback === null) {
            console.error("Config file state request failed. This call requires a callback.");
            return;
        }

        ConnectionManager.serviceConnection()?.RequestConfigFile(_callback);
    }
}