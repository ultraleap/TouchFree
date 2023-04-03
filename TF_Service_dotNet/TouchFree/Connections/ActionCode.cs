using System.Linq;

namespace Ultraleap.TouchFree.Library.Connections;

public enum ActionCode
{
    // Input Action
    INPUT_ACTION,

    // Configuration
    CONFIGURATION_STATE,
    CONFIGURATION_RESPONSE,
    SET_CONFIGURATION_STATE,
    REQUEST_CONFIGURATION_STATE,

    // Version
    VERSION_HANDSHAKE,
    VERSION_HANDSHAKE_RESPONSE,

    HAND_PRESENCE_EVENT,

    // Service Status
    REQUEST_SERVICE_STATUS,
    SERVICE_STATUS_RESPONSE,
    SERVICE_STATUS,

    // Configuration File
    REQUEST_CONFIGURATION_FILE,
    CONFIGURATION_FILE_STATE,
    SET_CONFIGURATION_FILE,
    CONFIGURATION_FILE_CHANGE_RESPONSE,

    // Quick Setup
    QUICK_SETUP,
    QUICK_SETUP_CONFIG,
    QUICK_SETUP_RESPONSE,

    // Hand Data
    HAND_DATA,
    SET_HAND_DATA_STREAM_STATE,
    SET_HAND_DATA_STREAM_STATE_RESPONSE,

    // Tracking State
    GET_TRACKING_STATE,
    SET_TRACKING_STATE,
    TRACKING_STATE,

    // Interaction Zone
    INTERACTION_ZONE_EVENT,
}

public static class ActionCodeExtensions
{
    private static readonly ActionCode[] _handledActionCodes = {
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

    private static readonly ActionCode[] _unexpectedActionCodes = {
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
        ActionCode.TRACKING_STATE,
        ActionCode.QUICK_SETUP_CONFIG,
        ActionCode.INTERACTION_ZONE_EVENT,
    };

    public static bool ExpectedToBeHandled(this ActionCode actionCode) => _handledActionCodes.Contains(actionCode);

    public static bool UnexpectedByTheService(this ActionCode actionCode) => _unexpectedActionCodes.Contains(actionCode);
}