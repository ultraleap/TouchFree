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

namespace Ultraleap.TouchFree.Library.Connections
{
    public class ClientConnection : IClientConnection
    {
        public WebSocket Socket { get; }

        private bool HandshakeCompleted;
        private readonly IEnumerable<IMessageQueueHandler> messageQueueHandlers;
        private readonly IClientConnectionManager clientMgr;
        private readonly IConfigManager configManager;

        public ClientConnection(WebSocket _socket, IEnumerable<IMessageQueueHandler> _messageQueueHandlers, IClientConnectionManager _clientMgr, IConfigManager _configManager)
        {
            Socket = _socket;
            messageQueueHandlers = _messageQueueHandlers;
            clientMgr = _clientMgr;
            configManager = _configManager;
            HandshakeCompleted = false;

            TouchFreeLog.WriteLine("Websocket Connection opened");
        }

        public void SendInputAction(InputAction _data)
        {
            if (!HandshakeCompleted)
            {
                // Long-term we shouldn't get this far until post-handshake, but the systems should
                // be designed cohesively when the Service gets its polish
                return;
            }

            WebsocketInputAction converted = new WebsocketInputAction(_data);

            SendResponse(converted, ActionCode.INPUT_ACTION);
        }

        public void SendHandData(HandFrame _data, ArraySegment<byte> lastHandData)
        {
            if (!HandshakeCompleted)
            {
                // Long-term we shouldn't get this far until post-handshake, but the systems should
                // be designed cohesively when the Service gets its polish
                return;
            }

            string jsonMessage = JsonConvert.SerializeObject(_data);

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

        public void SendHandPresenceEvent(HandPresenceEvent _response)
        {
            SendResponse(_response, ActionCode.HAND_PRESENCE_EVENT);
        }

        private void SendHandshakeResponse(HandShakeResponse _response)
        {
            SendResponse(_response, ActionCode.VERSION_HANDSHAKE_RESPONSE);
        }

        public void SendResponse<T>(T _response, ActionCode actionCode)
        {
            CommunicationWrapper<T> message =
                new CommunicationWrapper<T>(actionCode.ToString(), _response);

            string jsonMessage = JsonConvert.SerializeObject(message);

            Socket.SendAsync(
                Encoding.UTF8.GetBytes(jsonMessage),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        private void SendInitialHandState()
        {
            this.SendHandPresenceEvent(clientMgr.MissedHandPresenceEvent);
        }

        public static Compatibility GetVersionCompability(string _clientVersion, Version _coreVersion)
        {
            Version clientVersionParsed = new Version(_clientVersion);

            if (clientVersionParsed.Major < _coreVersion.Major)
            {
                return Compatibility.CLIENT_OUTDATED;
            }
            else if (clientVersionParsed.Major > _coreVersion.Major)
            {
                return Compatibility.SERVICE_OUTDATED;
            }

            else if (clientVersionParsed.Minor < _coreVersion.Minor)
            {
                return Compatibility.CLIENT_OUTDATED_WARNING;
            }
            else if (clientVersionParsed.Minor > _coreVersion.Minor)
            {
                return Compatibility.SERVICE_OUTDATED;
            }

            if (clientVersionParsed.Build > _coreVersion.Build)
            {
                return Compatibility.SERVICE_OUTDATED_WARNING;
            }

            return Compatibility.COMPATIBLE;
        }

        public void OnMessage(string _message)
        {
            // Find key areas of the rawData, the "action" and the "content"
            var match = Regex.Match(_message, "{\\s*?\"action\"\\s*?:\\s*?\"([\\w\\d_]+?)\"\\s*?,\\s*?\"content\"\\s*?:\\s*?({.+?})\\s*?}$");

            // "action" = match.Groups[1] // "content" = match.Groups[2]
            ActionCode action = (ActionCode)Enum.Parse(typeof(ActionCode), match.Groups[1].ToString());
            string content = match.Groups[2].ToString();

            // New case for version Handshake
            // if anything comes in BEFORE version handshake, respond w/ an error

            if (!HandshakeCompleted)
            {
                ProcessHandshake(action, content);
                return;
            }

            // We don't handle after-the-fact Handshake Requests here. We may wish to
            // if / when we anticipate externals building their own Tooling clients.
            var queueHandler = messageQueueHandlers.SingleOrDefault(x => x.ActionCodes.Contains(action));
            if (queueHandler != null)
            {
                queueHandler.AddItemToQueue(new IncomingRequest(action, null, content));
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

            response.requestID = contentObj["requestID"].Value<string>();

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
            Compatibility compatibility = GetVersionCompability(clientApiVersion, VersionManager.ApiVersion);

            string configurationWarning = string.Empty;

            if (!configManager.AreConfigsInGoodState())
            {
                configurationWarning = " Configuration is in a bad state. Please update the configuration via TouchFree Settings";
            }

            switch (compatibility)
            {
                case Compatibility.COMPATIBLE:
                    SendAndHandleHandshakeSuccess($"Handshake Successful.{configurationWarning}", response);
                    return;
                case Compatibility.CLIENT_OUTDATED_WARNING:
                    SendAndHandleHandshakeSuccess($"Handshake Warning: Client is outdated relative to Service.{configurationWarning}", response);
                    return;
                case Compatibility.SERVICE_OUTDATED_WARNING:
                    SendAndHandleHandshakeSuccess($"Handshake Warning: Service is outdated relative to Client.{configurationWarning}", response);
                    return;
                case Compatibility.CLIENT_OUTDATED:
                    SendAndHandleHandshakeFailure($"Handshake Failed: Client is outdated relative to Service.{configurationWarning}", response);
                    return;
                case Compatibility.SERVICE_OUTDATED:
                    SendAndHandleHandshakeFailure($"Handshake Failed: Service is outdated relative to Client.{configurationWarning}", response);
                    return;
            }

            response.status = "Failure";
            SendHandshakeResponse(response);
        }

        private void SendAndHandleHandshakeFailure(string message, HandShakeResponse response)
        {
            response.message = message;
            response.status = "Failure";
            TouchFreeLog.ErrorWriteLine(response.message);
            SendHandshakeResponse(response);
        }

        private void SendAndHandleHandshakeSuccess(string message, HandShakeResponse response)
        {
            HandshakeCompleted = true;
            response.message = message;
            response.status = "Success";
            TouchFreeLog.WriteLine(response.message);
            SendHandshakeResponse(response);
            SendInitialHandState();
        }
    }
}