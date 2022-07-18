import { ConnectionManager } from '../Connection/ConnectionManager';
import {
    ActionCode,
    CommunicationWrapper,
    PartialTrackingState,
    TrackingState,
    WebSocketResponse,
} from '../Connection/TouchFreeServiceTypes';
import { v4 as uuidgen } from 'uuid';
import { Mask } from './TrackingTypes';

// class: TrackingManager
// This class provides methods for getting and setting the settings of the tracking software.
export class TrackingManager {
    // Function: RequestTrackingState
    // Used to request a <TrackingState> representing the current state of the tracking software's settings via the
    // WebSocket.
    // Provides a <TrackingState> asynchronously via the _callback parameter.
    //
    // If your _callback requires context it should be bound to that context via .bind()
    public static RequestTrackingState(_callback: (detail: WebSocketResponse) => void) {
        if (!_callback) {
            console.error('Config file state request failed. This call requires a callback.');
            return;
        }

        ConnectionManager.serviceConnection()?.RequestTrackingState(_callback);
    }

    // Function: RequestTrackingChange
    // Requests a modification to the tracking software's settings. Takes any of the following arguments representing
    // the desired changes and sends them through the <ConnectionManager>.
    // <MaskingConfig>, <CameraConfig>, and bools for if images are allowed and if analytics are enabled.
    //
    // Provide a _callback if you require confirmation that your settings were used correctly.
    // If your _callback requires context it should be bound to that context via .bind().
    public static RequestTrackingChange(
        _callback: (detail: WebSocketResponse) => void | null,
        _mask: Mask | null,
        _cameraReversed: boolean | null,
        _allowImages: boolean | null,
        _analyticsEnabled: boolean | null
    ) {
        const requestID = uuidgen();

        const content = new PartialTrackingState(
            requestID,
            _mask,
            _cameraReversed,
            _allowImages,
            _analyticsEnabled
        );
        const request = new CommunicationWrapper(ActionCode.SET_TRACKING_STATE, content);

        const jsonContent = JSON.stringify(request);

        ConnectionManager.serviceConnection()?.SendMessage(jsonContent, requestID, _callback);
    }
}
