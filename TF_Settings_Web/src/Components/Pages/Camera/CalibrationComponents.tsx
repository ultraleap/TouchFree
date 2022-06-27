import '../../../Styles/Camera/Calibrate.css';

import React from 'react';
import { CSSProperties } from 'react';
import { useNavigate } from 'react-router-dom';

import FingerprintIcon from '../../../Images/Camera/Fingerprint_Icon.svg';
import IconTextButton from '../../Controls/IconTextButton';

export const CalibrateInstructions = () => {
    return (
        <div className="instructions">
            <h1>
                Hold INDEX FINGER against
                <br /> this <span style={{ color: '#01EB85' }}>GREEN CIRCLE</span>
            </h1>
            <div className="arrow">
                <div id="downLine" />
                <div id="arrowHead" />
            </div>
            <div id="touchCircle">
                <img src={FingerprintIcon} alt="Fingerprint Icon showing where to place finger for Quick Setup" />
            </div>
        </div>
    );
};

const setupButtonStyle: CSSProperties = {
    width: '300px',
    height: '80px',
    borderRadius: '33px',
    background: 'transparent linear-gradient(180deg, #5c5c5c 0%, #454545 100%) 0% 0% no-repeat padding-box',
};

const setupButtonTitleStyle: CSSProperties = {
    fontSize: '2rem',
    padding: '0',
    textAlign: 'center',
    margin: '0',
    height: '100%',
};

interface CalibrateCancelButtonProps {
    buttonStyle: CSSProperties;
}

export const CalibrateCancelButton: React.FC<CalibrateCancelButtonProps> = ({ buttonStyle }) => {
    const navigate = useNavigate();
    return (
        <IconTextButton
            buttonStyle={{ ...setupButtonStyle, ...buttonStyle }}
            icon={''}
            alt=""
            title="Cancel Setup"
            titleStyle={setupButtonTitleStyle}
            text={''}
            textStyle={{ display: 'none' }}
            onClick={() => navigate('/camera/quick/')}
        />
    );
};

export const CalibratePracticeButton = () => {
    const [hovered, setHovered] = React.useState<boolean>(false);
    const [pressed, setPressed] = React.useState<boolean>(false);

    return (
        <button
            className={`setupPracticeButton ${hovered ? ' setupPracticeButtonHovered' : ''} ${
                pressed ? ' setupPracticeButtonPressed' : ''
            }`}
            onPointerOver={() => setHovered(true)}
            onPointerLeave={() => {
                setHovered(false);
                setPressed(false);
            }}
            onPointerDown={() => setPressed(true)}
            onPointerUp={() => {
                setPressed(false);
                console.log('CONFETTI');
            }}
        >
            Practice Button Press
        </button>
    );
};
