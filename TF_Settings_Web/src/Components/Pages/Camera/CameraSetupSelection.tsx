import React from 'react';

import '../../../Styles/Camera/CameraPage.css';

import QuickSetupIcon from '../../../Images/Quick_Setup_Icon.svg';
import ManualSetupIcon from '../../../Images/Manual_Setup_Icon.svg';
import IconTextButton from '../../Controls/IconTextButton';
import { SetupType } from './CameraPage';

interface CameraSetupSelectionProps {
    onClick: (position: SetupType) => void;
}

const CameraSetupSelection: React.FC<CameraSetupSelectionProps> = ({ onClick }) => {
    return (
        <div className="page">
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
                    onClick={() => onClick('Quick')}
                />
                <IconTextButton
                    buttonStyle={{ width: '33.75%' }}
                    icon={ManualSetupIcon}
                    iconStyle={{ margin: '65px 0px', height: '180px' }}
                    title="Manual Calibration"
                    text="Full control of your calibration"
                    onClick={() => onClick('Manual')}
                />
            </div>
        </div>
    );
};

export default CameraSetupSelection;
