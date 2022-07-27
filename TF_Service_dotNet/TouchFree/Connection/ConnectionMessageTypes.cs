using System;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connection
{
    public enum TrackingServiceState
    {
        UNAVAILABLE,
        NO_CAMERA,
        CONNECTED
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

        public ServiceStatus(string _id, TrackingServiceState _trackingServiceState, ConfigurationState _configurationState)
        {
            requestID = _id;
            trackingServiceState = _trackingServiceState;
            configurationState = _configurationState;
        }
    }

    [Serializable]
    public struct ResponseToClient
    {
        public string requestID;
        public string status;
        public string message;
        public string originalRequest;

        public ResponseToClient(string _id, string _status, string _msg, string _request)
        {
            requestID = _id;
            status = _status;
            message = _msg;
            originalRequest = _request;
        }
    }
}
