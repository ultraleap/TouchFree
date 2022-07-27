import 'Styles/Camera/Camera.scss';

import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import CameraMaskingIcon from 'Images/Camera/Camera_Masking_Icon.png';
import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';

import { HorizontalIconTextButton, VerticalIconTextButton } from 'Components/Controls/TFButton';

const CameraSetupScreen = () => {
    const navigate = useNavigate();
    const [isFullscreen, setIsFullscreen] = useState(false);

    const resizeHandler = () => {
        if (window.innerWidth === screen.width && window.innerHeight === screen.height) {
            setIsFullscreen(true);
            return;
        }

        setIsFullscreen(false);
    };

    useEffect(() => {
        window.addEventListener('resize', resizeHandler);
        return () => window.removeEventListener('resize', resizeHandler);
    }, []);

    return (
        <div>
            <div className="title-line">
                <h1> Camera Setup </h1>
            </div>
            <div className="tf-button-container">
                <div className="quick-setup-container">
                    <VerticalIconTextButton
                        buttonStyle={{ width: '100%', height: '100%' }}
                        icon={QuickSetupIcon}
                        alt="Icon for Quick Setup option"
                        iconStyle={{ margin: '30px 0px', height: '250px' }}
                        title="Auto Calibration"
                        text="Our automatic calibration enables you to set up quickly"
                        onClick={() => navigate('quick')}
                    />
                    <div className="quick-setup-overlay" style={{ display: isFullscreen ? 'none' : 'flex' }}>
                        <p>{String.fromCodePoint(9432)} Fullscreen recommended for Quick Setup</p>
                        <p>Press F11</p>
                    </div>
                </div>
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
            <HorizontalIconTextButton
                buttonStyle={{}}
                icon={CameraMaskingIcon}
                alt="Icon for Camera Masking"
                iconStyle={{ marginBottom: '0px' }}
                title="Camera Masking"
                text="Mask areas of your cameras vision from reflections and harsh areas of light"
                onClick={() => navigate('masking')}
            />
        </div>
    );
};

export default CameraSetupScreen;
