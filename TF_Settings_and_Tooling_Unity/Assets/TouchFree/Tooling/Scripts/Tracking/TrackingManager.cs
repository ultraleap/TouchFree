using System;

using UnityEngine;

using Ultraleap.TouchFree.Tooling.Configuration;
using Ultraleap.TouchFree.Tooling.Connection;

namespace Ultraleap.TouchFree.Tooling.Tracking
{
    // class: TrackingManager
    // This class provides methods for getting and setting the settings of the tracking software.
    public class TrackingManager
    {
        // Function: RequestTrackingState
        // Used to request a <TrackingState> representing the current state of the tracking software's settings via the
        // WebSocket.
        // Provides a <TrackingState> asynchronously via the _callback parameter.
        //
        // If your _callback requires context it should be bound to that context via .bind()
        public static void RequestTrackingState(Action<TrackingStateResponse> _callback)
        {
            if (_callback == null)
            {
                Debug.LogError("Config file state request failed. This call requires a callback.");
                return;
            }

            ConnectionManager.serviceConnection?.RequestTrackingState(_callback);
        }

        // Function: RequestTrackingChange
        // Requests a modification to the tracking software's settings. Takes any of the following arguments representing
        // the desired changes and sends them through the <ConnectionManager>.
        // <MaskingConfig>, <CameraConfig>, and bools for if images are allowed and if analytics are enabled.
        //
        // Provide a _callback if you require confirmation that your settings were used correctly.
        // If your _callback requires context it should be bound to that context via .bind().
        public static void RequestTrackingChange(
            Action<WebSocketResponse>? _callback,
            MaskData? _mask,
            bool? _allowImages,
            bool? _cameraReversed,
            bool? _analyticsEnabled
        )
        {
            Guid requestID = Guid.NewGuid();

            var content = new TrackingState(
                requestID.ToString(),
                _mask,
                _cameraReversed,
                _allowImages,
                _analyticsEnabled
            );

            var request = new CommunicationWrapper<TrackingState>(ActionCode.SET_TRACKING_STATE.ToString(), content);

            var jsonContent = JsonUtility.ToJson(request);

            ConnectionManager.serviceConnection?.SendMessage(jsonContent, requestID.ToString(), _callback);
        }
    }
}