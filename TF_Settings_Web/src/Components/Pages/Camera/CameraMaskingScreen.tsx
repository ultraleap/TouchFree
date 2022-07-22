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

    // Ref to track frames so we only display every X frames to save rendering power
    const frameCount = useRef<number>(0);
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
            frameCount.current++;
            if (frameCount.current > 1) {
                if (frameCount.current == 10) {
                    frameCount.current = 0;
                }
                return;
            }
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
                    displayLensFeed(data, lens, ref, isCamReversedRef.current, showOverexposedRef.current);
                }
            }
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

const CameraMaskingSlider: React.FC<{ direction: Direction }> = ({ direction }) => {
    const [isDragging, setIsDragging] = useState<boolean>(false);
    const [startPos, setStartPos] = useState<{ X: number; Y: number }>({ X: 0, Y: 0 });

    const sliderRef = useRef<HTMLSpanElement>(null);

    const onStartDrag = (event: PointerEvent<HTMLDivElement>) => {
        setIsDragging(true);
        if (startPos.X === 0) {
            setStartPos({ X: event.pageX, Y: event.pageY });
        }

        console.log('START X: ' + event.pageX + '    START Y: ' + event.pageY);
    };

    const onMove = (event: PointerEvent<HTMLDivElement>) => {
        if (!isDragging || !sliderRef.current) return;
        const elem = event.target as HTMLDivElement;
        // UP = negative
        // LEFT = positive

        // console.log('START X: ' + startPos.X + '    START Y: ' + startPos.Y);

        // moving up page = -offsetY
        let offsetY = event.pageY - startPos.Y;
        offsetY = Math.min(0, offsetY);
        offsetY = Math.max(-350, offsetY);

        console.log('X: ' + offsetY + '    Y: ' + event.pageY);

        // sliderRef.current.style.transform = `translateY(${offsetY}px)`;
        sliderRef.current.style.top = `${900 + offsetY}px`;
    };

    const onEndDrag = (event: PointerEvent<HTMLDivElement>) => {
        console.log('END X: ' + event.pageX + '    END Y: ' + event.pageY);

        setIsDragging(false);
        // setStartPos({ X: event.pageX, Y: event.pageY });
    };

    return (
        <span ref={sliderRef} className={`masking-slider--${direction}`} onPointerMove={onMove}>
            <div
                className="masking-slider--knob"
                onPointerDown={onStartDrag}
                onPointerUp={onEndDrag}
                onPointerCancel={onEndDrag}
            />
        </span>
    );
};

// Decimal in signed 2's complement
const OVEREXPOSED_THRESHOLD = -8355712; //#FF808080;
const OVEREXPOSED_COLOR = -13434625; //#FFFF0033;

const displayLensFeed = (
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

    const offset = lens === Lens.Right ? 0 : width * lensHeight;

    for (let i = 0; i < width * lensHeight; i++) {
        const px = data.getUint8(9 + i + offset);
        const hexColor = (255 << 24) | (px << 16) | (px << 8) | px;
        buf32[i] = showOverexposedAreas && hexColor > OVEREXPOSED_THRESHOLD ? OVEREXPOSED_COLOR : hexColor;
    }
    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : (lensHeight - 1) * width;
    buf32.fill(0xff000000, startOffset, startOffset + width);

    canvas.width = width;
    canvas.height = lensHeight;
    context.putImageData(new ImageData(buf8, width, lensHeight), 0, 0);
};

export default CameraMaskingScreen;
