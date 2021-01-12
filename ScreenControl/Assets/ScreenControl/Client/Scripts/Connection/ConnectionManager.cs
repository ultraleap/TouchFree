using System;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client.Connection
{
    // Class: ConnectionManager
    // This Class manages the connection to the Service. It provides static variables
    // for ease of use and is a Singleton to allow for easy referencing.
    public class ConnectionManager : MonoBehaviour
    {
        // Group: Variables

        // Variable: Instance
        // A static instance of the <ConnectionManager> to provide it as a singleton.
        public static ConnectionManager Instance;

        // Variable: OnConnected
        // An event which can be listneed to to know when a connection has been made.
        // Invoked when <Connect> is called.
        public static event Action OnConnected;

        // Variable: currentServiceConnection
        // The private reference to the currently managed <ServiceConnection>.
        static ServiceConnection currentServiceConnection;
        // Variable: serviceConnection
        // The public get-only reference to the currently managed <ServiceConnection>.
        public static ServiceConnection serviceConnection {
            get {
                return currentServiceConnection;
            }
        }

        // Variable: currentServiceConnection
        // The IP Address that will be used in the <ServiceConnection> to connect to the target WebSocket.
        // This value is settable in the Inspector.
        [Header("WebSocket connection values")]
        [SerializeField] string iPAddress = "127.0.0.1";
        // Variable: currentServiceConnection
        // The Port that will be used in the <ServiceConnection> to connect to the target WebSocket.
        // This value is settable in the Inspector.
        [SerializeField] string port = "9739";

        // Group: Functions

        // Function: Awake
        // Run by Unity on Initialization. Sets up the Singleton of this Class.
        // Also attempts to immediately <Connect> to a WebSocket.
        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                return;
            }
            Instance = this;

            Connect();
        }

        // Function: AddConnectionListener
        // Used to both add the <_onConnectFunc> action to the listeners of <OnConnected>
        // as well as auto-call the <_onConnectFunc> if a connection is already made.
        public static void AddConnectionListener(Action _onConnectFunc)
        {
            OnConnected += _onConnectFunc;

            if (currentServiceConnection != null)
            {
                _onConnectFunc();
            }
        }

        // Function: Connect
        // Creates a new <ServiceConnection> using <iPAddress> and <port>.
        // Also invokes <OnConnected> on all listeners.
        public void Connect()
        {
            currentServiceConnection = new ServiceConnection(iPAddress, port);
            OnConnected?.Invoke();
        }

        // Function: OnDestroy
        // Unitys Destroy function for handling the deconstruction of a MonoBehaviour.
        // Ensures <Disconnect> is called.
        private void OnDestroy()
        {
            Disconnect();
        }

        // Function: Disconnect
        // Disconnects <currentServiceConnection> if it is connected to a WebSocket and
        // sets it to null.
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