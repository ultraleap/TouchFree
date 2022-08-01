import { useWindowSize } from 'customHooks';

import 'Styles/Camera/Calibrate.scss';
import cssVariables from 'Styles/_variables.scss';
import 'react-circular-progressbar/dist/styles.css';

import { CreateTypes } from 'canvas-confetti';
import React, { useEffect, useRef, useState } from 'react';
import { CSSProperties } from 'react';
import ReactCanvasConfetti from 'react-canvas-confetti';
import { buildStyles, CircularProgressbar } from 'react-circular-progressbar';
import { CSSTransition, SwitchTransition } from 'react-transition-group';

import DownArrow from 'Images/Down_Arrow.svg';
import HandIcon from 'Images/Tracking_Status_Icon.svg';
import TutorialVideo from 'Videos/Calibration_Tutorial.mp4';

import { TextButton } from 'Components/Controls/TFButton';
import { TFClickEvent } from 'Components/SettingsTypes';

import { TIMEOUT_S } from './CalibrationScreen';

interface CalibrationInstructionsProps {
    progress: number;
    containerStyle: CSSProperties;
    top?: boolean;
}
export const CalibrationInstructions: React.FC<CalibrationInstructionsProps> = ({ progress, containerStyle, top }) => {
    const instructionsText = (
        <h1>
            Hold INDEX FINGER against <br /> this <span className="greenText">GREEN CIRCLE</span>
        </h1>
    );

    const calibratingText = (
        <h1 className="greenText">
            <div style={{ display: 'flex', height: '3.2vh' }}>
                <span>Calibrating</span>
                <span style={{ width: '1vw', textAlign: 'left' }} className="loading" />
            </div>
            <div style={{ paddingTop: '0.5rem' }}>{Math.ceil(progress * 100).toFixed(0)}%</div>
        </h1>
    );

    if (top) {
        return (
            <div className="instructions" style={containerStyle}>
                <img src={DownArrow} alt="Down arrow" className="arrow" style={{ transform: 'rotate(180deg)' }} />
                <SwitchTransition>
                    <CSSTransition
                        key={progress > 0 ? 'calibratingText' : 'instructionText'}
                        addEndListener={(node, done) => node.addEventListener('transitionend', done, false)}
                        classNames="fade"
                    >
                        {progress > 0 ? calibratingText : instructionsText}
                    </CSSTransition>
                </SwitchTransition>
            </div>
        );
    }

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
            <img src={DownArrow} alt="Down arrow" className="arrow" />
        </div>
    );
};

interface CalibrationProgressCircleProps {
    progress: number;
    style: CSSProperties;
}

export const CalibrationProgressCircle: React.FC<CalibrationProgressCircleProps> = ({ progress, style }) => (
    <div style={style} className="touch-circle-progress">
        <CircularProgressbar
            value={Math.ceil(progress * 50) / 50}
            maxValue={1}
            strokeWidth={25}
            styles={buildStyles({
                strokeLinecap: 'butt',
                pathColor: cssVariables.ultraleapGreen,
                trailColor: 'transparent',
                pathTransitionDuration: 0.08,
            })}
        />
    </div>
);

interface HandsLostProps {
    display?: boolean;
    timeToPosSelect?: number;
    handsLostStyle?: CSSProperties;
}

const ReturnToPositionScreenMessage: React.FC<HandsLostProps> = ({ timeToPosSelect }) => {
    if (!timeToPosSelect) {
        return <></>;
    }

    const timeToPosSelectLength = timeToPosSelect.toString().length;
    const timeoutLength = TIMEOUT_S.toString().length;

    // If the time left has fewer characters than the initial time, pad the start with "0"s
    const numString = ''.padEnd(timeoutLength - timeToPosSelectLength, '0') + timeToPosSelect;
    const formattedString = [...numString].map((char, index) => {
        return (
            <span key={index} style={{ width: '1ch', justifyContent: 'center' }}>
                {char}
            </span>
        );
    });

    return (
        <div id="return-message">
            <p>
                Returning in <span style={{ marginLeft: '0.3rem' }}>{formattedString}</span>s
            </p>
        </div>
    );
};

export const CalibrationHandLostMessage: React.FC<HandsLostProps> = ({ display, timeToPosSelect, handsLostStyle }) => {
    if (!display) {
        return <></>;
    }
    return (
        <div className={'hand-not-found-container'} style={handsLostStyle}>
            <img src={HandIcon} alt="Hand Icon" />
            <p>No Hand Detected{timeToPosSelect ? ':' : ''}</p>
            <ReturnToPositionScreenMessage timeToPosSelect={timeToPosSelect} />
        </div>
    );
};

interface CalibrationTutorialVideoProps {
    videoStyle: CSSProperties;
}

export const CalibrationTutorialVideo: React.FC<CalibrationTutorialVideoProps> = ({ videoStyle }) => {
    const [loaded, setLoaded] = useState<boolean>(false);

    const getVideoStyle = (): CSSProperties => {
        return { ...videoStyle, visibility: loaded ? 'visible' : 'hidden' };
    };

    return (
        <video
            className="interaction-guide"
            style={getVideoStyle()}
            autoPlay={true}
            loop={true}
            src={TutorialVideo}
            onLoadedData={() => setLoaded(true)}
        />
    );
};

const cancelSetupButtonStyle: CSSProperties = {
    width: '300px',
    height: '4vh',
    borderRadius: '50px',
    background: cssVariables.lightGreyGradient,
};

const cancelSetupButtonTextStyle: CSSProperties = {
    fontSize: '1.7vh',
    opacity: 1,
};

interface CalibrationCancelButtonProps {
    onCancel: (event: TFClickEvent) => void;
    buttonStyle: CSSProperties;
}

export const CalibrationCancelButton: React.FC<CalibrationCancelButtonProps> = ({ onCancel, buttonStyle }) => {
    return (
        <TextButton
            buttonStyle={{ ...cancelSetupButtonStyle, ...buttonStyle }}
            title=""
            titleStyle={{ display: 'none' }}
            text="Cancel Setup"
            textStyle={cancelSetupButtonTextStyle}
            onClick={onCancel}
            canHover={false}
        />
    );
};

const canvasStyles = (): CSSProperties => {
    return {
        height: '100%',
        width: cssVariables.appWidth,
        position: 'fixed',
        top: 0,
        left: cssVariables.appMarginLeft,
        pointerEvents: 'none',
    };
};

interface CalibrationPracticeButtonProps {
    isHandPresent: boolean;
    progress: number;
}

export const CalibrationPracticeButton: React.FC<CalibrationPracticeButtonProps> = ({ isHandPresent, progress }) => {
    const [hovered, setHovered] = React.useState<boolean>(false);
    const numFired = useRef<number>(0);

    useEffect(() => {
        if (!isHandPresent) {
            setHovered(false);
        }
    }, [isHandPresent]);

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
                className={`setup-practice-button ${hovered ? ' setup-practice-button-hovered' : ''}`}
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
        background: 'linear-gradient(107deg, #e2164d 0%, #d11883 100%)',
        boxShadow: '0px 5px 25px #000000',
    };
};

export const FullScreenPrompt: React.FC<{ promptStyle: CSSProperties }> = ({ promptStyle }) => {
    const { isFullScreen, isZoomed } = useWindowSize();

    if (isZoomed) {
        return (
            <div className="full-screen-prompt" style={promptStyle}>
                <p>Browser is not at 100% zoom. This is not recommended.</p>
            </div>
        );
    }

    if (!isFullScreen) {
        return (
            <div className="full-screen-prompt" style={promptStyle}>
                <p>
                    Full Screen Recommended: <span style={{ fontWeight: 'bold' }}>Press F11</span>
                </p>
            </div>
        );
    }

    return <></>;
};
