import { VerticalIconTextButton } from '@/Components';
import { CameraFacingUserIcon, CameraFacingScreenIcon, CameraBelowIcon } from '@/Images';

import '@/Pages/Camera/Camera.scss';
import cssVariables from '@/variables.module.scss';

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import { ConfigurationManager } from 'TouchFree/src/Configuration/ConfigurationManager';
import { ConfigState } from 'TouchFree/src/Connection/TouchFreeServiceTypes';

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
const textStyle: React.CSSProperties = {
    color: cssVariables.ultraleapGreen,
    opacity: '1',
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
            <div className="title-line">
                <h1> Where is Your Camera Positioned? </h1>
            </div>
            <div className="title-line" style={{ fontStyle: 'italic', paddingBottom: '1vh', marginTop: '-0.5vh' }}>
                <p>Full screen is recommended for optimal calibration</p>
            </div>
            <div className="tf-button-container">
                {positionOptions.map(({ type, title, icon }: PositionOption) => (
                    <VerticalIconTextButton
                        key={type}
                        buttonStyle={buttonStyle}
                        icon={icon}
                        alt={`Icon for ${title} option`}
                        iconStyle={iconStyle}
                        title={title}
                        text={activePosition === type ? 'Current Setup' : ''}
                        textStyle={textStyle}
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
