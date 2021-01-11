using System;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client.Connection
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;

        public static event Action OnConnected;

        static ServiceConnection currentServiceConnection;
        public static ServiceConnection serviceConnection {
            get {
                return currentServiceConnection;
            }
        }

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

            if (currentServiceConnection != null)
            {
                onConnectFunc();
            }
        }

        public void Connect()
        {
            currentServiceConnection = new ServiceConnection(iPAddress, port);
            // Invoke OnConnected event when connect successfully completes
            OnConnected?.Invoke();
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        public static void Disconnect()
        {
            if (currentServiceConnection != null)
            {
                currentServiceConnection.Disconnect();
                currentServiceConnection = null;
            }
        }
    }
}