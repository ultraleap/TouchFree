import './Camera.scss';

import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { QuickSetupIcon, ManualSetupIcon, CameraMaskingIcon } from '@/Images';

import { Alert, DocsLink, HorizontalIconTextButton, VerticalIconTextButton } from '@/Components';

interface CameraSetupScreenProps {
    trackingStatus: TrackingServiceState;
}

const CameraSetupScreen: React.FC<CameraSetupScreenProps> = ({ trackingStatus }) => {
    const [isConnected, setIsConnected] = useState<boolean>(false);
    const navigate = useNavigate();

    useEffect(() => {
        setIsConnected(trackingStatus !== TrackingServiceState.UNAVAILABLE);
    }, [trackingStatus]);

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
            <div className="page-divider" />
            <div className="title-line">
                <h1> Tools </h1>
            </div>
            <div style={{ position: 'relative' }}>
                <HorizontalIconTextButton
                    buttonStyle={{
                        position: 'relative',
                        opacity: isConnected ? 1 : 0.5,
                        pointerEvents: isConnected ? 'auto' : 'none',
                    }}
                    icon={CameraMaskingIcon}
                    alt="Icon for Camera Masking"
                    iconStyle={{ marginBottom: '0px' }}
                    title="Camera Masking"
                    text="Mask areas of your cameras vision from reflections and harsh areas of light"
                    onClick={() => navigate('masking')}
                />
                <Alert
                    show={!isConnected}
                    cssWidth={'60%'}
                    text="Failed to connect to service: masking unavailable"
                    animationType="fadeIn"
                    animationTime={1}
                />
            </div>
            <DocsLink title="Support" url="https://www.ultraleap.com/contact-us/" />
        </div>
    );
};

export default CameraSetupScreen;
