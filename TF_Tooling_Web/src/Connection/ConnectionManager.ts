import TouchFree from '../TouchFree';
import { TrackingServiceState } from '../TouchFreeToolingTypes';
import { MessageReceiver } from './MessageReceiver';
import { ServiceConnection } from './ServiceConnection';
import { HandPresenceState, InteractionZoneState, ServiceStatus } from './TouchFreeServiceTypes';

export interface Address {
    ip?: string;
    port?: string;
}

interface InitParams {
    address?: Address;
}

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
    }

    // Variable: messageReceiver
    // A reference to the receiver that handles distribution of data received
    // via the <currentServiceConnection> if connected.
    public static messageReceiver: MessageReceiver;

    // Variable: instance
    // The instance of the singleton for referencing the events transmitted
    public static instance: ConnectionManager;

    // Variable: iPAddress
    // The IP Address that will be used in the <ServiceConnection> to connect to the target
    // WebSocket. This value is settable in the Inspector.
    static iPAddress = '127.0.0.1';

    // Variable: port
    // The Port that will be used in the <ServiceConnection> to connect to the target WebSocket.
    // This value is settable in the Inspector.
    static port = '9739';

    // Variable: currentHandPresence
    // Private reference to the current hand presence state
    private static currentHandPresence: HandPresenceState = HandPresenceState.HANDS_LOST;

    // Variable: currentInteractionZoneState
    // Private reference to the current interaction zone state
    private static currentInteractionZoneState: InteractionZoneState = InteractionZoneState.HAND_EXITED;

    // Group: Functions

    // Function: init
    // Used to begin the connection. Creates the required <MessageReceiver>.
    // Also attempts to immediately <Connect> to a WebSocket.
    public static init(initParams?: InitParams) {
        if (initParams?.address) {
            ConnectionManager.SetAddress(initParams.address);
        }
        ConnectionManager.messageReceiver = new MessageReceiver();
        ConnectionManager.instance = new ConnectionManager();
        ConnectionManager.Connect();
    }

    // Function: AddConnectionListener
    // Used to both add the _onConnectFunc action to the listeners of <OnConnected>
    // as well as auto-call the _onConnectFunc if a connection is already made.
    public static AddConnectionListener(_onConnectFunc: () => void): void {
        TouchFree.RegisterEventCallback('WhenConnected', _onConnectFunc);
    }

    public static get IsConnected(): boolean {
        return (
            ConnectionManager.currentServiceConnection !== null &&
            ConnectionManager.currentServiceConnection.webSocket.readyState === WebSocket.OPEN &&
            ConnectionManager.currentServiceConnection.handshakeComplete
        );
    }

    public static AddServiceStatusListener(_serviceStatusFunc: (serviceStatus: TrackingServiceState) => void): void {
        TouchFree.RegisterEventCallback('OnTrackingServiceStateChange', _serviceStatusFunc);
    }

    // Function: Connect
    // Creates a new <ServiceConnection> using <iPAddress> and <port>.
    // Also invokes <OnConnected> on all listeners.
    public static Connect(): void {
        ConnectionManager.currentServiceConnection = new ServiceConnection(
            ConnectionManager.iPAddress,
            ConnectionManager.port
        );
    }

    // Function: HandleHandPresenceEvent
    // Called by the <MessageReciever> to pass HandPresence events via the <HandFound> and
    // <HandsLost> events on this class
    public static HandleHandPresenceEvent(_state: HandPresenceState): void {
        ConnectionManager.currentHandPresence = _state;

        if (_state === HandPresenceState.HAND_FOUND) {
            TouchFree.DispatchEvent('HandFound');
        } else {
            TouchFree.DispatchEvent('HandsLost');
        }
    }

    // Function: HandleInteractionZoneEvent
    // Called by the <MessageReciever> to pass InteractionZone events via the <HandEntered> and
    // <HandsExited> events on this class
    public static HandleInteractionZoneEvent(_state: InteractionZoneState): void {
        ConnectionManager.currentInteractionZoneState = _state;

        if (_state === InteractionZoneState.HAND_ENTERED) {
            TouchFree.DispatchEvent('HandEntered');
        } else {
            TouchFree.DispatchEvent('HandExited');
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
            console.error('Request failed. This is due to a missing callback');
            return;
        }

        ConnectionManager.serviceConnection()?.RequestServiceStatus(_callback);
    }

    // Function: GetCurrentHandPresence
    // Function to get the current hand presense state
    public static GetCurrentHandPresence(): HandPresenceState {
        return ConnectionManager.currentHandPresence;
    }

    // Function: GetCurrentInteractionZoneState
    // Function to get the current hand presense state
    public static GetCurrentInteractionZoneState(): InteractionZoneState {
        return ConnectionManager.currentInteractionZoneState;
    }

    // Function: SetAddress
    // Function to set the ip and port that Tooling should attempt to connect to the Service via
    public static SetAddress(address: Address): void {
        ConnectionManager.iPAddress = address.ip ?? '127.0.0.1';
        ConnectionManager.port = address.port ?? '9739';
        ConnectionManager.Connect();
    }
}
