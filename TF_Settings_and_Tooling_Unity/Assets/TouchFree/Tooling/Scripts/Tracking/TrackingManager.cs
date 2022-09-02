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
            TrackingState _state,
            Action<TrackingStateResponse> _callback = null)
        {
            Guid requestID = Guid.NewGuid();

            var content = StringifyTrackingState(_state);

            string jsonContent = "";
            jsonContent += "{\"action\":\"";
            jsonContent += ActionCode.SET_TRACKING_STATE + "\",\"content\":{\"requestID\":\"";
            jsonContent += requestID + "\",";
            jsonContent += content + "}}";

            Debug.Log("Sending Tracking Request:");
            Debug.Log(jsonContent);

            if (_callback == null) {
                ConnectionManager.serviceConnection?.RequestTrackingChange(jsonContent, requestID.ToString());
            } else {
                ConnectionManager.serviceConnection?.RequestTrackingChange(jsonContent, requestID.ToString(), _callback);
            }
        }

        private static string StringifyTrackingState(TrackingState _state)
        {
            string newContent = "";

            Debug.Log(JsonUtility.ToJson(_state));

            if (_state.mask.HasValue) {
                newContent += "\"mask\": " + JsonUtility.ToJson(_state.mask.Value) + ",";
            }

            if (_state.allowImages.HasValue) {
                newContent += $"\"allowImages\": {_state.allowImages.Value.ToString().ToLower()},";
            }
            if (_state.cameraReversed.HasValue) {
                newContent += $"\"cameraReversed\": {_state.cameraReversed.Value.ToString().ToLower()},";
            }
            if (_state.analyticsEnabled.HasValue) {
                newContent += $"\"analyticsEnabled\": {_state.analyticsEnabled.Value.ToString().ToLower()},";
            }

            return newContent;
        }
    }
}