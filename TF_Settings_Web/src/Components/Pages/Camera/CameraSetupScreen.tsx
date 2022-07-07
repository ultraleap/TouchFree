import 'Styles/Camera/Camera.css';

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';

import IconTextButton from 'Components/Controls/IconTextButton';

const CameraSetupScreen = () => {
    const videoRef = React.useRef<HTMLVideoElement>(null);

    useEffect(() => {
        getVideo();
    }, [videoRef]);

    const getVideo = () => {
        navigator.mediaDevices.enumerateDevices().then((list) => {
            // console.log(list);
            const device = list.find((device) => device.label.includes('Leap Motion'));
            if (!device) {
                console.error('Can not find device');
                return;
            }
            navigator.mediaDevices
                .getUserMedia({ video: { deviceId: device.deviceId } })
                .then((stream) => {
                    const video = videoRef.current;
                    if (!video) return;
                    video.srcObject = stream;
                    video.play();
                })
                .catch((error) => {
                    // SEE IF TRACKING SERVICE IS CLAIMING CAMERA
                    console.error('error:', error);
                });
        });
    };

    const getUSB = () => {
        navigator.usb
            .getDevices()
            .then((list) => {
                console.log(list);
            })
            .catch((error) => {
                // SEE IF TRACKING SERVICE IS CLAIMING CAMERA
                console.error('error:', error);
            });
    };

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
            <video ref={videoRef} style={{ height: '300px', marginTop: '100px' }} />
            <button
                style={{ width: '500px', height: '100px', marginTop: '100px', background: 'pink' }}
                onClick={getUSB}
            >
                CLICK FOR USB
            </button>
        </div>
    );
};

export default CameraSetupScreen;
