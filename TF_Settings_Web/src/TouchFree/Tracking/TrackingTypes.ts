// Enum: MaskingConfig
// device_id - ID of the camera to apply masking to
// left, right, upper, lower - masking values to apply to each edge of the camera's feed
export interface Mask {
    left: number;
    right: number;
    upper: number;
    lower: number;
}


// Class: TrackingState
// Represents the settings available for modification in the Tracking API
export class TrackingState {
    // Variable: mask
    mask: Mask | null;
    // Variable: cameraOrientation
    cameraReversed: boolean | null;
    // Variable: allowImages
    allowImages: boolean | null;
    // Variable: analyticsEnabled
    analyticsEnabled: boolean | null;

    constructor(
        _mask: Mask | null,
        _cameraReversed: boolean | null,
        _allowImages: boolean | null,
        _analyticsEnabled: boolean | null
    ) {
        this.mask = _mask;
        this.cameraReversed = _cameraReversed;
        this.allowImages = _allowImages;
        this.analyticsEnabled = _analyticsEnabled;
    }
}