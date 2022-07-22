import 'Styles/Camera/CameraMasking.scss';

import React, { useEffect, useRef, useState } from 'react';

import SwapMainLensIcon from 'Images/Camera/Swap_Main_Lens_Icon.svg';

import { ToggleSwitch } from 'Components/Controls/ToggleSwitch';

enum Lens {
    Left,
    Right,
}

const CameraMaskingScreen = () => {
    const [mainLens, setMainLens] = useState<Lens>(Lens.Left);
    const [isSubFeedHovered, setIsSubFeedHovered] = useState<boolean>(false);
    // State for Config Options
    const [isCamReversed, _setIsCamReversed] = useState<boolean>(false);
    const [showOverexposed, _setShowOverexposed] = useState<boolean>(false);
    // Refs to be able to use current state in eventListeners
    const isCamReversedRef = useRef(isCamReversed);
    const showOverexposedRef = useRef(showOverexposed);

    // Refs for camera displays
    const leftLensRef = useRef<HTMLCanvasElement>(null);
    const rightLensRef = useRef<HTMLCanvasElement>(null);

    for (let i = 0; i < 256; i++) {
        DataConverter.conversionArray[i] = (255 << 24) | (i << 16) | (i << 8) | i;
        DataConverter.conversionArrayOverExposed[i] =
            i > 224 ? OVEREXPOSED_COLOR : (255 << 24) | (i << 16) | (i << 8) | i;
    }

    // Ref to track if a frame is being rendered so we don't start rendering a new one until the current is complete
    const frameProcessing = useRef<boolean>(false);
    // Ref to track if we have successfully subscribed to camera images
    const successfullySubscribed = useRef<boolean>(false);

    const setIsCameraReversed = (value: boolean) => {
        _setIsCamReversed(value);
        isCamReversedRef.current = value;
    };

    const setShowOverexposedAreas = (value: boolean) => {
        _setShowOverexposed(value);
        showOverexposedRef.current = value;
    };

    useEffect(() => {
        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', () => {
            console.log('WebSocket open');
        });

        socket.addEventListener('message', (event) => {
            if (!leftLensRef.current || !rightLensRef.current || typeof event.data == 'string') return;

            if (frameProcessing.current) {
                return;
            }

            frameProcessing.current = true;

            if (!successfullySubscribed.current) {
                socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
            }

            const data = new DataView(event.data);
            if (data.getUint8(0) === 1) {
                successfullySubscribed.current = true;
                const lensInfo = [
                    { lens: Lens.Left, ref: leftLensRef.current },
                    { lens: Lens.Right, ref: rightLensRef.current },
                ];

                for (const { lens, ref } of lensInfo) {
                    DataConverter.displayLensFeed(
                        data,
                        lens,
                        ref,
                        isCamReversedRef.current,
                        showOverexposedRef.current
                    );
                }
            }
            frameProcessing.current = false;
        });
    }, []);

    return (
        <div>
            <div className="title-line" style={{ flexDirection: 'column' }}>
                <h1> Camera Masking </h1>
                <p style={{ opacity: '50%' }}>
                    The camera will ignore the areas defined by the boxes that you draw on the camera feed
                </p>
            </div>
            <div className="cam-feed-box--main">
                <canvas ref={mainLens === Lens.Left ? leftLensRef : rightLensRef} />
                <p>{Lens[mainLens]} Lens</p>
            </div>
            <div className="cam-feeds-bottom-container">
                <div
                    className="cam-feed-box--sub"
                    onPointerEnter={() => setIsSubFeedHovered(true)}
                    onPointerLeave={() => setIsSubFeedHovered(false)}
                    onPointerDown={() => setMainLens(1 - mainLens)}
                >
                    <canvas ref={mainLens === Lens.Left ? rightLensRef : leftLensRef} />
                    <p>{Lens[1 - mainLens]} Lens</p>
                    <span className="sub-feed-overlay" style={{ opacity: isSubFeedHovered ? 0.85 : 0 }}>
                        <div className="sub-feed-overlay--content">
                            <img src={SwapMainLensIcon} alt="Swap Camera Lens icon" />
                            <p>Swap as main lens view</p>
                        </div>
                    </span>
                </div>
                <div className="cam-feeds-options-container">
                    <CameraMaskingOption
                        title="Reverse Camera Orientation"
                        description="Reverse the camera orientation (hand should enter from the bottom)"
                        value={isCamReversed}
                        onChange={setIsCameraReversed}
                    />
                    <CameraMaskingOption
                        title="Display Overexposed Areas"
                        description="Areas, where hand tracking may be an issue will be highlighted"
                        value={showOverexposed}
                        onChange={setShowOverexposedAreas}
                    />
                </div>
            </div>
        </div>
    );
};

interface CameraMaskingOptionProps {
    title: string;
    description: string;
    value: boolean;
    onChange: (value: boolean) => void;
}

const CameraMaskingOption: React.FC<CameraMaskingOptionProps> = ({ title, description, value, onChange }) => (
    <label className="cam-feeds-option">
        <div className="cam-feeds-option-text">
            <h1>{title}</h1>
            <p>{description}</p>
        </div>
        <div className="cam-feeds-option-toggle">
            <ToggleSwitch value={value} onChange={onChange} />
        </div>
    </label>
);

// Decimal in signed 2's complement
//const OVEREXPOSED_THRESHOLD = -8355712; //#FF808080;
const OVEREXPOSED_COLOR = -13434625; //#FFFF0033;

class DataConverter {
    value = 1;

    static conversionArray = new Uint32Array(256);
    static conversionArrayOverExposed = new Uint32Array(256);

    static displayLensFeed = (
        data: DataView,
        lens: Lens,
        canvas: HTMLCanvasElement,
        isCameraReversed: boolean,
        showOverexposedAreas: boolean
    ) => {
        const context = canvas.getContext('2d');
        if (!context) return;

        const dim1 = data.getUint32(1);
        const dim2 = data.getUint32(5);

        const width = Math.min(dim1, dim2);
        const lensHeight = Math.max(dim1, dim2) / 2;

        const buf = new ArrayBuffer(width * lensHeight * 4);
        const buf8 = new Uint8ClampedArray(buf);
        const buf32 = new Uint32Array(buf);

        const rotated90 = dim2 < dim1;

        const offset = lens === Lens.Right ? 0 : rotated90 ? lensHeight : width * lensHeight;

        const conversionMethod = showOverexposedAreas
            ? DataConverter.convertByteToHexOverExposed
            : DataConverter.convertByteToHex;

        if (rotated90) {
            for (let rowIndex = 0; rowIndex < width; rowIndex++) {
                const rowBase = rowIndex * lensHeight;
                const start = 9 + offset + rowBase * 2;
                const dataRow = data.buffer.slice(start, start + lensHeight);
                const rowView = new DataView(dataRow);

                for (let i = 0; i < lensHeight; i++) {
                    const px = rowView.getUint8(i);
                    buf32[i + rowBase] = conversionMethod(px);
                }
            }
        } else {
            const offsetView = new DataView(data.buffer.slice(9 + offset, 9 + width * lensHeight + offset));
            for (let i = 0; i < width * lensHeight; i++) {
                const px = offsetView.getUint8(i);
                buf32[i] = conversionMethod(px);
            }
        }
        // Set black pixels to remove flashing camera bytes
        const startOffset = isCameraReversed ? 0 : (lensHeight - 1) * width;
        buf32.fill(0xff000000, startOffset, startOffset + width);

        canvas.width = width;
        canvas.height = lensHeight;
        context.putImageData(new ImageData(buf8, width, lensHeight), 0, 0);
    };

    static convertByteToHex = (byte: number): number => {
        return DataConverter.conversionArray[byte];
    };

    static convertByteToHexOverExposed = (byte: number): number => {
        return DataConverter.conversionArrayOverExposed[byte];
    };
}

export default CameraMaskingScreen;
