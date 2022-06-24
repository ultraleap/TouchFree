import '../../../Styles/Camera/CameraPage.css';

import React from 'react';
import { useNavigate } from 'react-router-dom';

import CameraBelowIcon from '../../../Images/Camera/Camera_Below.svg';
import CameraFacingScreenIcon from '../../../Images/Camera/Camera_Facing_Screen.svg';
import CameraFacingUserIcon from '../../../Images/Camera/Camera_Facing_User.svg';
import IconTextButton from '../../Controls/IconTextButton';
import { PositionType } from './CameraPage';

const buttonStyle: React.CSSProperties = { width: '48.75%', height: '350px' };
const iconStyle: React.CSSProperties = { marginTop: '20px', height: '220px' };
const textStyle: React.CSSProperties = { color: '#00EB85', opacity: '1' };

interface CameraPositionProps {
    configPosition: PositionType;
}

const CameraPosition: React.FC<CameraPositionProps> = ({ configPosition }) => {
    const navigate = useNavigate();
    return (
        <div>
            <div className="titleLine">
                <h1> Where is Your Camera Positioned? </h1>
            </div>
            <div className="IconTextButtonDiv">
                <IconTextButton
                    buttonStyle={buttonStyle}
                    icon={CameraFacingUserIcon}
                    alt="Icon for Camera Facing User option"
                    iconStyle={iconStyle}
                    title="Camera Above Facing User"
                    text={configPosition === 'FaceUser' ? 'Current Setup' : ''}
                    textStyle={textStyle}
                    onClick={() => navigate('FaceUser/calibrateTop')}
                />
                <IconTextButton
                    buttonStyle={buttonStyle}
                    icon={CameraFacingScreenIcon}
                    alt="Icon for Camera Facing Screen option"
                    iconStyle={iconStyle}
                    title="Camera Above Facing Screen"
                    text={configPosition === 'FaceScreen' ? 'Current Setup' : ''}
                    textStyle={textStyle}
                    onClick={() => navigate('FaceScreen/calibrateTop')}
                />
                <IconTextButton
                    buttonStyle={{ ...buttonStyle, marginTop: '2.5%' }}
                    icon={CameraBelowIcon}
                    alt="Icon for Camera Below Screen option"
                    iconStyle={iconStyle}
                    title="Camera Below"
                    text={configPosition === 'Below' ? 'Current Setup' : ''}
                    textStyle={textStyle}
                    onClick={() => navigate('Below/calibrateTop')}
                />
            </div>
        </div>
    );
};

export default CameraPosition;
