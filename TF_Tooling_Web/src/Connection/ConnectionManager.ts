
import { MessageReceiver } from "./MessageReceiver";
import { ServiceConnection } from "./ServiceConnection";
import { HandPresenceState, ServiceStatus } from "./TouchFreeServiceTypes";

// Class: ConnectionManager
// This Class manages the connection to the Service. It provides static variables
// for ease of use and is a Singleton to allow for easy referencing.
export class ConnectionManager extends EventTarget {

    // Group: Events

    // Event: OnConnected
    // An event which is emitted when <Connect> is called.
    //
    // Instead of adding listeners to this event, use <AddConnectionListener> to ensure that your
    // function is invoked if the connection has already been made by the time your class runs.

    // Group: Variables

    // Variable: currentServiceConnection
    // The private reference to the currently managed <ServiceConnection>.
    private static currentServiceConnection: ServiceConnection | null;

    // Variable: serviceConnection
    // The public get-only reference to the currently managed <ServiceConnection>.
    public static serviceConnection(): ServiceConnection | null {
        return ConnectionManager.currentServiceConnection;
    };

    // Variable: messageReceiver
    // A reference to the receiver that handles destribution of data received via the <currentServiceConnection> if connected.
    public static messageReceiver: MessageReceiver;

    // Variable: instance
    // The instance of the singleton for referencing the events transmitted
    public static instance: ConnectionManager;

    // Variable: iPAddress
    // The IP Address that will be used in the <ServiceConnection> to connect to the target
    // WebSocket. This value is settable in the Inspector.
    static iPAddress: string = "127.0.0.1";

    // Variable: port
    // The Port that will be used in the <ServiceConnection> to connect to the target WebSocket.
    // This value is settable in the Inspector.
    static port: string = "9739";

    private handEventTimeout: number = -1;

    // Group: Functions

    // Function: init
    // Used to begin the connection. Creates the required <MessageReceiver>.
    // Also attempts to immediately <Connect> to a WebSocket.
    public static init() {
        ConnectionManager.messageReceiver = new MessageReceiver();
        ConnectionManager.instance = new ConnectionManager();
        ConnectionManager.Connect();
    }

    // Function: AddConnectionListener
    // Used to both add the _onConnectFunc action to the listeners of <OnConnected>
    // as well as auto-call the _onConnectFunc if a connection is already made.
    public static AddConnectionListener(_onConnectFunc: () => void): void {
        ConnectionManager.instance.addEventListener('OnConnected', _onConnectFunc);

        if (ConnectionManager.currentServiceConnection !== null &&
            ConnectionManager.currentServiceConnection.webSocket.readyState === WebSocket.OPEN) {
            _onConnectFunc();
        }
    }

    // Function: Connect
    // Creates a new <ServiceConnection> using <iPAddress> and <port>.
    // Also invokes <OnConnected> on all listeners.
    public static Connect(): void {
        ConnectionManager.currentServiceConnection = new ServiceConnection(
            ConnectionManager.iPAddress,
            ConnectionManager.port);
    }

    // Function: HandleHandPresenceEvent
    // Called by the <MessageReciever> to pass HandPresence events via the <HandFound> and
    // <HandsLost> events on this class
    public static HandleHandPresenceEvent(_state: HandPresenceState): void {
        let handPresenceEvent: CustomEvent;

        if (_state === HandPresenceState.HAND_FOUND) {
            if (ConnectionManager.instance.handEventTimeout !== -1) {
                clearTimeout(ConnectionManager.instance.handEventTimeout);
            } 
            handPresenceEvent = new CustomEvent('HandFound');
            ConnectionManager.instance.dispatchEvent(handPresenceEvent);
        } else {
            ConnectionManager.instance.handEventTimeout = window.setTimeout(() => {
                handPresenceEvent = new CustomEvent('HandsLost');
                ConnectionManager.instance.dispatchEvent(handPresenceEvent);
                ConnectionManager.instance.handEventTimeout = -1;
            }, 1500);
        }
    }

    // Function: Disconnect
    // Disconnects <currentServiceConnection> if it is connected to a WebSocket and
    // sets it to null.
    public static Disconnect(): void {
        if (ConnectionManager.currentServiceConnection !== null) {
            ConnectionManager.currentServiceConnection.Disconnect();
            ConnectionManager.currentServiceConnection = null;
        }
    }

    // Function: RequestServiceStatus
    // Used to request information from the Service via the <ConnectionManager>. Provides an asynchronous
    // <ServiceStatus> via the _callback parameter.
    //
    // If your _callback requires context it should be bound to that context via .bind()
    public static RequestServiceStatus(_callback: (detail: ServiceStatus) => void): void {
        if (_callback === null) {
            console.error("Request failed. This is due to a missing callback");
            return;
        }

        ConnectionManager.serviceConnection()?.RequestServiceStatus(_callback);
    }
}