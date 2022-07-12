import 'Styles/Camera/Camera.css';

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';

import IconTextButton from 'Components/Controls/IconTextButton';

const CameraSetupScreen = () => {
    const videoRef = React.useRef<HTMLCanvasElement>(null);

    useEffect(() => {
        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', () => {
            socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
        });

        socket.addEventListener('message', (event) => {
            if (!videoRef.current) return;
            const data = new DataView(event.data);
            if (data.getUint8(0) === 1) {
                // Image data
                const width = data.getUint32(1);
                const height = data.getUint32(5);

                const buf = new ArrayBuffer(width * height * 4);
                const buf8 = new Uint8ClampedArray(buf);
                const buf32 = new Uint32Array(buf);

                for (let i = 0; i < width * height; i++) {
                    const px = data.getUint8(6 + i);
                    buf32[i] = (255 << 24) | (px << 16) | (px << 8) | px;
                }

                const image = new ImageData(buf8, width, height);
                videoRef.current.width = width;
                videoRef.current.height = height;
                const context = videoRef.current.getContext('2d');
                if (context) {
                    context.putImageData(image, 0, 0);
                }
            }
        });
    });

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
            <canvas ref={videoRef} style={{ width: '80%', height: '800px', marginTop: '50px' }} />
        </div>
    );
};

export default CameraSetupScreen;
