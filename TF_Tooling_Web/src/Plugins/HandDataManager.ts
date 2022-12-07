/**
 * Handles dispatching `"TransmitHandData"` events from received hand frame messages
 * 
 * @internal
 */
export class HandDataManager extends EventTarget {
    
    /** Global static instance of the manager */
    private static _instance: HandDataManager;

    /** Global static for limiting how many frames are handled */
    private static readonly maximumFrameFrequencyMs = 50;

    /**
     * Getter for the global instance. Will initialize if not initialized already.
     */
    public static get instance() {
        if (HandDataManager._instance === undefined) {
            HandDataManager._instance = new HandDataManager();
        }

        return HandDataManager._instance;
    }

    /** Global state for timestamp of last handled hand frame */
    static lastFrame: number | undefined = undefined;

    /**
     * Handles a buffer on hand frame data and dispatches a `"TransmitHandData"` event
     * @param _data Buffer of hand frame data
     */
    public static HandleHandFrame(_data: ArrayBuffer): void {
        const currentTimeStamp = Date.now();
        if (
            !HandDataManager.lastFrame ||
            HandDataManager.lastFrame + HandDataManager.maximumFrameFrequencyMs < currentTimeStamp
        ) {
            const rawHandsEvent: CustomEvent<ArrayBuffer> = new CustomEvent<ArrayBuffer>('TransmitHandData', {
                detail: _data,
            });
            HandDataManager.instance.dispatchEvent(rawHandsEvent);
            HandDataManager.lastFrame = currentTimeStamp;
        }
    }
}
