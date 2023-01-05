import { Connection, Configuration } from 'index';
import {
    ActionCode,
    CommunicationWrapper,
    ConfigState,
    PartialConfigState,
    WebSocketResponse,
} from '../Connection/TouchFreeServiceTypes';
import { InteractionConfig, PhysicalConfig } from './ConfigurationTypes';
import { v4 as uuidgen } from 'uuid';

/**
 * Provides methods for changing the configuration of the TouchFree Service.
 * @public
 */
export class ConfigurationManager {
    /**
     * Send updated configuration to the TouchFree Service
     * 
     * @remarks
     * WARNING! If a user changes ANY values via the TouchFree Service Settings UI,
     * all values set from the Tooling via this function will be discarded.
     * @param _interaction - Optional interaction config modifications to send
     * @param _physical - Optional physical config modifications to send
     * @param _callback - Optional callback confirming a response from the service
     */
    public static RequestConfigChange(
        _interaction: Partial<InteractionConfig> | null,
        _physical: Partial<PhysicalConfig> | null,
        _callback: (detail: WebSocketResponse) => void
    ): void {
        ConfigurationManager.BaseConfigChangeRequest(
            _interaction,
            _physical,
            _callback,
            ActionCode.SET_CONFIGURATION_STATE
        );
    }

    /**
     * Request active configuration state of the TouchFree Service
     * @param _callback - Callback with the requested {@link Connection.ConfigState}
     */
    public static RequestConfigState(_callback: (detail: ConfigState) => void): void {
        if (_callback === null) {
            console.error('Config state request failed. This call requires a callback.');
            return;
        }

        Connection.ConnectionManager.serviceConnection()?.RequestConfigState(_callback);
    }

    /**
     * Requests a modification to the configuration **files** used by the TouchFree Service.
     * 
     * @remarks
     * WARNING! Any changes that have been made using {@link Configuration.ConfigurationManager.RequestConfigChange}
     * by *any* connected client will be lost when changing these files.
     * The change will be applied **to the current config files directly**,
     * disregarding current active config state, and the config will be loaded from files.
     * @param _interaction - Optional interaction config modifications to send
     * @param _physical - Optional physical config modifications to send
     * @param _callback - Optional callback confirming a response from the service
     */
    public static RequestConfigFileChange(
        _interaction: Partial<InteractionConfig> | null,
        _physical: Partial<PhysicalConfig> | null,
        _callback: (detail: WebSocketResponse) => void | null
    ): void {
        ConfigurationManager.BaseConfigChangeRequest(
            _interaction,
            _physical,
            _callback,
            ActionCode.SET_CONFIGURATION_FILE
        );
    }

    private static BaseConfigChangeRequest(
        _interaction: Partial<InteractionConfig> | null,
        _physical: Partial<PhysicalConfig> | null,
        _callback: (detail: WebSocketResponse) => void | null,
        action: ActionCode
    ): void {
        const requestID = uuidgen();

        const content = new PartialConfigState(requestID, _interaction, _physical);
        const request = new CommunicationWrapper(action, content);

        const jsonContent = JSON.stringify(request);

        ConnectionManager.serviceConnection()?.SendMessage(jsonContent, requestID, _callback);
    }

    /**
     * Request configuration state of the services config files.
     * @param _callback - Callback with the requested {@link Connection.ConfigState}
     */
    public static RequestConfigFileState(_callback: (detail: ConfigState) => void): void {
        if (_callback === null) {
            console.error('Config file state request failed. This call requires a callback.');
            return;
        }

        ConnectionManager.serviceConnection()?.RequestConfigFile(_callback);
    }
}
