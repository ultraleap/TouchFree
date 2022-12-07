/**
 * Masking values to apply to each edge of the camera's feed
 * @public
 */
export interface Mask {
    left: number;
    right: number;
    upper: number;
    lower: number;
}

/**
 * Represents the settings available for modification in the Tracking API
 * @public
 */
export class TrackingState {
    /** Camera masking state */
    mask: Mask;
    /** Is camera orientation reversed from normal? */
    cameraReversed: boolean;
    /** Toggle images being sent from the camera */
    allowImages: boolean;
    /** Toggle analytics */
    analyticsEnabled: boolean;

    constructor(_mask: Mask, _cameraReversed: boolean, _allowImages: boolean, _analyticsEnabled: boolean) {
        this.mask = _mask;
        this.cameraReversed = _cameraReversed;
        this.allowImages = _allowImages;
        this.analyticsEnabled = _analyticsEnabled;
    }
}
