import Alert from '@/Components/Controls/Alert';
import { ConnectionManager } from 'TouchFree/src/Connection/ConnectionManager';

import 'Styles/Camera/Camera.scss';

import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import CameraMaskingIcon from 'Images/Camera/Camera_Masking_Icon.png';
import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';

import { HorizontalIconTextButton, VerticalIconTextButton } from 'Components/Controls/TFButton';

const CameraSetupScreen = () => {
    const navigate = useNavigate();

    const [touchFreeConnected, setTouchFreeConnected] = useState<boolean>(false);

    const onConnected = () => {
        setTouchFreeConnected(true);
    };

    useEffect(() => {
        ConnectionManager.AddConnectionListener(onConnected);
    }, []);

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
                        opacity: touchFreeConnected ? 1 : 0.5,
                        pointerEvents: touchFreeConnected ? 'auto' : 'none',
                    }}
                    icon={CameraMaskingIcon}
                    alt="Icon for Camera Masking"
                    iconStyle={{ marginBottom: '0px' }}
                    title="Camera Masking"
                    text="Mask areas of your cameras vision from reflections and harsh areas of light"
                    onClick={() => navigate('masking')}
                />
                <Alert
                    show={!touchFreeConnected}
                    cssWidth={'60%'}
                    text="Failed to connect to service: masking unavailable"
                    animationType="fadeIn"
                    animationTime={1}
                />
            </div>
        </div>
    );
};

export default CameraSetupScreen;
