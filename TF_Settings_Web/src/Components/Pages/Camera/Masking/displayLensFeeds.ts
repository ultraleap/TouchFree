export const displayLensFeeds = (
    data: ArrayBuffer,
    leftLensRef: HTMLCanvasElement,
    rightLensRef: HTMLCanvasElement,
    isCameraReversed: boolean,
    byteConversionArray: Uint32Array
) => {
    const leftContext = leftLensRef.getContext('2d');
    const rightContext = rightLensRef.getContext('2d');
    if (!leftContext || !rightContext) return;

    const startOfBuffer = new DataView(data.slice(0, 10));

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
        let rowBase = 0;
        const offsetView = new DataView(data.slice(offset, offset + width * lensHeight * 2));

        for (let rowIndex = 0; rowIndex < width; rowIndex++) {
            let rowStart = rowBase * 2;
            for (let i = 0; i < lensHeight; i++) {
                rightBuf32[i + rowBase] = byteConversionArray[offsetView.getUint8(i + rowStart)];
            }

            rowStart += lensHeight;
            for (let i = 0; i < lensHeight; i++) {
                leftBuf32[i + rowBase] = byteConversionArray[offsetView.getUint8(i + rowStart)];
            }

            rowBase += lensHeight;
        }
    } else {
        let offsetView = new DataView(data.slice(offset, offset + width * lensHeight));

        for (let i = 0; i < width * lensHeight; i++) {
            rightBuf32[i] = byteConversionArray[offsetView.getUint8(i)];
        }

        offsetView = new DataView(data.slice(offset + width * lensHeight, offset + width * lensHeight * 2));
        for (let i = 0; i < width * lensHeight; i++) {
            leftBuf32[i] = byteConversionArray[offsetView.getUint8(i)];
        }
    }
    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : (lensHeight - 1) * width;
    rightBuf32.fill(0xff000000, startOffset, startOffset + width);
    leftBuf32.fill(0xff000000, startOffset, startOffset + width);

    leftLensRef.width = width;
    leftLensRef.height = lensHeight;
    leftContext.putImageData(new ImageData(leftBuf8, width, lensHeight), 0, 0);

    rightLensRef.width = width;
    rightLensRef.height = lensHeight;
    rightContext.putImageData(new ImageData(rightBuf8, width, lensHeight), 0, 0);
};
