import 'Styles/Camera/Camera.css';

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import { ULTRALEAP_GREEN } from 'index';

import { ConfigurationManager } from 'TouchFree/Configuration/ConfigurationManager';
import { ConfigState } from 'TouchFree/Connection/TouchFreeServiceTypes';

import CameraBelowIcon from 'Images/Camera/Camera_Below.svg';
import CameraFacingScreenIcon from 'Images/Camera/Camera_Facing_Screen.svg';
import CameraFacingUserIcon from 'Images/Camera/Camera_Facing_User.svg';

import IconTextButton from 'Components/Controls/IconTextButton';

export type PositionType = 'FaceUser' | 'FaceScreen' | 'Below' | null;

interface PositionOption {
    type: PositionType;
    title: string;
    icon: string;
}

const positionOptions: PositionOption[] = [
    { type: 'FaceUser', title: 'Camera Above Facing User', icon: CameraFacingUserIcon },
    { type: 'FaceScreen', title: 'Camera Above Facing Screen', icon: CameraFacingScreenIcon },
    { type: 'Below', title: 'Camera Below', icon: CameraBelowIcon },
];

const buttonStyle: React.CSSProperties = { width: '48.75%', height: '350px', marginBottom: '2.5%' };
const iconStyle: React.CSSProperties = { marginTop: '20px', height: '220px' };
const textStyle = (): React.CSSProperties => {
    return {
        color: ULTRALEAP_GREEN,
        opacity: '1',
    };
};

type PositionSelectionProps = { activePosition: PositionType; setPosition: (position: PositionType) => void };

const PositionSelectionScreen: React.FC<PositionSelectionProps> = ({ activePosition, setPosition }) => {
    useEffect(() => {
        ConfigurationManager.RequestConfigState((config: ConfigState) => {
            setPosition(getPositionFromConfig(config));
        });
    }, []);

    const navigate = useNavigate();
    return (
        <div>
            <div className="titleLine">
                <h1> Where is Your Camera Positioned? </h1>
            </div>
            <div className="IconTextButtonDiv">
                {positionOptions.map(({ type, title, icon }: PositionOption) => (
                    <IconTextButton
                        key={type}
                        buttonStyle={buttonStyle}
                        icon={icon}
                        alt={`Icon for ${title} option`}
                        iconStyle={iconStyle}
                        title={title}
                        text={activePosition === type ? 'Current Setup' : ''}
                        textStyle={textStyle()}
                        onClick={() => {
                            setPosition(type);
                            navigate('calibrate/top');
                        }}
                    />
                ))}
            </div>
        </div>
    );
};

export default PositionSelectionScreen;

const getPositionFromConfig = (config: ConfigState): PositionType => {
    const leapRotation = config.physical.LeapRotationD;
    if (Math.abs(leapRotation.Z) > 90) {
        if (leapRotation.X <= 0) {
            return 'FaceUser';
        }
        return 'FaceScreen';
    }
    return 'Below';
};
