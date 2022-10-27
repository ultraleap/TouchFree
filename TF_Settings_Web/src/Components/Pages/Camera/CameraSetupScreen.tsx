import 'Styles/Camera/Camera.scss';

import { useNavigate } from 'react-router-dom';

import CameraMaskingIcon from 'Images/Camera/Camera_Masking_Icon.png';
import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';

import { HorizontalIconTextButton, VerticalIconTextButton } from 'Components/Controls/TFButton';

const CameraSetupScreen = () => {
    const navigate = useNavigate();

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
                        opacity: 1,
                        pointerEvents: 'auto',
                    }}
                    icon={CameraMaskingIcon}
                    alt="Icon for Camera Masking"
                    iconStyle={{ marginBottom: '0px' }}
                    title="Camera Masking"
                    text="Mask areas of your cameras vision from reflections and harsh areas of light"
                    onClick={() => navigate('masking')}
                />
            </div>
        </div>
    );
};

export default CameraSetupScreen;
