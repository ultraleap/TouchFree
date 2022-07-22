import 'Styles/Camera/CameraMasking.scss';

import React, { PointerEvent, useEffect, useRef, useState } from 'react';

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

    const byteConversionArray = new Uint32Array(256);
    const byteConversionArrayOverExposed = new Uint32Array(256);

    for (let i = 0; i < 256; i++) {
        byteConversionArray[i] = (255 << 24) | (i << 16) | (i << 8) | i;
        // -13434625 = #FFFF0033 in signed 2's complement
        byteConversionArrayOverExposed[i] = i > 224 ? -13434625 : byteConversionArray[i];
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

                displayLensFeeds(
                    data,
                    leftLensRef.current,
                    rightLensRef.current,
                    isCamReversedRef.current,
                    showOverexposedRef.current ? byteConversionArrayOverExposed : byteConversionArray
                );
            }

            // Settimeout with 32ms for ~30fps if we have the performance
            setTimeout(() => {
                frameProcessing.current = false;
            }, 32);
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
                <CameraMaskingSlider direction="left" />
                <CameraMaskingSlider direction="right" />
                <CameraMaskingSlider direction="top" />
                <CameraMaskingSlider direction="bottom" />
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

type Direction = 'left' | 'right' | 'top' | 'bottom';
interface SliderPosInfo {
    X: number;
    Y: number;
    initialTop: number;
    initialLeft: number;
}

const CameraMaskingSlider: React.FC<{ direction: Direction }> = ({ direction }) => {
    const startPosInfo = useRef<SliderPosInfo>({
        X: Number.NaN,
        Y: Number.NaN,
        initialTop: Number.NaN,
        initialLeft: Number.NaN,
    });

    const sliderRef = useRef<HTMLSpanElement>(null);

    const onStartDrag = (event: PointerEvent<HTMLSpanElement>) => {
        if (!sliderRef.current) return;

        if (Number.isNaN(startPosInfo.current.X) || Number.isNaN(startPosInfo.current.Y)) {
            const top = Number.parseFloat(getComputedStyle(sliderRef.current).top);
            const left = Number.parseFloat(getComputedStyle(sliderRef.current).left);

            startPosInfo.current = { X: event.pageX, Y: event.pageY, initialTop: top, initialLeft: left };
        }

        window.addEventListener('pointermove', onMove);
        window.addEventListener('pointerup', onEndDrag);
    };

    const onMove = (event: globalThis.PointerEvent) => {
        if (!sliderRef.current) return;

        switch (direction) {
            case 'left':
                sliderRef.current.style.left = `${
                    startPosInfo.current.initialLeft + limitVal(event.pageX - startPosInfo.current.X)
                }px`;
                break;
            case 'right':
                sliderRef.current.style.left = `${
                    startPosInfo.current.initialLeft - limitVal(startPosInfo.current.X - event.pageX)
                }px`;
                break;
            case 'top':
                sliderRef.current.style.top = `${
                    startPosInfo.current.initialTop + limitVal(event.pageY - startPosInfo.current.Y)
                }px`;
                break;
            case 'bottom':
                sliderRef.current.style.top = `${
                    startPosInfo.current.initialTop - limitVal(startPosInfo.current.Y - event.pageY)
                }px`;
                break;
        }
    };

    const onEndDrag = (event: globalThis.PointerEvent) => {
        window.removeEventListener('pointermove', onMove);
        window.removeEventListener('pointerup', onEndDrag);

        let dragVal = 0;
        switch (direction) {
            case 'left':
                dragVal = limitVal(event.pageX - startPosInfo.current.X);
                break;
            case 'right':
                dragVal = limitVal(startPosInfo.current.X - event.pageX);
                break;
            case 'top':
                dragVal = limitVal(event.pageY - startPosInfo.current.Y);
                break;
            case 'bottom':
                dragVal = limitVal(startPosInfo.current.Y - event.pageY);
                break;
        }

        console.log('Drag end: ' + dragVal);
    };

    return (
        <span ref={sliderRef} className={`masking-slider--${direction}`} onPointerDown={onStartDrag}>
            <div className="masking-slider--knob" />
        </span>
    );
};

const limitVal = (val: number): number => Math.min(360, Math.max(0, val));

const displayLensFeeds = (
    data: DataView,
    leftLensRef: HTMLCanvasElement,
    rightLensRef: HTMLCanvasElement,
    isCameraReversed: boolean,
    byteConversionArray: Uint32Array
) => {
    const leftContext = leftLensRef.getContext('2d');
    const rightContext = rightLensRef.getContext('2d');
    if (!leftContext || !rightContext) return;

    const dim1 = data.getUint32(1);
    const dim2 = data.getUint32(5);

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
        const offsetView = new DataView(data.buffer.slice(offset, offset + width * lensHeight * 2));

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
        let offsetView = new DataView(data.buffer.slice(offset, offset + width * lensHeight));

        for (let i = 0; i < width * lensHeight; i++) {
            rightBuf32[i] = byteConversionArray[offsetView.getUint8(i)];
        }

        offsetView = new DataView(data.buffer.slice(offset + width * lensHeight, offset + width * lensHeight * 2));
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

export default CameraMaskingScreen;
