using System;
using System.Text.RegularExpressions;
using UnityEngine;

using WebSocketSharp;

namespace Ultraleap.TouchFree.Tooling.Connection
{
    // Class: ServiceConnection
    // This represents a connection to a TouchFree Service. It should be created by a
    // <ConnectionManager> to ensure there is only one active connection at a time. The sending
    // and receiving of data to the client is handled here.
    public class ServiceConnection
    {
        // Group: Variables

        // Variable: webSocket
        // A reference to the websocket we are connected to.
        WebSocket webSocket;

        // Group: Functions

        // Function: ServiceConnection
        // The constructor for <ServiceConnection> that can be given a different IP Address and Port
        // to connect to on construction. This constructor also redirects incoming messages to
        // <OnMessage>. Once the websocket connection opens, a handshake request is sent with this
        // Client's API version number. The service will not send data over an open connection
        // until this handshake is completed succesfully.
        internal ServiceConnection(
            string _ip = "127.0.0.1",
            string _port = "9739",
            Action onClose = null,
            Action onError = null
        )
        {
            webSocket = new WebSocket($"ws://{_ip}:{_port}/connect");

            webSocket.OnMessage += (sender, e) =>
            {
                OnMessage(e);
            };

            webSocket.OnOpen += (sender, e) =>
            {
                // Send a handshake message with the API version of this client
                string guid = Guid.NewGuid().ToString();

                string handshakeMessage = "{";
                handshakeMessage += $"\"action\": \"{ActionCode.VERSION_HANDSHAKE.ToString()}\",";
                handshakeMessage += "\"content\": {";
                handshakeMessage += $"\"requestID\": \"{guid}\",";
                handshakeMessage += $"\"{VersionInfo.API_HEADER_NAME}\": \"{VersionInfo.ApiVersion}\"";
                handshakeMessage += "}}";

                SendMessage(handshakeMessage, guid, ConnectionResultCallback);
            };

            webSocket.Connect();

            webSocket.OnError += (sender, e) =>
            {
                onError?.Invoke();
            };

            webSocket.OnClose += (sender, e) =>
            {
                onClose?.Invoke();
            };
        }

        public bool IsConnected()
        {
            return webSocket.ReadyState == WebSocketState.Open;
        }

        public void Connect()
        {
            webSocket.Connect();
        }

        // Function: ConnectionResultCallback
        // Passed into <SendMessage> as part of connecting to TouchFree Service, handles the
        // result of the Version Checking handshake.
        private void ConnectionResultCallback(WebSocketResponse response)
        {
            // if failed, console log
            if (response.status != "Success")
            {
                Debug.Log($"Connection to Service failed. Details:\n{response.message}");
            }
        }

        // Function: Disconnect
        // Can be used to force the connection to the <webSocket> to be closed.
        public void Disconnect()
        {
            if (webSocket != null)
            {
                webSocket.Close();
            }
        }

        // Function: OnMessage
        // The first point of contact for new messages received, these are sorted into appropriate
        // types based on their <ActionCode> and added to queues on the <ConnectionManager's>
        // <MessageReceiver>.
        public void OnMessage(MessageEventArgs _message)
        {
            string rawData = _message.Data;

            // Find key areas of the rawData, the "action" and the "content"
            Match match = Regex.Match(rawData, "{\"action\": ?\"([\\w\\d_]+?)\",\"content\": ?({.+?})}$");

            // "action" = match.Groups[1] // "content" = match.Groups[2]
            ActionCode action = (ActionCode)Enum.Parse(typeof(ActionCode), match.Groups[1].ToString());
            string content = match.Groups[2].ToString();

            switch (action)
            {
                case ActionCode.INPUT_ACTION:
                    WebsocketInputAction wsInput = JsonUtility.FromJson<WebsocketInputAction>(content);
                    InputAction cInput = new InputAction(wsInput);
                    ConnectionManager.messageReceiver.actionQueue.Enqueue(cInput);
                    break;

                case ActionCode.CONFIGURATION_STATE:
                case ActionCode.QUICK_SETUP_CONFIG:
                case ActionCode.CONFIGURATION_FILE_STATE:
                    ConfigState configState = JsonUtility.FromJson<ConfigState>(content);
                    ConnectionManager.messageReceiver.configStateQueue.Enqueue(configState);
                    break;

                case ActionCode.HAND_PRESENCE_EVENT:
                    HandPresenceEvent handEvent = JsonUtility.FromJson<HandPresenceEvent>(content);
                    ConnectionManager.messageReceiver.handState = handEvent.state;
                    break;

                case ActionCode.SERVICE_STATUS:
                    ServiceStatus serviceStatus = JsonUtility.FromJson<ServiceStatus>(content);
                    ConnectionManager.messageReceiver.serviceStatusQueue.Enqueue(serviceStatus);
                    break;

                case ActionCode.TRACKING_STATE:
                    TrackingStateResponse trackingResponse = DeserializeTrackingApiResponse(content);
                    ConnectionManager.messageReceiver.trackingStateQueue.Enqueue(trackingResponse);
                    break;

                case ActionCode.CONFIGURATION_RESPONSE:
                case ActionCode.VERSION_HANDSHAKE_RESPONSE:
                case ActionCode.SERVICE_STATUS_RESPONSE:
                case ActionCode.CONFIGURATION_FILE_RESPONSE:
                case ActionCode.QUICK_SETUP_RESPONSE:
                    WebSocketResponse response = JsonUtility.FromJson<WebSocketResponse>(content);
                    ConnectionManager.messageReceiver.responseQueue.Enqueue(response);
                    break;

                case ActionCode.INTERACTION_ZONE_EVENT:
                    InteractionZoneEvent interactionZoneEvent = JsonUtility.FromJson<InteractionZoneEvent>(content);
                    ConnectionManager.messageReceiver.interactionZoneUpdate = new EventUpdate<InteractionZoneState>(interactionZoneEvent.state, EventStatus.UNPROCESSED);
                    break;
            }
        }

        // Function: SendMessage
        // Used internally to send or request information from the Service via the <webSocket>. To
        // be given a pre-made _message and _requestID. Provides an asynchronous <WebSocketResponse>
        // via the _callback parameter.
        internal void SendMessage(string _message, string _requestID, Action<WebSocketResponse> _callback = null)
        {
            if (_requestID == "")
            {
                if (_callback != null)
                {
                    WebSocketResponse response = new WebSocketResponse("", "Failure", "Request failed. This is due to a missing or invalid requestID", _message);
                    _callback.Invoke(response);
                }

                Debug.LogError("Request failed. This is due to a missing or invalid requestID");
                return;
            }

            if (_callback != null)
            {
                ConnectionManager.messageReceiver.responseCallbacks.Add(_requestID, new ResponseCallback(DateTime.Now.Millisecond, _callback));
            }

            webSocket.Send(_message);
        }

        // Function: RequestConfigState
        // Used internally to request a <ConfigState> from the Service via the <webSocket>.
        // Provides an asynchronous <ConfigState> via the _callback parameter.
        internal void RequestConfigState(Action<ConfigState> _callback)
        {
            string requestID = Guid.NewGuid().ToString();
            ConfigChangeRequest request = new ConfigChangeRequest(requestID);

            CommunicationWrapper<ConfigChangeRequest> message =
                new CommunicationWrapper<ConfigChangeRequest>(ActionCode.REQUEST_CONFIGURATION_STATE.ToString(), request);

            string jsonMessage = JsonUtility.ToJson(message);

            if (_callback != null)
            {
                ConnectionManager.messageReceiver.configStateCallbacks.Add(requestID, new ConfigStateCallback(DateTime.Now.Millisecond, _callback));
            }

            webSocket.Send(jsonMessage);
        }

        internal void RequestConfigFile(Action<ConfigState> _callback)
        {
            string requestID = Guid.NewGuid().ToString();
            ConfigChangeRequest request = new ConfigChangeRequest(requestID);

            CommunicationWrapper<ConfigChangeRequest> message =
                new CommunicationWrapper<ConfigChangeRequest>(ActionCode.REQUEST_CONFIGURATION_FILE.ToString(), request);

            string jsonMessage = JsonUtility.ToJson(message);

            if (_callback != null)
            {
                ConnectionManager.messageReceiver.configStateCallbacks.Add(requestID, new ConfigStateCallback(DateTime.Now.Millisecond, _callback));
            }

            webSocket.Send(jsonMessage);
        }

        // Function: RequestConfigFile
        // Used internally to request information from the Service via the <webSocket>.
        // Provides an asynchronous <ServiceStatus> via the _callback parameter.
        internal void RequestServiceStatus(Action<ServiceStatus> _callback)
        {
            string requestID = Guid.NewGuid().ToString();
            ServiceStatusRequest request = new ServiceStatusRequest(requestID);

            CommunicationWrapper<ServiceStatusRequest> message =
                new CommunicationWrapper<ServiceStatusRequest>(ActionCode.REQUEST_SERVICE_STATUS.ToString(), request);

            string jsonMessage = JsonUtility.ToJson(message);

            if (_callback != null)
            {
                ConnectionManager.messageReceiver.serviceStatusCallbacks.Add(requestID, new ServiceStatusCallback(DateTime.Now.Millisecond, _callback));
            }

            webSocket.Send(jsonMessage);
        }

        // Function: SendQuickSetupMessage
        // Used internally to send data about quick setup to the Service via the <webSocket>
        // Provides an asynchronous <WebSocketResponse> via the _callback parameter.
        // Provides an asynchronous <ConfigState> via the _configCallback parameter.
        internal void SendQuickSetupMessage(QuickSetupPosition _position, Action<WebSocketResponse> _callback, Action<ConfigState> _configCallback)
        {
            string requestID = Guid.NewGuid().ToString();
            if (_callback != null)
            {
                ConnectionManager.messageReceiver.responseCallbacks.Add(requestID, new ResponseCallback(DateTime.Now.Millisecond, _callback));
            }
            if (_configCallback != null)
            {
                ConnectionManager.messageReceiver.configStateCallbacks.Add(requestID, new ConfigStateCallback(DateTime.Now.Millisecond, _configCallback));
            }

            var request = new QuickSetupRequest(requestID, _position);
            var message = new CommunicationWrapper<QuickSetupRequest>(ActionCode.QUICK_SETUP.ToString(), request);

            string jsonMessage = JsonUtility.ToJson(message);

            webSocket.Send(jsonMessage);
        }

        // Function: SendQuickSetupMessage
        // Used internally to send data about quick setup to the Service via the <webSocket>
        // Provides an asynchronous <TrackingStateResponse> via the _stateCallback parameter on a successful response.
        // Provides an asynchronous <WebSocketResponse> via the _responseCallback parameter if there are issues to communicate why.
        internal void RequestTrackingState(Action<TrackingStateResponse> _stateCallback)
        {
            if (_stateCallback == null)
            {
                Debug.Log("Request for tracking state failed. This is due to a missing callback");
                return;
            }

            string requestID = Guid.NewGuid().ToString();

            ConnectionManager.messageReceiver.trackingStateCallbacks.Add(requestID, new TrackingStateCallback(DateTime.Now.Millisecond, _stateCallback));

            var request = new ServiceStatusRequest(requestID);
            var message = new CommunicationWrapper<ServiceStatusRequest>(ActionCode.GET_TRACKING_STATE.ToString(), request);

            string jsonMessage = JsonUtility.ToJson(message);

            webSocket.Send(jsonMessage);
        }

        internal void RequestTrackingChange(string _message, string _requestID, Action<TrackingStateResponse> _stateCallback = null)
        {
            if (_requestID == "")
            {
                if (_stateCallback != null)
                {
                    string message = "Tracking State change request failed. This is due to a missing or invalid requestID";

                    var maskResponse = new SuccessWrapper<MaskData?>(false, message, null);
                    var boolResponse = new SuccessWrapper<bool?>(false, message, null);

                    TrackingStateResponse response = new TrackingStateResponse()
                    {
                        requestID = "",
                        mask = maskResponse,
                        allowImages = boolResponse,
                        cameraReversed = boolResponse,
                        analyticsEnabled = boolResponse
                    };

                    _stateCallback.Invoke(response);
                }

                Debug.LogError("Tracking State change request failed. This is due to a missing or invalid requestID");
                return;
            }

            if (_stateCallback != null)
            {
                ConnectionManager.messageReceiver.trackingStateCallbacks.Add(_requestID, new TrackingStateCallback(DateTime.Now.Millisecond, _stateCallback));
            }

            webSocket.Send(_message);
        }

        private static TrackingStateResponse DeserializeTrackingApiResponse(string raw)
        {
            var response = new TrackingStateResponse();

            Regex requestIdExtractor = new Regex(@"""requestID"":""([\w-]+?)"",");
            Regex memberIdentifier = new Regex(@"(?:""(\w+?)"":(?:(?:{""succeeded"":((?:true)|(?:false)),""msg"":""([\w\s]+?)"",(?:""content"":((?:{.+?})|(?:(?:true)|(?:false)))})?)),?)");

            Match requestIdMatch = requestIdExtractor.Match(raw);

            response.requestID = requestIdMatch.Groups[1].Value;

            // each member of matches is 1 member of the TrackingApiResponse bar the requestID, extracted seperately
            MatchCollection matches = memberIdentifier.Matches(raw);

            // Debug.Log($"Got {matches.Count} members with the member identifier");

            foreach (Match match in matches)
            {
                switch (match.Groups[1].Value)
                {
                    case "mask":
                        response.mask = GenerateMaskDataFromMatch(match);
                        break;
                    case "allowImages":
                        response.allowImages = GenerateBoolDataFromMatch(match);
                        break;
                    case "cameraReversed":
                        response.cameraReversed = GenerateBoolDataFromMatch(match);
                        break;
                    case "analyticsEnabled":
                        response.analyticsEnabled = GenerateBoolDataFromMatch(match);
                        break;
                }
            }

            return response;
        }

        private static SuccessWrapper<MaskData?> GenerateMaskDataFromMatch(Match match)
        {
            var result = new SuccessWrapper<MaskData?>();

            result.succeeded = Convert.ToBoolean(match.Groups[2].Value);
            result.msg = match.Groups[3].Value;

            if (match.Groups.Count > 3)
            {
                Regex maskSplitter = new Regex(@"\{""lower"":([\d.]+?),""upper"":([\d.]+?),""right"":([\d.]+?),""left"":([\d.]+?)\}");
                Match maskMatch = maskSplitter.Match(match.Groups[4].Value);

                result.content = new MaskData(
                    Convert.ToSingle(maskMatch.Groups[1].Value),
                    Convert.ToSingle(maskMatch.Groups[2].Value),
                    Convert.ToSingle(maskMatch.Groups[3].Value),
                    Convert.ToSingle(maskMatch.Groups[4].Value)
                );
            }

            return result;
        }

        private static SuccessWrapper<bool?> GenerateBoolDataFromMatch(Match match)
        {
            var result = new SuccessWrapper<bool?>();

            result.succeeded = Convert.ToBoolean(match.Groups[2].Value);
            result.msg = match.Groups[3].Value;

            if (match.Groups.Count > 3)
            {
                result.content = Convert.ToBoolean(match.Groups[4].Value);
            }

            return result;
        }
    }
}