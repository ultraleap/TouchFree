using System;
using Ultraleap.TouchFree.Library;

namespace Ultraleap.TouchFree.Tooling
{
    public class ConnectionManager
    {
        // Group: Variables

        // Variable: OnConnected
        // An event which is emitted when <Connect> is called.
        //
        // Instead of adding listeners to this event, use <AddConnectionListener> to ensure that your
        // function is invoked if the connection has already been made by the time your class runs.
        public static event Action OnConnected;

        // Variable: currentServiceConnection
        // The private reference to the currently managed <ServiceConnection>.
        static ServiceConnection currentServiceConnection;

        // Variable: serviceConnection
        // The public get-only reference to the currently managed <ServiceConnection>.
        public static ServiceConnection serviceConnection
        {
            get
            {
                return currentServiceConnection;
            }
        }

        // Variable: messageReceiver
        // A reference to the receiver that handles destribution of data received via the <currentServiceConnection> if connected.
        public static MessageReceiver messageReceiver;

        // Variable: HandFound
        // An event allowing users to react to a hand being found when none has been present for a moment.
        public static event Action HandFound;

        // Variable: HandsLost
        // An event allowing users to react to the last hand being lost when one has been present.
        public static event Action HandsLost;

        // Variable: iPAddress
        // The IP Address that will be used in the <ServiceConnection> to connect to the target WebSocket.
        // This value is settable in the Inspector.
        string iPAddress = "127.0.0.1";

        // Variable: port
        // The Port that will be used in the <ServiceConnection> to connect to the target WebSocket.
        // This value is settable in the Inspector.
        string port = "9739";

        // Group: Functions

        // Function: AddConnectionListener
        // Used to both add the _onConnectFunc action to the listeners of <OnConnected>
        // as well as auto-call the _onConnectFunc if a connection is already made.
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

        // Function: HandleHandPresenceEvent
        // Called by the <MessageReciever> to pass HandPresence events via the <HandFound> and
        // <HandsLost> events on this class
        internal static void HandleHandPresenceEvent(HandPresenceState _state)
        {
            if (_state == HandPresenceState.HAND_FOUND)
            {
                HandFound?.Invoke();
            }
            else
            {
                HandsLost?.Invoke();
            }
        }

        // Group: Unity monoBehaviour overrides

        // Function: Awake
        // Run by Unity on Initialization. Finds the required <MessageReceiver> component.
        // Also attempts to immediately <Connect> to a WebSocket.
        public ConnectionManager()
        {
            messageReceiver = new MessageReceiver();
            Connect();
        }

        // Function: OnEnable
        // Unity's OnEnable function for handling when the behaviour is enabled. Connects
        // to SC Service if not already connected.
        private void OnEnable()
        {
            if (currentServiceConnection == null)
            {
                Connect();
            }
        }

        // Function: OnDisable
        // Unity's OnDisable function for handling when the behaviour is disabled. Disconnects
        // from SC Service to prevent caching any new incoming inputs.
        private void OnDisable()
        {
            Disconnect();
        }

        // Function: OnDestroy
        // Unity's Destroy function for handling the deconstruction of a MonoBehaviour.
        // Ensures <Disconnect> is called.
        private void OnDestroy()
        {
            Disconnect();
        }
    }
}
