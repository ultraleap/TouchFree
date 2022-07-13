import 'Styles/Camera/Camera.css';

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';

import IconTextButton from 'Components/Controls/IconTextButton';

type CameraType = 'left' | 'right';

const CameraSetupScreen = () => {
    const topVideoRef = React.useRef<HTMLCanvasElement>(null);
    const botVideoRef = React.useRef<HTMLCanvasElement>(null);

    const frameCount = React.useRef<number>(0);

    useEffect(() => {
        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', () => {
            socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
        });

        // setInterval(() => {
        //     console.log(frameCount.current);
        //     frameCount.current = 0;
        // }, 1000);

        socket.addEventListener('message', (event) => {
            if (!topVideoRef.current || !botVideoRef.current || typeof event.data == 'string') return;
            frameCount.current++;
            if (frameCount.current > 1) {
                if (frameCount.current == 10) {
                    frameCount.current = 0;
                }
                return;
            }
            const data = new DataView(event.data);
            if (data.getUint8(0) === 1) {
                displayCameraFeed(data, 'left', topVideoRef.current);
                displayCameraFeed(data, 'right', botVideoRef.current);
            }
        });
    }, []);

    const navigate = useNavigate();
    return (
        <div>
            <div className="titleLine">
                <h1> Camera Setup </h1>
            </div>
            <div className="IconTextButtonDiv">
                <IconTextButton
                    buttonStyle={{ width: '63.75%' }}
                    icon={QuickSetupIcon}
                    alt="Icon for Quick Setup option"
                    iconStyle={{ margin: '30px 0px', height: '250px' }}
                    title="Auto Calibration"
                    text="Our automatic calibration enables you to set up quickly"
                    onClick={() => navigate('quick')}
                />
                <IconTextButton
                    buttonStyle={{ width: '33.75%' }}
                    icon={ManualSetupIcon}
                    alt="Icon for Manual Setup option"
                    iconStyle={{ margin: '65px 0px', height: '180px' }}
                    title="Manual Calibration"
                    text="Full control of your calibration"
                    onClick={() => navigate('manual')}
                />
            </div>
            <canvas ref={topVideoRef} style={{ width: '400px', height: '400px', margin: '50px' }} />
            <canvas ref={botVideoRef} style={{ width: '400px', height: '400px', margin: '50px' }} />
        </div>
    );
};

export default CameraSetupScreen;

const displayCameraFeed = (data: DataView, camera: CameraType, canvas: HTMLCanvasElement) => {
    const width = data.getUint32(1);
    const cameraHeight = data.getUint32(5) / 2;

    const buf = new ArrayBuffer(width * cameraHeight * 4);
    const buf8 = new Uint8ClampedArray(buf);
    const buf32 = new Uint32Array(buf);

    const offset = camera === 'right' ? 0 : width * cameraHeight;

    for (let i = width; i < width * cameraHeight; i++) {
        const px = data.getUint8(9 + i + offset);
        buf32[i] = (255 << 24) | (px << 16) | (px << 8) | px;
    }

    const context = canvas.getContext('2d');
    if (context) {
        canvas.width = width;
        canvas.height = cameraHeight;
        context.putImageData(new ImageData(buf8, width, cameraHeight), 0, 0);
    }
};
