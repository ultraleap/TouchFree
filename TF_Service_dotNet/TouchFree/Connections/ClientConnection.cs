using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections.MessageQueues;

namespace Ultraleap.TouchFree.Library.Connections;

public class ClientConnection : IClientConnection
{
    public WebSocket Socket { get; }

    private bool _handshakeCompleted;
    private readonly IEnumerable<IMessageQueueHandler> _messageQueueHandlers;
    private readonly IClientConnectionManager _clientMgr;
    private readonly IConfigManager _configManager;

    public ClientConnection(WebSocket socket, IEnumerable<IMessageQueueHandler> messageQueueHandlers, IClientConnectionManager clientMgr, IConfigManager configManager)
    {
        Socket = socket;
        _messageQueueHandlers = messageQueueHandlers;
        _clientMgr = clientMgr;
        _configManager = configManager;
        _handshakeCompleted = false;

        TouchFreeLog.WriteLine("Websocket Connection opened");
    }

    public void SendInputAction(in InputAction inputAction)
    {
        if (!_handshakeCompleted)
        {
            // Long-term we shouldn't get this far until post-handshake, but the systems should
            // be designed cohesively when the Service gets its polish
            return;
        }

        WebsocketInputAction converted = (WebsocketInputAction)inputAction;

        SendResponse(converted, ActionCode.INPUT_ACTION);
    }

    public void SendHandData(in HandFrame handFrame, in ArraySegment<byte> lastHandData)
    {
        if (!_handshakeCompleted)
        {
            // Long-term we shouldn't get this far until post-handshake, but the systems should
            // be designed cohesively when the Service gets its polish
            return;
        }

        // TODO: Reduce allocations in this method

        string jsonMessage = JsonConvert.SerializeObject(handFrame);

        byte[] jsonAsBytes = Encoding.UTF8.GetBytes(jsonMessage);

        Int32 dataLength = lastHandData.Count;

        IEnumerable<byte> binaryMessageType = BitConverter.GetBytes((int)BinaryMessageType.Hand_Data);
        var dataToSend = binaryMessageType.Concat(BitConverter.GetBytes(dataLength));
        dataToSend = dataLength > 0 ? dataToSend.Concat(lastHandData) : dataToSend;
        dataToSend = dataToSend.Concat(jsonAsBytes);

        Socket.SendAsync(
            dataToSend.ToArray(),
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None);
    }

    public void SendHandPresenceEvent(in HandPresenceEvent handPresenceEvent) => SendResponse(handPresenceEvent, ActionCode.HAND_PRESENCE_EVENT);

    public void SendInteractionZoneEvent(in InteractionZoneEvent interactionZoneEvent) => SendResponse(interactionZoneEvent, ActionCode.INTERACTION_ZONE_EVENT);

    private void SendHandshakeResponse(in HandShakeResponse response)
    {
        var message = new CommunicationWrapper<HandShakeResponse>(
            ActionCode.VERSION_HANDSHAKE_RESPONSE.ToString(),
            response);

        string jsonMessage = JsonConvert.SerializeObject(message);

        Socket.SendAsync(
            Encoding.UTF8.GetBytes(jsonMessage),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public void SendResponse<T>(in T response, in ActionCode actionCode)
    {
        if (!_handshakeCompleted)
        {
            return;
        }

        var message = new CommunicationWrapper<T>(actionCode.ToString(), response);

        string jsonMessage = JsonConvert.SerializeObject(message);

        Socket.SendAsync(
            Encoding.UTF8.GetBytes(jsonMessage),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    private void SendInitialHandState() => SendHandPresenceEvent(_clientMgr.MissedHandPresenceEvent);

    public static CompatibilityInformation GetVersionCompability(string clientVersion, Version coreVersion)
    {
        Version clientVersionParsed = new Version(clientVersion);

        Compatibility compatibility;
        if (clientVersionParsed.Major < coreVersion.Major) compatibility = Compatibility.CLIENT_OUTDATED;
        else if (clientVersionParsed.Major > coreVersion.Major) compatibility = Compatibility.SERVICE_OUTDATED;
        else if (clientVersionParsed.Minor < coreVersion.Minor) compatibility = Compatibility.CLIENT_OUTDATED_WARNING;
        else if (clientVersionParsed.Minor > coreVersion.Minor) compatibility = Compatibility.SERVICE_OUTDATED;
        else if (clientVersionParsed.Build > coreVersion.Build) compatibility = Compatibility.SERVICE_OUTDATED_WARNING;
        else compatibility = Compatibility.COMPATIBLE;

        return new CompatibilityInformation(compatibility, clientVersionParsed, coreVersion);
    }

    public void OnMessage(string message)
    {
        // Find key areas of the rawData, the "action" and the "content"
        var match = Regex.Match(message, "{\\s*?\"action\"\\s*?:\\s*?\"([\\w\\d_]+?)\"\\s*?,\\s*?\"content\"\\s*?:\\s*?({.+?})\\s*?}$");

        // "action" = match.Groups[1] // "content" = match.Groups[2]
        ActionCode action = (ActionCode)Enum.Parse(typeof(ActionCode), match.Groups[1].ToString());
        string content = match.Groups[2].ToString();

        // New case for version Handshake
        // if anything comes in BEFORE version handshake, respond w/ an error

        if (!_handshakeCompleted)
        {
            ProcessHandshake(action, content);
            return;
        }

        // We don't handle after-the-fact Handshake Requests here. We may wish to
        // if / when we anticipate externals building their own Tooling clients.
        var queueHandler = _messageQueueHandlers.SingleOrDefault(x => x.HandledActionCodes.Contains(action));
        if (queueHandler != null)
        {
            queueHandler.AddItemToQueue(new IncomingRequest(action, content));
        }
        else if (action.ExpectedToBeHandled())
        {
            TouchFreeLog.ErrorWriteLine($"Expected to be able to handle a {action} action but unable to find queue.");
        }
        else if (action.UnexpectedByTheService())
        {
            TouchFreeLog.ErrorWriteLine($"Received a {action} action. This action is not expected on the Service.");
        }
        else
        {
            TouchFreeLog.ErrorWriteLine($"Received a {action} action. This action is not recognised.");
        }
    }

    private void ProcessHandshake(ActionCode action, string requestContent)
    {
        JObject contentObj = JsonConvert.DeserializeObject<JObject>(requestContent);
        var response = new HandShakeResponse("", "Success", "", requestContent, VersionManager.Version, VersionManager.ApiVersion.ToString());

        if (!contentObj.ContainsKey("requestID") || contentObj.GetValue("requestID").ToString() == "")
        {
            // Validation has failed because there is no valid requestID
            SendAndHandleHandshakeFailure("Handshaking failed. This is due to a missing or invalid requestID", response);
            return;
        }

        response = response with { requestID = contentObj["requestID"].Value<string>() };

        if (action != ActionCode.VERSION_HANDSHAKE)
        {
            // Send back immediate error: Handshake hasn't been completed so other requests
            // cannot be processed
            SendAndHandleHandshakeFailure("Request Rejected: Requests cannot be processed until handshaking is complete.", response);
            return;
        }

        if (!contentObj.ContainsKey(VersionManager.API_HEADER_NAME))
        {
            // Send back immediate error: Cannot compare version number w/o a version number
            SendAndHandleHandshakeFailure("Handshaking Failed: No API Version supplied.", response);
            return;
        }

        string clientApiVersion = (string)contentObj[VersionManager.API_HEADER_NAME];
        CompatibilityInformation compatibilityInfo = GetVersionCompability(clientApiVersion, VersionManager.ApiVersion);

        string configurationWarning = string.Empty;

        if (!_configManager.AreConfigsInGoodState())
        {
            configurationWarning = " Configuration is in a bad state. Please update the configuration via TouchFree Settings";
        }

        var clientText = $"Client (API v{compatibilityInfo.ClientVersion})";
        var serviceText = $"Service (API v{compatibilityInfo.ServiceVersion})";

        switch (compatibilityInfo.Compatibility)
        {
            case Compatibility.COMPATIBLE:
                SendAndHandleHandshakeSuccess($"Handshake Successful.{configurationWarning}", response);
                return;
            case Compatibility.CLIENT_OUTDATED_WARNING:
                SendAndHandleHandshakeSuccess($"Handshake Warning: {clientText} is outdated relative to {serviceText}.{configurationWarning}", response);
                return;
            case Compatibility.SERVICE_OUTDATED_WARNING:
                SendAndHandleHandshakeSuccess($"Handshake Warning: {serviceText} is outdated relative to {clientText}.{configurationWarning}", response);
                return;
            case Compatibility.CLIENT_OUTDATED:
                SendAndHandleHandshakeFailure($"Handshake Failed: {clientText} is outdated relative to {serviceText}.{configurationWarning}", response);
                return;
            case Compatibility.SERVICE_OUTDATED:
                SendAndHandleHandshakeFailure($"Handshake Failed: {serviceText} is outdated relative to {clientText}.{configurationWarning}", response);
                return;
        }

        response = response with { status = "Failure" };
        SendHandshakeResponse(response);
    }

    private void SendAndHandleHandshakeFailure(string message, HandShakeResponse response)
    {
        response = response with { message = message, status = "Failure" };
        TouchFreeLog.ErrorWriteLine(response.message);
        SendHandshakeResponse(response);
    }

    private void SendAndHandleHandshakeSuccess(string message, HandShakeResponse response)
    {
        _handshakeCompleted = true;
        response = response with { message = message, status = "Success" };
        TouchFreeLog.WriteLine(response.message);
        SendHandshakeResponse(response);
        SendInitialHandState();
    }
}