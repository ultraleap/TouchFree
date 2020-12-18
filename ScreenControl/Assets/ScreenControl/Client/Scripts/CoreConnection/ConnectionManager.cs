using System;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    /* Enum: ConnectionType     
        WEB_SOCKET - Default connection via WebSocket
        DIRECT_CORE_MODULE - Internal utility for testing connections, DO NOT USE
    */
    [Serializable]
    enum ConnectionType
    {
        WEB_SOCKET,
        DIRECT_CORE_MODULE
    }

    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;

        public static event Action OnConnected;

        static CoreConnection currentCoreConnection;
        public static CoreConnection coreConnection {
            get {
                return currentCoreConnection;
            }
        }

        [SerializeField] ConnectionType connectionType = ConnectionType.WEB_SOCKET;

        [Header("WebSocket connection values")]
        [SerializeField] string iPAddress = "127.0.0.1";
        [SerializeField] string port = "9739";

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                return;
            }
            Instance = this;

            Connect();
        }

        public static void AddConnectionListener(Action onConnectFunc)
        {
            // TODO: CHECK THIS
            // Add it to the listener where all members get pinged once connected
            OnConnected += onConnectFunc;

            if (currentCoreConnection != null)
            {
                onConnectFunc();
            }
        }

        public void Connect()
        {
            switch (connectionType)
            {
                case ConnectionType.WEB_SOCKET:
                    currentCoreConnection = new WebSocketCoreConnection(iPAddress, port);
                    // Invoke OnConnected event when connect successfully completes
                    OnConnected?.Invoke();
                    break;

                case ConnectionType.DIRECT_CORE_MODULE:
#if SCREENCONTROL_CORE
                    currentCoreConnection = new DirectCoreConnection();

                    // Invoke OnConnected event when connect successfully completes
                    OnConnected?.Invoke();
#else
                    var errorMsg = @"Could not initialise a Direct connection to Screen Control Core as it wasn't available!
If you wish to use ScreenControl in this manner, please import the Screen Control Core module and add
""SCREENCONTROL_CORE"" to the ""Scripting Define Symbols"" in your Player settings.";
                    Debug.Log(errorMsg);
#endif
                    break;
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        public static void Disconnect()
        {
            if (currentCoreConnection != null)
            {
                currentCoreConnection.Disconnect();
                currentCoreConnection = null;
            }
        }
    }
}