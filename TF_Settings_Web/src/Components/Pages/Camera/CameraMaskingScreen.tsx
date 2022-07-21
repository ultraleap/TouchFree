import 'Styles/Camera/Camera.scss';

import React, { useEffect, useRef, useState } from 'react';

import { ToggleSwitch } from 'Components/Controls/ToggleSwitch';

enum Lens {
    LEFT,
    RIGHT,
}

const CameraMaskingScreen = () => {
    // State for Config Options
    const [isCamReversed, _setIsCamReversed] = useState<boolean>(false);
    const [showOverexposed, _setShowOverexposed] = useState<boolean>(false);
    // Refs to be able to use current state in eventListeners
    const isCamReversedRef = useRef(isCamReversed);
    const showOverexposedRef = useRef(showOverexposed);

    // Refs for camera displays
    const mainLensRef = useRef<HTMLCanvasElement>(null);
    const subLensRef = useRef<HTMLCanvasElement>(null);

    // Ref to track frames so we only display every X frames to save rendering power
    const frameCount = useRef<number>(0);

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
        // socket.close();
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', () => {
            console.log('WebSocket open');
            socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
        });

        socket.addEventListener('message', (event) => {
            if (!mainLensRef.current || !subLensRef.current || typeof event.data == 'string') return;
            frameCount.current++;
            if (frameCount.current > 1) {
                if (frameCount.current == 10) {
                    frameCount.current = 0;
                }
                return;
            }

            const data = new DataView(event.data);
            if (data.getUint8(0) === 1) {
                const lensInfo = [
                    { lens: Lens.LEFT, ref: mainLensRef.current },
                    { lens: Lens.RIGHT, ref: subLensRef.current },
                ];

                for (const { lens, ref } of lensInfo) {
                    displayLensFeed(data, lens, ref, isCamReversedRef.current, showOverexposedRef.current);
                }
            } else {
                console.log('NOT INT: ' + data.getUint8(0));
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
                <canvas ref={mainLensRef} />
                <p>Left Lens</p>
            </div>
            <div className="cam-feeds-bottom-container">
                <div className="cam-feed-box--sub" onClick={() => console.log('SWAP  LENS')}>
                    <canvas ref={subLensRef} />
                    <p>Right Lens</p>
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

    const width = data.getUint32(1);
    const lensHeight = data.getUint32(5) / 2;

    const buf = new ArrayBuffer(width * lensHeight * 4);
    const buf8 = new Uint8ClampedArray(buf);
    const buf32 = new Uint32Array(buf);

    const offset = lens === Lens.RIGHT ? 0 : width * lensHeight;

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
