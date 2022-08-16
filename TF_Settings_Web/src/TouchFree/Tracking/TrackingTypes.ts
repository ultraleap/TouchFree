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
    mask: Mask;
    // Variable: cameraOrientation
    cameraReversed: boolean;
    // Variable: allowImages
    allowImages: boolean;
    // Variable: analyticsEnabled
    analyticsEnabled: boolean;

    constructor(
        _mask: Mask,
        _cameraReversed: boolean,
        _allowImages: boolean,
        _analyticsEnabled: boolean
    ) {
        this.mask = _mask;
        this.cameraReversed = _cameraReversed;
        this.allowImages = _allowImages;
        this.analyticsEnabled = _analyticsEnabled;
    }
}