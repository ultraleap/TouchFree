let conversionArraysInitialised = false;
const byteConversionArray = new Uint32Array(256);
const byteConversionArrayOverExposed = new Uint32Array(256);

export const displayLensFeeds = (
    data: ArrayBuffer,
    leftLensRef: HTMLCanvasElement | null,
    rightLensRef: HTMLCanvasElement | null,
    isCameraReversed: boolean,
    showOverexposedAreas: boolean
) => {
    const leftContext = getContext(leftLensRef);
    const rightContext = getContext(rightLensRef);
    if (!leftContext && !rightContext) return;

    if (!conversionArraysInitialised) {
        for (let i = 0; i < 256; i++) {
            byteConversionArray[i] = (255 << 24) | (i << 16) | (i << 8) | i;
            // -13434625 = #FFFF0033 in signed 2's complement
            byteConversionArrayOverExposed[i] = i > 128 ? -13434625 : byteConversionArray[i];
        }
        conversionArraysInitialised = true;
    }

    const conversionArrayToUse = showOverexposedAreas ? byteConversionArrayOverExposed : byteConversionArray;

    const startOfBuffer = new DataView(data, 0, 10);

    const dim1 = startOfBuffer.getUint32(1);
    const dim2 = startOfBuffer.getUint32(5);

    const width = Math.min(dim1, dim2);
    const lensHeight = Math.max(dim1, dim2) / 2;

    const leftBuf = new ArrayBuffer(width * lensHeight * 4);
    const leftBuf8 = new Uint8ClampedArray(leftBuf);
    const leftBuf32 = new Uint32Array(leftBuf);

    const rightBuf = new ArrayBuffer(width * lensHeight * 4);
    const rightBuf8 = new Uint8ClampedArray(rightBuf);
    const rightBuf32 = new Uint32Array(rightBuf);

    const rotated90 = dim2 < dim1;
    const offset = 9;

    if (rotated90) {
        processRotatedScreen(data, rightBuf32, leftBuf32, conversionArrayToUse, offset, width, lensHeight);
    } else {
        processScreen(data, rightBuf32, leftBuf32, conversionArrayToUse, offset, width, lensHeight);
    }
    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : (lensHeight - 1) * width;
    rightBuf32.fill(0xff000000, startOffset, startOffset + width);
    leftBuf32.fill(0xff000000, startOffset, startOffset + width);

    if (leftLensRef && leftContext) {
        putBufferToImage(leftLensRef, leftContext, leftBuf8, width, lensHeight);
    }

    if (rightLensRef && rightContext) {
        putBufferToImage(rightLensRef, rightContext, rightBuf8, width, lensHeight);
    }
};

const putBufferToImage = (
    lensRef: HTMLCanvasElement,
    context: CanvasRenderingContext2D,
    buf8: Uint8ClampedArray,
    width: number,
    lensHeight: number
) => {
    lensRef.width = width;
    lensRef.height = lensHeight;
    context.putImageData(new ImageData(buf8, width, lensHeight), 0, 0);
};

const processRotatedScreen = (
    data: ArrayBuffer,
    rightBuf32: Uint32Array,
    leftBuf32: Uint32Array,
    byteConversionArray: Uint32Array,
    offset: number,
    width: number,
    lensHeight: number
) => {
    let rowBase = 0;
    const offsetView = new Uint8Array(data, offset, width * lensHeight * 2);

    for (let rowIndex = 0; rowIndex < width; rowIndex++) {
        let rowStart = rowBase * 2;
        for (let i = 0; i < lensHeight; i++) {
            rightBuf32[i + rowBase] = byteConversionArray[offsetView[i + rowStart]];
        }

        rowStart += lensHeight;
        for (let i = 0; i < lensHeight; i++) {
            leftBuf32[i + rowBase] = byteConversionArray[offsetView[i + rowStart]];
        }

        rowBase += lensHeight;
    }
};

const processScreen = (
    data: ArrayBuffer,
    rightBuf32: Uint32Array,
    leftBuf32: Uint32Array,
    byteConversionArray: Uint32Array,
    offset: number,
    width: number,
    lensHeight: number
) => {
    const offsetViewRight = new Uint8Array(data, offset, width * lensHeight);
    const offsetViewLeft = new Uint8Array(data, offset + width * lensHeight, width * lensHeight);
    for (let i = 0; i < width * lensHeight; i++) {
        rightBuf32[i] = byteConversionArray[offsetViewRight[i]];
        leftBuf32[i] = byteConversionArray[offsetViewLeft[i]];
    }
};

const canvasElements: HTMLCanvasElement[] = [];
const canvasContexts: CanvasRenderingContext2D[] = [];

const getContext = (canvasElement: HTMLCanvasElement | null) => {
    if (!canvasElement) {
        return null;
    }
    const canvasIndex = canvasElements.indexOf(canvasElement);
    if (canvasIndex < 0) {
        const context = canvasElement.getContext('2d');
        if (context === null) {
            return context;
        }
        const newIndex = canvasElements.length;
        canvasElements[newIndex] = canvasElement;
        canvasContexts[newIndex] = context;
        return context;
    }
    return canvasContexts[canvasIndex];
};
