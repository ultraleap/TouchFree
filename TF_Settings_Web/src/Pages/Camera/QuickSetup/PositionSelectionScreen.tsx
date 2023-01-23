import styles from './PositionSelection.module.scss';
import cssVariables from '@/variables.module.scss';

import classnames from 'classnames/bind';
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import { useIsLandscape } from '@/customHooks';

import { ConfigurationManager } from 'TouchFree/src/Configuration/ConfigurationManager';
import { ConfigState } from 'TouchFree/src/Connection/TouchFreeServiceTypes';

import { CameraFacingUserIcon, CameraFacingScreenIcon, CameraBelowIcon } from '@/Images';

import { BackButton, DocsLink, VerticalIconTextButton } from '@/Components';

const classes = classnames.bind(styles);

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

const buttonStyle: React.CSSProperties = { width: '100%', height: '350px', marginBottom: '2.5%' };
const iconStyle: React.CSSProperties = { marginTop: '20px', height: '220px' };
const textStyle: React.CSSProperties = {
    color: cssVariables.ultraleapGreen,
    opacity: '1',
};

type PositionSelectionProps = { activePosition: PositionType; setPosition: (position: PositionType) => void };

const PositionSelectionScreen: React.FC<PositionSelectionProps> = ({ activePosition, setPosition }) => {
    const isLandscape = useIsLandscape();

    useEffect(() => {
        ConfigurationManager.RequestConfigState((config: ConfigState) => {
            setPosition(getPositionFromConfig(config));
        });
    }, []);

    const navigate = useNavigate();
    return (
        <div className={classes('container')}>
            <div className={classes('header')}>
                <div>
                    <div className={classes('title-line')}>
                        <h1> Where is Your Camera Positioned? </h1>
                    </div>
                    <div
                        className={classes('title-line')}
                        style={{ fontStyle: 'italic', paddingBottom: '1vh', marginTop: '-0.5vh' }}
                    >
                        <p>Full screen is recommended for optimal calibration</p>
                    </div>
                </div>
                {isLandscape ? <></> : <BackButton />}
            </div>
            <div className={classes('button-container')}>
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
                <div></div>
                {isLandscape ? <BackButton /> : <></>}
            </div>
            <DocsLink
                title="Setup Guide"
                url="https://docs.ultraleap.com/touchfree-user-manual/camera-placement"
                buttonStyle={{ position: 'fixed', bottom: '2vh', right: '2vh' }}
            />
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
