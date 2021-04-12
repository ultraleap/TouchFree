using System;

namespace Ultraleap.ScreenControl.Service.ScreenControlTypes
{
    internal enum ActionCode
    {
        INPUT_ACTION,
        CONFIGURATION_STATE,
        CONFIGURATION_RESPONSE,
        SET_CONFIGURATION_STATE,
        REQUEST_CONFIGURATION_STATE,
        VERSION_HANDSHAKE,
        VERSION_HANDSHAKE_RESPONSE
    }

    internal enum Compatibility
    {
        COMPATIBLE,
        SERVICE_OUTDATED,
        CLIENT_OUTDATED
    }

    [Serializable]
    public struct ConfigState
    {
        public string requestID;
        public Core.InteractionConfig interaction;
        public Core.PhysicalConfig physical;

        public ConfigState(string _id, Core.InteractionConfig _interaction, Core.PhysicalConfig _physical)
        {
            requestID = _id;
            interaction = _interaction;
            physical = _physical;
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