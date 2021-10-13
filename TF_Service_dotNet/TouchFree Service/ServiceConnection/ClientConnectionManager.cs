using System;
using System.Collections.Generic;
using System.Timers;

using WebSocketSharp;
using WebSocketSharp.Server;

//using Ultraleap.TouchFree.ServiceShared;
using Ultraleap.TouchFree.Service.ConnectionTypes;

namespace Ultraleap.TouchFree.Service
{
    public class ClientConnectionManager
    {
        public static ClientConnectionManager Instance;
        public WebSocketReceiver receiverQueue;

        public event Action LostAllConnections;

        public short port = 9739;

        private WebSocketServer wsServer = null;
        private List<ClientConnection> activeConnections = new List<ClientConnection>();   

        private Timer mainTimer;
        private bool websocketInitalised = false;

        //internal HandPresenceEvent missedHandPresenceEvent = new HandPresenceEvent(HandPresenceState.HANDS_LOST);

        public ClientConnectionManager(Timer _mainTimer)
        {
            Instance = this;
            this.mainTimer = _mainTimer;
            //InteractionManager.HandleInputAction += Instance.SendInputActionToWebsocket;
            InitialiseServer();
        }

        ~ClientConnectionManager()
        {
            //InteractionManager.HandleInputAction -= Instance.SendInputActionToWebsocket;
        }

        private void OnHandFound()
        {
            //HandPresenceEvent handFoundEvent = new HandPresenceEvent(HandPresenceState.HAND_FOUND);

            foreach (ClientConnection _connection in activeConnections)
            {
                if (_connection.State == WebSocketState.Open)
                {
                    //_connection.SendHandPresenceEvent(handFoundEvent);
                }
            }

            // Cache handPresenceEvent when no clients are connected
            if (activeConnections.Count == 0)
            {
                //missedHandPresenceEvent = handFoundEvent;
            }
        }

        private void OnHandsLost()
        {
            HandPresenceEvent handsLostEvent = new HandPresenceEvent(HandPresenceState.HANDS_LOST);

            foreach (ClientConnection _connection in activeConnections)
            {
                if (_connection.State == WebSocketState.Open)
                {
                    _connection.SendHandPresenceEvent(handsLostEvent);
                }
            }

            // Cache handPresenceEvent when no clients are connected
            if (activeConnections.Count == 0)
            {
                //missedHandPresenceEvent = handsLostEvent;
            }
        }

        private void SetupConnection(ClientConnection _connection)
        {
            if (_connection != null)
            {
                activeConnections.Add(_connection);
                Console.WriteLine("Connection set up");
            }
        }

        internal void RemoveConnection(ClientConnection _connection)
        {
            activeConnections.Remove(_connection);

            if (activeConnections.Count < 1)
            {
                // there are no connections
                LostAllConnections?.Invoke();
            }
        }

        private void InitialiseServer()
        {
            websocketInitalised = false;

            receiverQueue = new WebSocketReceiver(mainTimer);

            wsServer = new WebSocketServer($"ws://127.0.0.1:{port}");
            wsServer.AddWebSocketService<ClientConnection>("/connect", SetupConnection);

            wsServer.AllowForwardedRequest = true;
            wsServer.ReuseAddress = true;
            wsServer.Start();

            //HandManager.Instance.HandFound += OnHandFound;
            //HandManager.Instance.HandsLost += OnHandsLost;

            // This is here so the test infrastructure has some sign that the app is ready
            Console.WriteLine("Service Setup Complete");
        }

        void SendInputActionToWebsocket(InputAction _data)
        {
            // if IsListening stops being true the server
            // has aborted / stopped, so needs remaking
            if (wsServer == null ||
                (!wsServer.IsListening && websocketInitalised))
            {
                InitialiseServer();
            }

            if (wsServer.IsListening)
            {
                websocketInitalised = true;
            }

            if (!websocketInitalised ||
                activeConnections == null ||
                activeConnections.Count < 1)
            {
                return;
            }

            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.State == WebSocketState.Open)
                {
                    connection.SendInputAction(_data);
                }
            }
        }

        public void SendConfigChangeResponse(ResponseToClient _response)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.State == WebSocketState.Open)
                {
                    connection.SendConfigChangeResponse(_response);
                }
            }
        }

        public void SendConfigState(ConfigState _config)
        {
            foreach (ClientConnection connection in activeConnections)
            {
                if (connection.State == WebSocketState.Open)
                {
                    connection.SendConfigState(_config);
                }
            }
        }
    }
}