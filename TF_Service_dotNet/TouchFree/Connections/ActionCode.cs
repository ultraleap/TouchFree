using System.Linq;

namespace Ultraleap.TouchFree.Library.Connections
{
    public enum ActionCode
    {
        INPUT_ACTION,

        CONFIGURATION_STATE,
        CONFIGURATION_RESPONSE,
        SET_CONFIGURATION_STATE,
        REQUEST_CONFIGURATION_STATE,

        VERSION_HANDSHAKE,
        VERSION_HANDSHAKE_RESPONSE,

        HAND_PRESENCE_EVENT,

        REQUEST_SERVICE_STATUS,
        SERVICE_STATUS_RESPONSE,
        SERVICE_STATUS,

        REQUEST_CONFIGURATION_FILE,
        CONFIGURATION_FILE_STATE,
        SET_CONFIGURATION_FILE,
        CONFIGURATION_FILE_CHANGE_RESPONSE,

        QUICK_SETUP,
        QUICK_SETUP_CONFIG,
        QUICK_SETUP_RESPONSE,

        HAND_DATA,
        SET_HAND_DATA_STREAM_STATE,
        SET_HAND_DATA_STREAM_STATE_RESPONSE,

        GET_TRACKING_STATE,
        GET_TRACKING_STATE_RESPONSE,
        SET_TRACKING_STATE,
        SET_TRACKING_STATE_RESPONSE,
    }

    public static class ActionCodeExtensions
    {
        private static readonly ActionCode[] HandledActionCodes = new ActionCode[] {
            ActionCode.SET_CONFIGURATION_STATE,
            ActionCode.REQUEST_CONFIGURATION_STATE,
            ActionCode.REQUEST_SERVICE_STATUS,
            ActionCode.SET_CONFIGURATION_FILE,
            ActionCode.REQUEST_CONFIGURATION_FILE,
            ActionCode.QUICK_SETUP,
            ActionCode.SET_HAND_DATA_STREAM_STATE,
            ActionCode.GET_TRACKING_STATE,
            ActionCode.SET_TRACKING_STATE,
            ActionCode.VERSION_HANDSHAKE
        };

        private static readonly ActionCode[] UnexpectedActionCodes = new ActionCode[] {
            ActionCode.INPUT_ACTION,
            ActionCode.CONFIGURATION_STATE,
            ActionCode.CONFIGURATION_RESPONSE,
            ActionCode.VERSION_HANDSHAKE_RESPONSE,
            ActionCode.HAND_PRESENCE_EVENT,
            ActionCode.SERVICE_STATUS_RESPONSE,
            ActionCode.SERVICE_STATUS,
            ActionCode.CONFIGURATION_FILE_STATE,
            ActionCode.CONFIGURATION_FILE_CHANGE_RESPONSE,
            ActionCode.HAND_DATA,
            ActionCode.SET_HAND_DATA_STREAM_STATE_RESPONSE,
            ActionCode.QUICK_SETUP_RESPONSE,
            ActionCode.GET_TRACKING_STATE_RESPONSE,
            ActionCode.SET_TRACKING_STATE_RESPONSE,
            ActionCode.QUICK_SETUP_CONFIG
        };

        public static bool ExpectedToBeHandled(this ActionCode actionCode)
        {
            return HandledActionCodes.Contains(actionCode);
        }

        public static bool UnexpectedByTheService(this ActionCode actionCode)
        {
            return UnexpectedActionCodes.Contains(actionCode);
        }
    }
}
