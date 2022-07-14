import 'Styles/Camera/Camera.css';

import { useNavigate } from 'react-router-dom';

import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';

import { HandSvg } from 'Components/Controls/HandSvg';
import IconTextButton from 'Components/Controls/IconTextButton';

const CameraSetupScreen = () => {
    const navigate = useNavigate();

    const handData = {
        indexTip: { x: 40, y: 10 },
        indexKnuckle: { x: 40, y: 90 },
        middleTip: { x: 70, y: 10 },
        middleKnuckle: { x: 70, y: 90 },
        ringTip: { x: 100, y: 10 },
        ringKnuckle: { x: 100, y: 90 },
        littleTip: { x: 130, y: 10 },
        littleKnuckle: { x: 130, y: 90 },
        thumbTip: { x: 10, y: 40 },
        wristA: { x: 50, y: 160 },
        wristB: { x: 120, y: 160 },
        dotColor: 'blue'
    };

    return (
        <div>
            <div className="titleLine">
                <h1> Camera Setup </h1>
            </div>
            <div className="IconTextButtonDiv">
                <HandSvg {...handData} />
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
