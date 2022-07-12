import 'Styles/Camera/Calibrate.css';
import 'react-circular-progressbar/dist/styles.css';

import { CreateTypes } from 'canvas-confetti';
import React, { useRef } from 'react';
import { CSSProperties } from 'react';
import ReactCanvasConfetti from 'react-canvas-confetti';
import { buildStyles, CircularProgressbar } from 'react-circular-progressbar';
import { CSSTransition, SwitchTransition } from 'react-transition-group';

import { APP_HEIGHT, APP_WIDTH, ULTRALEAP_GREEN } from 'index';

import TutorialPoster from 'Images/Camera/Calibration_Tutorial_Poster.png';
import FingerprintIcon from 'Images/Camera/Fingerprint_Icon.svg';
import HandIcon from 'Images/Tracking_Status_Icon.svg';
import TutorialVideo from 'Videos/Calibration_Tutorial.mp4';

import IconTextButton from 'Components/Controls/IconTextButton';

interface CalibrationInstructionsProps {
    progress: number;
    containerStyle: CSSProperties;
}
export const CalibrationInstructions: React.FC<CalibrationInstructionsProps> = ({ progress, containerStyle }) => {
    const instructionsText = (
        <h1>
            Hold INDEX FINGER against <br /> this <span className="greenText">GREEN CIRCLE</span>
        </h1>
    );

    const calibratingText = (
        <h1 className="greenText">
            <div style={{ display: 'flex', height: '62px' }}>
                <span style={{ width: '25%', paddingLeft: '37.5%' }}>Calibrating</span>
                <span style={{ width: '4%', textAlign: 'left' }} className="loading" />
            </div>
            {Math.ceil(progress * 100).toFixed(0)}%
        </h1>
    );

    return (
        <div className="instructions" style={containerStyle}>
            <SwitchTransition>
                <CSSTransition
                    key={progress > 0 ? 'calibratingText' : 'instructionText'}
                    addEndListener={(node, done) => node.addEventListener('transitionend', done, false)}
                    classNames="fade"
                >
                    {progress > 0 ? calibratingText : instructionsText}
                </CSSTransition>
            </SwitchTransition>
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

interface CalibrationProgressCircleProps {
    progress: number;
    style: CSSProperties;
}

export const CalibrationProgressCircle: React.FC<CalibrationProgressCircleProps> = ({ progress, style }) => (
    <div style={style} className="progressCircleContainer">
        <CircularProgressbar
            value={Math.ceil(progress * 50) / 50}
            maxValue={1}
            strokeWidth={25}
            styles={buildStyles({
                strokeLinecap: 'butt',
                pathColor: ULTRALEAP_GREEN,
                trailColor: 'transparent',
                pathTransitionDuration: 0.08,
            })}
        />
    </div>
);

export const CalibrationHandLostMessage = () => (
    <div className={'handNotFoundContainer'}>
        <img src={HandIcon} alt="Hand Icon" />
        <p>Hand Not Detected</p>
    </div>
);

interface CalibrationTutorialVideoProps {
    videoStyle: CSSProperties;
}

export const CalibrationTutorialVideo: React.FC<CalibrationTutorialVideoProps> = ({ videoStyle }) => (
    <video
        className="interactionGuide"
        style={videoStyle}
        autoPlay={true}
        loop={true}
        src={TutorialVideo}
        poster={TutorialPoster}
    />
);

const setupButtonStyle: CSSProperties = {
    width: '300px',
    height: '80px',
    borderRadius: '40px',
    background: 'transparent linear-gradient(180deg, #5c5c5c 0%, #454545 100%) 0% 0% no-repeat padding-box',
};

const setupButtonTitleStyle: CSSProperties = {
    fontSize: '2rem',
    padding: '0',
    textAlign: 'center',
    margin: '0',
    height: '100%',
};

interface CalibrationCancelButtonProps {
    onCancel: () => void;
    buttonStyle: CSSProperties;
}

export const CalibrationCancelButton: React.FC<CalibrationCancelButtonProps> = ({ onCancel, buttonStyle }) => {
    return (
        <IconTextButton
            buttonStyle={{ ...setupButtonStyle, ...buttonStyle }}
            icon={''}
            alt=""
            title="Cancel Setup"
            titleStyle={setupButtonTitleStyle}
            text={''}
            textStyle={{ display: 'none' }}
            onClick={() => onCancel()}
            canHover={false}
        />
    );
};

const canvasStyles = (): CSSProperties => {
    return {
        position: 'fixed',
        pointerEvents: 'none',
        width: APP_WIDTH,
        height: APP_HEIGHT,
        top: 0,
        left: 0,
    };
};

interface CalibrationPracticeButtonProps {
    progress: number;
}

export const CalibrationPracticeButton: React.FC<CalibrationPracticeButtonProps> = ({ progress }) => {
    const [hovered, setHovered] = React.useState<boolean>(false);
    const numFired = useRef<number>(0);

    const refAnimationInstance = useRef<CreateTypes | null>(null);
    const fire = () => {
        // Limit number of concurrent confetti events to prevent page from becoming unresponsive
        if (numFired.current > 2) return;

        refAnimationInstance.current &&
            refAnimationInstance.current({
                spread: 360,
                startVelocity: 50,
                origin: { y: 0.42 },
                particleCount: 50,
                gravity: 0.4,
                scalar: 1.5,
            });

        numFired.current++;

        setTimeout(() => {
            numFired.current--;
        }, 3000);
    };

    return (
        <>
            <button
                className={`setupPracticeButton ${hovered ? ' setupPracticeButtonHovered' : ''}`}
                style={progressStyle(progress, hovered)}
                onPointerOver={() => setHovered(true)}
                onPointerLeave={() => setHovered(false)}
                onPointerDown={() => fire()}
                onKeyDown={(keyEvent) => {
                    if (keyEvent.key === 'Enter') fire();
                }}
            >
                Practice
                <br />
                Button Press
            </button>
            <ReactCanvasConfetti
                refConfetti={(instance) => (refAnimationInstance.current = instance)}
                style={canvasStyles()}
            />
        </>
    );
};

const progressStyle = (progress: number, isHovered: boolean): CSSProperties => {
    if (!isHovered) {
        return {};
    }
    if (progress < 0.9) {
        return { transform: `scale(${1.3 - progress * 0.4})` };
    }
    return {
        transform: `scale(${1.3 - progress * 0.4})`,
        background: 'transparent linear-gradient(107deg, #e2164d 0%, #d11883 100%) 0% 0% no-repeat padding-box',
        boxShadow: '0px 5px 25px #000000',
    };
};
