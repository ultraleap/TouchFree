import 'Styles/Camera/Camera.css';

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';

import IconTextButton from 'Components/Controls/IconTextButton';

const CameraSetupScreen = () => {
    const videoRef = React.useRef<HTMLVideoElement>(null);

    useEffect(() => {
        // getVideo();
    }, [videoRef]);

    const getVideo = () => {
        navigator.mediaDevices.enumerateDevices().then((list) => {
            // console.log(list);
            const cam = list.find((device) => device.label.includes('Leap Motion'));
            // console.log(cam);
            if (!cam) {
                console.error('Can not find device');
                return;
            }
            navigator.mediaDevices
                .getUserMedia({ video: { deviceId: cam.deviceId } })
                // .getUserMedia({ video: true })
                .then((stream) => {
                    console.log('TEST');
                    console.log(stream);
                    const video = videoRef.current;
                    if (!video) return;
                    video.srcObject = stream;
                    video.play();
                })
                .catch((error) => {
                    console.error(error.name + ': ' + error.message);
                });
        });
    };

    let device: USBDevice;

    const getUSB = () => {
        return;
        navigator.usb.requestDevice({ filters: [{ vendorId: 10550 }] }).then(function (device) {
            console.log(device);
        });
        navigator.usb
            .getDevices()
            .then((deviceList) => {
                device = deviceList[0];
                console.log(device);
                return device.open(); // Begin a session.
            })
            .then(() => device.selectConfiguration(1))
            // VDIEO INTERFACES CANNOT BE CLAIMED BY WEBUSB
            // "An attempt to claim a USB device interface has been blocked because it
            // implements a protected interface class."
            .then(() => device.claimInterface(0))

            .catch((error) => {
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
            <video ref={videoRef} style={{ width: '100%', height: '300px', marginTop: '100px' }} />
            <button
                style={{ width: '500px', height: '100px', marginTop: '100px', background: 'pink' }}
                // onClick={getUSB}
                onClick={getVideo}
            >
                CLICK FOR CAMERA?
            </button>
        </div>
    );
};

export default CameraSetupScreen;
