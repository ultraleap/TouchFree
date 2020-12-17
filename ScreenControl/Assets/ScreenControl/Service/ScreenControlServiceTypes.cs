using System;

namespace Ultraleap.ScreenControl.Service.ScreenControlTypes
{
    internal enum ActionCodes
    {
        INPUT_ACTION,
        CONFIGURATION_STATE,
        CONFIGURATION_RESPONSE,
        SET_CONFIGURATION_STATE,
        REQUEST_CONFIGURATION_STATE
    }

    internal enum Compatibility
    {
        COMPATIBLE,
        CORE_OUTDATED,
        CLIENT_OUTDATED
    }

    [Serializable]
    public struct ConfigRequest
    {
        public string requestID;
        public Core.InteractionConfig interaction;
        public Core.PhysicalConfig physical;

        public ConfigRequest(string _id, Core.InteractionConfig _interaction, Core.PhysicalConfig _physical)
        {
            requestID = _id;
            interaction = _interaction;
            physical = _physical;
        }
    }

    [Serializable]
    public struct ConfigResponse
    {
        public string requestID;
        public string status;
        public string message;
        public string originalRequest;

        public ConfigResponse(string _id, string _status, string _msg, string _request)
        {
            requestID = _id;
            status = _status;
            message = _msg;
            originalRequest = _request;
        }
    }

    internal struct CommunicationWrapper<T>
    {
        public string action;
        public T content;

        public CommunicationWrapper(string _actionCode, T _content)
        {
            action = _actionCode;
            content = _content;
        }
    }
}