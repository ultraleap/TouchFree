import { TrackingState } from './TrackingTypes';

import { ConnectionManager } from '../Connection/ConnectionManager';
import { TrackingStateResponse } from '../Connection/TouchFreeServiceTypes';

// class: TrackingManager
// This class provides methods for getting and setting the settings of the tracking software.
export class TrackingManager {
    // Function: RequestTrackingState
    // Used to request a <TrackingState> representing the current state of the tracking software's settings via the
    // WebSocket.
    // Provides a <TrackingState> asynchronously via the _callback parameter.
    //
    // If your _callback requires context it should be bound to that context via .bind()
    public static RequestTrackingState(_callback: (detail: TrackingStateResponse) => void) {
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
        _state: Partial<TrackingState>,
        _callback: ((detail: TrackingStateResponse) => void) | null = null
    ): void {
        ConnectionManager.serviceConnection()?.RequestTrackingChange(_state, _callback);
    }

    // Function: ConvertResponseToState
    // Converts a TrackingStateResponse to a Partial<TrackingState> to make the response easier to consume.
    public static ConvertResponseToState(_response: TrackingStateResponse): Partial<TrackingState> {
        const response: Partial<TrackingState> = {};

        if (_response.mask !== undefined && _response.mask !== null) {
            response.mask = _response.mask.content;
        }

        if (_response.cameraReversed !== undefined && _response.cameraReversed !== null) {
            response.cameraReversed = _response.cameraReversed.content;
        }

        if (_response.allowImages !== undefined && _response.allowImages !== null) {
            response.allowImages = _response.allowImages.content;
        }

        if (_response.analyticsEnabled !== undefined && _response.analyticsEnabled !== null) {
            response.analyticsEnabled = _response.analyticsEnabled.content;
        }

        return response;
    }
}
