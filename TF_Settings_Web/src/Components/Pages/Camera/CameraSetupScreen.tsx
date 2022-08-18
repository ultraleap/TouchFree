import 'Styles/Camera/Camera.scss';

import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import CameraMaskingIcon from 'Images/Camera/Camera_Masking_Icon.png';
import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';

import Alert from 'Components/Controls/Alert';
import { HorizontalIconTextButton, VerticalIconTextButton } from 'Components/Controls/TFButton';

type TrackingServiceStatus = 'connecting' | 'connected' | 'closed';

const CameraSetupScreen = () => {
    const [trackingServiceStatus, setTrackingServiceStatus] = useState<TrackingServiceStatus>('connecting');
    const socket = useRef<WebSocket>();
    const timeoutRef = useRef<number>();

    const navigate = useNavigate();

    const connectToTrackingService = () => {
        socket.current = new WebSocket('ws://127.0.0.1:1024');
        socket.current.addEventListener('open', handleWSOpen);
        socket.current.addEventListener('close', handleWSClose);
    };

    useEffect(() => {
        connectToTrackingService();

        return () => {
            if (socket.current) {
                socket.current.removeEventListener('open', handleWSOpen);
                socket.current.removeEventListener('close', handleWSClose);
            }
            window.clearTimeout(timeoutRef.current);
        };
    }, []);

    const handleWSOpen = () => setTrackingServiceStatus('connected');
    const handleWSClose = () => {
        setTrackingServiceStatus('closed');
        timeoutRef.current = window.setTimeout(connectToTrackingService, 4000);
    };

    return (
        <div>
            <div className="title-line">
                <h1> Camera Setup </h1>
            </div>
            <div className="tf-button-container">
                <VerticalIconTextButton
                    buttonStyle={{ width: '63.75%' }}
                    icon={QuickSetupIcon}
                    alt="Icon for Quick Setup option"
                    iconStyle={{ margin: '30px 0px', height: '250px' }}
                    title="Auto Calibration"
                    text="Our automatic calibration enables you to set up quickly"
                    onClick={() => navigate('quick')}
                />
                <VerticalIconTextButton
                    buttonStyle={{ width: '33.75%' }}
                    icon={ManualSetupIcon}
                    alt="Icon for Manual Setup option"
                    iconStyle={{ margin: '65px 0px', height: '180px' }}
                    title="Manual Calibration"
                    text="Full control of your calibration"
                    onClick={() => navigate('manual')}
                />
            </div>
            <div className="camera-page-divider" />
            <div className="title-line">
                <h1> Tools </h1>
            </div>
            <div style={{ position: 'relative' }}>
                <HorizontalIconTextButton
                    buttonStyle={{
                        position: 'relative',
                        opacity: trackingServiceStatus === 'connected' ? 1 : 0.5,
                        pointerEvents: trackingServiceStatus === 'connected' ? 'auto' : 'none',
                    }}
                    icon={CameraMaskingIcon}
                    alt="Icon for Camera Masking"
                    iconStyle={{ marginBottom: '0px' }}
                    title="Camera Masking"
                    text="Mask areas of your cameras vision from reflections and harsh areas of light"
                    onClick={() => navigate('masking')}
                />
                <Alert
                    show={trackingServiceStatus === 'closed'}
                    cssWidth={'60%'}
                    text="Failed to connect to tracking service: masking unavailable"
                    animationType="fadeIn"
                    animationTime={1}
                />
            </div>
        </div>
    );
};

export default CameraSetupScreen;
