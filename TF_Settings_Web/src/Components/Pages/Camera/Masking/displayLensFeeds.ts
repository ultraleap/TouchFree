import { Lens } from './MaskingScreen';

export const displayLensFeeds = (
    data: DataView,
    canvasRef: HTMLCanvasElement,
    lens: Lens,
    isCameraReversed: boolean,
    byteConversionArray: Uint32Array
) => {
    const context = canvasRef.getContext('2d');
    if (!context) return;

    const dim1 = data.getUint32(1);
    const dim2 = data.getUint32(5);

    const width = Math.min(dim1, dim2);
    const lensHeight = Math.max(dim1, dim2) / 2;

    const buf = new ArrayBuffer(width * lensHeight * 4);
    const buf8 = new Uint8ClampedArray(buf);
    const buf32 = new Uint32Array(buf);

    const rotated90 = dim2 < dim1;
    const offset = 9;

    if (rotated90) {
        let rowBase = 0;
        const offsetView = new DataView(data.buffer.slice(offset, offset + width * lensHeight * 2));

        for (let rowIndex = 0; rowIndex < width; rowIndex++) {
            let rowStart = rowBase * 2;
            if (lens === 'Right') {
                for (let i = 0; i < lensHeight; i++) {
                    buf32[i + rowBase] = byteConversionArray[offsetView.getUint8(i + rowStart)];
                }
            }

            rowStart += lensHeight;
            if (lens === 'Left') {
                for (let i = 0; i < lensHeight; i++) {
                    buf32[i + rowBase] = byteConversionArray[offsetView.getUint8(i + rowStart)];
                }
            }

            rowBase += lensHeight;
        }
    } else {
        if (lens === 'Right') {
            const offsetView = new DataView(data.buffer.slice(offset, offset + width * lensHeight));

            for (let i = 0; i < width * lensHeight; i++) {
                buf32[i] = byteConversionArray[offsetView.getUint8(i)];
            }
        }

        if (lens === 'Left') {
            const offsetView = new DataView(
                data.buffer.slice(offset + width * lensHeight, offset + width * lensHeight * 2)
            );
            for (let i = 0; i < width * lensHeight; i++) {
                buf32[i] = byteConversionArray[offsetView.getUint8(i)];
            }
        }
    }
    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : (lensHeight - 1) * width;
    buf32.fill(0xff000000, startOffset, startOffset + width);

    canvasRef.width = width;
    canvasRef.height = lensHeight;
    context.putImageData(new ImageData(buf8, width, lensHeight), 0, 0);
};
