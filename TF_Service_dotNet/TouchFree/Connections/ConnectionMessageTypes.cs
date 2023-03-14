using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections
{
    public enum TrackingServiceState
    {
        UNAVAILABLE,
        NO_CAMERA,
        CONNECTED
    }

    public enum BinaryMessageType
    {
        Hand_Data = 1
    }

    public enum ConfigurationState
    {
        NOT_LOADED,
        LOADED,
        ERRORED
    }

    [Serializable]
    public struct ConfigState
    {
        public string requestID;
        public InteractionConfig interaction;
        public PhysicalConfig physical;

        public ConfigState(string _id, InteractionConfig _interaction, PhysicalConfig _physical)
        {
            requestID = _id;
            interaction = _interaction;
            physical = _physical;
        }
    }

    [Serializable]
    public struct ServiceStatus
    {
        public string requestID;
        public TrackingServiceState trackingServiceState;
        public ConfigurationState configurationState;
        public string serviceVersion;
        public string trackingVersion;
        public string cameraSerial;
        public string cameraFirmwareVersion;

        public ServiceStatus(string _id,
            TrackingServiceState _trackingServiceState,
            ConfigurationState _configurationState,
            string _serviceVersion,
            string? _trackingVersion,
            string? _cameraSerial,
            string? _cameraFirmwareVersion)
        {
            requestID = _id;
            trackingServiceState = _trackingServiceState;
            configurationState = _configurationState;
            serviceVersion = _serviceVersion;
            trackingVersion = _trackingVersion ?? "Tracking not connected";
            cameraSerial = _cameraSerial ?? "Device not connected";
            cameraFirmwareVersion = _cameraFirmwareVersion ?? "Device not connected";
        }
    }

    [Serializable]
    public struct HandShakeResponse
    {
        public string requestID;
        public string status;
        public string message;
        public string originalRequest;
        public string touchFreeVersion;
        public string apiVersion;

        public HandShakeResponse(string _id, string _status, string _msg, string _request, string _touchFreeVersion, string _apiVersion)
        {
            requestID = _id;
            status = _status;
            message = _msg;
            originalRequest = _request;
            touchFreeVersion = _touchFreeVersion;
            apiVersion = _apiVersion;
        }
    }

    [Serializable]
    public record struct ResponseToClient(string requestID, string status, string message, string originalRequest);

    [Serializable]
    public record struct MaskingData(double lower, double upper, double right, double left)
    {
        public static explicit operator MaskingData(Configuration.MaskingData other) => new()
        {
            left = other.Left,
            right = other.Right,
            upper = other.Upper,
            lower = other.Lower
        };

        public static explicit operator Configuration.MaskingData(MaskingData data) => new()
        {
            Left = data.left,
            Right = data.right,
            Lower = data.lower,
            Upper = data.upper
        };
    }

    [Serializable]
    public readonly record struct TrackingApiState(
        string requestID,
        SuccessWrapper<MaskingData?>? mask,
        SuccessWrapper<bool?>? allowImages,
        SuccessWrapper<bool?>? cameraReversed,
        SuccessWrapper<bool?>? analyticsEnabled);

    public struct SuccessWrapper<T>
    {
        public bool succeeded;
        public string msg;
        public T? content;

        public SuccessWrapper(bool _success, string _message, T _content)
        {
            succeeded = _success;
            msg = _message;
            content = _content;
        }
    }

    public readonly record struct IncomingRequest(ActionCode ActionCode, string Content)
    {
        public Result<IncomingRequestWithId> DeserializeAndValidateRequestId()
        {
            var contentObj = JsonConvert.DeserializeObject<JObject>(Content);
            if (contentObj == null) return new Error("Deserializing request content failed: returned null");
            
            var request = this; // Lambda cannot capture "this" in structs, need to copy to a local
            return MessageValidation.ValidateRequestId(contentObj)
                .Map(id =>  new IncomingRequestWithId(request.ActionCode, contentObj, id, request.Content));
        }
    }
    public readonly record struct IncomingRequestWithId(ActionCode ActionCode, JObject ContentRoot, string RequestId, string OriginalContent);
}
