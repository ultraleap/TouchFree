import React from 'react';

import '../../../Styles/Camera/CameraPage.css';

import QuickSetupIcon from '../../../Images/Quick_Setup_Icon.svg';
import ManualSetupIcon from '../../../Images/Manual_Setup_Icon.svg';
import IconTextButton from '../../Controls/IconTextButton';
import { useNavigate } from 'react-router-dom';

const CameraSetupSelection = () => {
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
                    iconStyle={{ margin: '30px 0px', height: '250px' }}
                    title="Auto Calibration"
                    text="Our automatic calibration enables you to set up quickly"
                    onClick={() => navigate('quick')}
                />
                <IconTextButton
                    buttonStyle={{ width: '33.75%' }}
                    icon={ManualSetupIcon}
                    iconStyle={{ margin: '65px 0px', height: '180px' }}
                    title="Manual Calibration"
                    text="Full control of your calibration"
                    onClick={() => navigate('manual')}
                />
            </div>
        </div>
    );
};

export default CameraSetupSelection;
