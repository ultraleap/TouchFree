import 'Styles/Camera/Camera.css';

import IconTextButton from 'Components/Controls/IconTextButton';
import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';
import { useNavigate } from 'react-router-dom';

const CameraSetupScreen = () => {
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
        </div>
    );
};

export default CameraSetupScreen;
