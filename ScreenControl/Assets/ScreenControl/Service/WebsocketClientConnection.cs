using UnityEngine;

using WebSocketSharp;
using WebSocketSharp.Server;

using Ultraleap.ScreenControl.Core;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;
using Ultraleap.ScreenControl.Service.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Service
{
    public class WebsocketClientConnection : MonoBehaviour
    {
        private WebSocketServer wsServer = null;
        private ScreenControlWsBehaviour socketBehaviour = null;
        public WebSocketReceiverQueue receiverQueue;

        private bool websocketInitalised = false;
        private bool restartServer = false;

        public short port = 9739;

        void OnEnable()
        {
            InitialiseServer();
        }

        internal WebsocketClientConnection()
        {
            InteractionManager.HandleInputAction += SendDataToWebsocket;
        }

        ~WebsocketClientConnection()
        {
            InteractionManager.HandleInputAction -= SendDataToWebsocket;
        }

        private void SetupConnection(ScreenControlWsBehaviour behaviour)
        {
            if (behaviour != null)
            {
                socketBehaviour = behaviour;
                socketBehaviour.clientConnection = this;
                Debug.Log("connection set up");
            }
        }

        private void InitialiseServer()
        {
            websocketInitalised = false;

            wsServer = new WebSocketServer($"ws://127.0.0.1:{port}");
            wsServer.AddWebSocketService<ScreenControlWsBehaviour>("/connect", SetupConnection);

            wsServer.AllowForwardedRequest = true;
            wsServer.ReuseAddress = true;
            wsServer.Start();

            receiverQueue = gameObject.AddComponent<WebSocketReceiverQueue>();
            receiverQueue.clientConnection = this;
        }

        void SendDataToWebsocket(CoreInputAction _data)
        {
            // if IsListening stops being true the server
            // has aborted / stopped, so needs remaking
            if (wsServer == null ||
                (!wsServer.IsListening && websocketInitalised))
            {
                InitialiseServer();
            }

            if (wsServer.IsListening) {
                websocketInitalised = true;
            }

            if (!websocketInitalised ||
                socketBehaviour == null ||
                socketBehaviour.ConnectionState != WebSocketState.Open)
            {
                return;
            }

            socketBehaviour.SendInputAction(_data);
        }

        public void SetConfigState(string _content)
        {
            ConfigRequest newData = new ConfigRequest("", ConfigManager.InteractionConfig, ConfigManager.PhysicalConfig);

            // TODO: Ideally here we want to see if anything sent was invalid and return them with an error message so they know that something was not settable
            JsonUtility.FromJsonOverwrite(_content, newData);

            ConfigManager.InteractionConfig = newData.interaction;
            ConfigManager.PhysicalConfig = newData.physical;

            ConfigManager.PhysicalConfig.ConfigWasUpdated();
            ConfigManager.InteractionConfig.ConfigWasUpdated();

            socketBehaviour.SendConfigurationResponse(new ConfigResponse(newData.requestID, "Success", ""));
        }
    }
}