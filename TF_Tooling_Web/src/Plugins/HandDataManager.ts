// Class: HandDataManager
// The manager for all <HandFrame> to be handled and distributed. This distributes the data
// via the <TransmitHandData> event which should be listened to by any class hoping to make
// use of incoming <HandFrame>s.
export class HandDataManager extends EventTarget {
    // Event: TransmitHandData
    // An event for transmitting <HandFrame>s that are received via the <messageReceiver> to
    // be listened to.

    // Variable: instance
    // The instance of the singleton for referencing the events transmitted
    private static _instance: HandDataManager;

    private static readonly maximumFrameFrequencyMs = 16;

    public static get instance() {
        if (HandDataManager._instance === undefined) {
            HandDataManager._instance = new HandDataManager();
        }

        return HandDataManager._instance;
    }

    static lastFrame: number | undefined = undefined;

    // Function: HandleHandFrame
    // Called by the <messageReceiver> to relay a <HandFrame> that has been received to any
    // listeners of <TransmitHandData>.
    public static HandleHandFrame(_data: ArrayBuffer): void {
        const currentTimeStamp = Date.now();
        if (!HandDataManager.lastFrame || HandDataManager.lastFrame + HandDataManager.maximumFrameFrequencyMs < currentTimeStamp ) {
            let rawHandsEvent: CustomEvent<ArrayBuffer> = new CustomEvent<ArrayBuffer>(
                'TransmitHandData',
                { detail: _data }
            );
            HandDataManager.instance.dispatchEvent(rawHandsEvent);
            HandDataManager.lastFrame = currentTimeStamp;
        }
    }
}