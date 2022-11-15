import { HandState } from './createHandData';
import { renderScene, updateCameraRender } from './sceneRendering';

let conversionArraysInitialised = false;
const byteConversionArray = new Uint32Array(256);
const byteConversionArrayOverExposed = new Uint32Array(256);

// Declared for performance
const width = 384;
const halfWidth = width / 2;
const doubleWidth = width * 2;

let cameraBuffer: ArrayBuffer;
let buf8: Uint8Array;
let buf32: Uint32Array;

export const updateCameraCanvas = (
    data: ArrayBuffer,
    isCameraReversed: boolean,
    showOverexposedAreas: boolean,
    handData: HandState
) => {
    if (!conversionArraysInitialised) {
        for (let i = 0; i < 256; i++) {
            byteConversionArray[i] = (255 << 24) | (i << 16) | (i << 8) | i;
            // -13434625 = #FFFF0033 in signed 2's complement
            byteConversionArrayOverExposed[i] = i > 128 ? -13434625 : byteConversionArray[i];
        }
        conversionArraysInitialised = true;
    }

    const conversionArrayToUse = showOverexposedAreas ? byteConversionArrayOverExposed : byteConversionArray;

    const lensHeight = width;

    if (!cameraBuffer) {
        cameraBuffer = new ArrayBuffer(width * lensHeight);
        buf8 = new Uint8Array(cameraBuffer);
        buf32 = new Uint32Array(cameraBuffer);
    }

    processCameraFrame(data, buf32, conversionArrayToUse);

    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : (lensHeight - 1) * width;
    buf32.fill(0xff000000, startOffset, startOffset + width);

    updateCameraRender(buf8, width / 2, lensHeight / 2, handData);
    renderScene();
};

const processCameraFrame = (data: ArrayBuffer, buf32: Uint32Array, byteConversionArray: Uint32Array) => {
    const offsetView = new Uint8Array(data);

    for (let i = 0; i < halfWidth; i++) {
        const qwOffset = i * halfWidth;
        const wOffset = i * doubleWidth;
        for (let j = 0; j < halfWidth; j++) {
            buf32[qwOffset + j] = byteConversionArray[offsetView[wOffset + j * 2]];
        }
    }
};
