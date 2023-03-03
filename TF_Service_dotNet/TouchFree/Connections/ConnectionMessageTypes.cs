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
    
    public struct MaskingData
    {
        public float lower;
        public float upper;
        public float right;
        public float left;

        public MaskingData(float _lower, float _upper, float _right, float _left)
        {
            lower = _lower;
            upper = _upper;
            right = _right;
            left = _left;
        }
    }

    [Serializable]
    public struct TrackingApiState
    {
        public string requestID;
        public SuccessWrapper<MaskingData?>? mask;
        public SuccessWrapper<bool?>? allowImages;
        public SuccessWrapper<bool?>? cameraReversed;
        public SuccessWrapper<bool?>? analyticsEnabled;
    }

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

    public struct TrackingResponse
    {
        public bool needsMask;
        public bool needsImages;
        public bool needsOrientation;
        public bool needsAnalytics;

        public string originalRequest;
        public bool isGetRequest;
        public TrackingApiState state;

        public TrackingResponse(string _requestId,
                                string _originalRequest,
                                bool _isGetRequest,
                                bool _needsMask,
                                bool _needsImages,
                                bool _needsOrientation,
                                bool _needsAnalytics)
        {
            originalRequest = _originalRequest;
            isGetRequest = _isGetRequest;
            needsMask = _needsMask;
            needsImages = _needsImages;
            needsOrientation = _needsOrientation;
            needsAnalytics = _needsAnalytics;

            state = new TrackingApiState();
            state.requestID = _requestId;
        }
    }
}
