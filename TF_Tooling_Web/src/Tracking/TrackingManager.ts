import { ConnectionManager } from '../Connection/ConnectionManager';
import { TrackingStateResponse } from '../Connection/TouchFreeServiceTypes';
import { TrackingState } from './TrackingTypes';

/**
 * This class provides methods for getting and setting the settings of the tracking software.
 * @public
 */
export class TrackingManager {
    
    /**
     * Request a {@link TrackingStateResponse} representing the current state of the tracking software
     * @remarks
     * Use {@link ConvertResponseToState} on the response to get TrackingState in a more helpful form
     * @param _callback Callback to call with {@link TrackingStateResponse}
     */
    public static RequestTrackingState(_callback: (detail: TrackingStateResponse) => void) {
        if (!_callback) {
            console.error('Config file state request failed. This call requires a callback.');
            return;
        }

        ConnectionManager.serviceConnection()?.RequestTrackingState(_callback);
    }

    /**
     * Requests a modification to the tracking software's settings.
     * @param _state State to request. Options not provided within the object will not be modified.
     * @param _callback Optional callback if you require confirmation that settings were changed correctly.
     */
    public static RequestTrackingChange(
        _state: Partial<TrackingState>,
        _callback: ((detail: TrackingStateResponse) => void) | null = null
    ): void {
        ConnectionManager.serviceConnection()?.RequestTrackingChange(_state, _callback);
    }

    /**
     * Converts a {@link TrackingStateResponse} to a `Partial<TrackingState>` to make the response easier to consume.
     * @param _response Response to convert
     * @returns Converted Partial {@link TrackingState}
     */
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
