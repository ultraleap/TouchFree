// Enum: MaskingConfig
// device_id - ID of the camera to apply masking to
// left, right, upper, lower - masking values to apply to each edge of the camera's feed
export interface Mask {
    left: number;
    right: number;
    upper: number;
    lower: number;
}
