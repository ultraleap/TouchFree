import 'Styles/Camera/Camera.scss';

import React, { useEffect, useState } from 'react';

import { ToggleSwitch } from 'Components/Controls/ToggleSwitch';

type CameraType = 'left' | 'right';

const CameraMaskingScreen = () => {
    const [isCameraReversed, setIsCameraReversed] = useState<boolean>(true);

    const topVideoRef = React.useRef<HTMLCanvasElement>(null);
    const botVideoRef = React.useRef<HTMLCanvasElement>(null);

    const frameCount = React.useRef<number>(0);

    useEffect(() => {
        console.log('USEEFFECT');
        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.close();
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', () => {
            console.log('WebSocket open');
            socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
        });

        socket.addEventListener('message', (event) => {
            if (!topVideoRef.current || !botVideoRef.current || typeof event.data == 'string') return;
            frameCount.current++;
            if (frameCount.current > 1) {
                if (frameCount.current == 200) {
                    frameCount.current = 0;
                }
                return;
            }
            const data = new DataView(event.data);
            if (data.getUint8(0) === 1) {
                console.log('HAS INT');
                displayCameraFeed(data, 'left', topVideoRef.current, isCameraReversed);
                displayCameraFeed(data, 'right', botVideoRef.current, isCameraReversed);
            } else {
                console.log('NOT INT: ' + data.getUint8(0));
                // console.log(data.getUint8(100));
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
                <canvas ref={topVideoRef} />
                <p>Left Lens</p>
            </div>
            <div className="cam-feeds-bottom-container">
                <div className="cam-feed-box--sub">
                    <canvas ref={botVideoRef} />
                    <p>Right Lens</p>
                </div>
                <div className="cam-feeds-options-container">
                    <label className="cam-feeds-option">
                        <div className="cam-feeds-option-text">
                            <h1>Reverse Camera Orientation</h1>
                            <p>Reverse the camera orientation (hand should enter from the bottom)</p>
                        </div>
                        <div className="cam-feeds-option-toggle">
                            <ToggleSwitch value={isCameraReversed} onChange={(value) => setIsCameraReversed(value)} />
                        </div>
                    </label>
                    <div className="cam-feeds-option"></div>
                </div>
            </div>
        </div>
    );
};

// Decimal in signed 2's complement
const OVEREXPOSED_THRESHOLD = -8355712; //#FF808080;
const OVEREXPOSED_COLOR = -13434625; //#FFFF0033;

const displayCameraFeed = (
    data: DataView,
    camera: CameraType,
    canvas: HTMLCanvasElement,
    isCameraReversed: boolean
) => {
    const context = canvas.getContext('2d');
    if (!context) return;

    const width = data.getUint32(1);
    const cameraHeight = data.getUint32(5) / 2;

    const buf = new ArrayBuffer(width * cameraHeight * 4);
    const buf8 = new Uint8ClampedArray(buf);
    const buf32 = new Uint32Array(buf);

    const offset = camera === 'right' ? 0 : width * cameraHeight;

    for (let i = 0; i < width * cameraHeight; i++) {
        const px = data.getUint8(9 + i + offset);
        const hexColor = (255 << 24) | (px << 16) | (px << 8) | px;
        buf32[i] = hexColor < OVEREXPOSED_THRESHOLD ? hexColor : OVEREXPOSED_COLOR;
    }
    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : (cameraHeight - 1) * width;
    buf32.fill(0xff000000, startOffset, startOffset + width);

    canvas.width = width;
    canvas.height = cameraHeight;
    context.putImageData(new ImageData(buf8, width, cameraHeight), 0, 0);
};

export default CameraMaskingScreen;
