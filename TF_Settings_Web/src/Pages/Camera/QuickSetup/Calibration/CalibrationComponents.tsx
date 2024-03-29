import styles from './Calibration.module.scss';
import cssVariables from '@/variables.module.scss';
import 'react-circular-progressbar/dist/styles.css';

import { CreateTypes } from 'canvas-confetti';
import classnames from 'classnames/bind';
import React, { useEffect, useRef, useState } from 'react';
import { CSSProperties } from 'react';
import ReactCanvasConfetti from 'react-canvas-confetti';
import { buildStyles, CircularProgressbar } from 'react-circular-progressbar';

import { useIsFullScreen } from '@/customHooks';

import { DownArrow, FingerprintIcon, HandIcon } from '@/Images';
import { CalibrationTutorial } from '@/Videos';

import { TextButton, TFClickEvent } from '@/Components';

import { TIMEOUT_S } from './CalibrationScreen';

const classes = classnames.bind(styles);

interface CalibrationInstructionsProps {
    progress: number;
    containerStyle: CSSProperties;
    isTop?: boolean;
}
export const CalibrationInstructions: React.FC<CalibrationInstructionsProps> = ({
    progress,
    containerStyle,
    isTop,
}) => {
    const instructionsText = (
        <h1 className={classes('instruction-text')}>
            Hold INDEX FINGER against <br /> this <span className={classes('green-text')}>GREEN CIRCLE</span>
        </h1>
    );

    const calibratingText = (
        <h1 className={classes('green-text')}>
            <div className={classes('calibrating-text')}>
                <span>Calibrating</span>
                <span style={{ width: '1vw', textAlign: 'left' }} className={classes('loading')} />
            </div>
            <div style={{ paddingTop: '0.5rem' }}>{Math.ceil(progress * 100).toFixed(0)}%</div>
        </h1>
    );

    if (isTop) {
        return (
            <div className={classes('instructions')} style={containerStyle}>
                <img
                    src={DownArrow}
                    alt="Down arrow"
                    className={classes('arrow')}
                    style={{ transform: 'rotate(180deg)' }}
                />
                {progress > 0 ? calibratingText : instructionsText}
            </div>
        );
    }

    return (
        <div className={classes('instructions')} style={containerStyle}>
            {progress > 0 ? calibratingText : instructionsText}
            <img src={DownArrow} alt="Down arrow" className={classes('arrow')} />
        </div>
    );
};

interface CalibrationProgressCircleProps {
    progress: number;
    style: CSSProperties;
}

export const CalibrationProgressCircle: React.FC<CalibrationProgressCircleProps> = ({ progress, style }) => (
    <div style={style} className={classes('touch-circle-progress')}>
        <CircularProgressbar
            value={Math.ceil(progress * 50) / 50}
            maxValue={1}
            strokeWidth={25}
            styles={buildStyles({
                strokeLinecap: 'butt',
                pathColor: cssVariables.ultraleapGreen,
                trailColor: 'transparent',
                pathTransition: progress === 0 ? 'none' : 'stroke-dashoffset 0.1s ease 0s',
            })}
        />
        <img
            className={classes('touch-circle')}
            style={{ position: 'absolute', top: '22%', left: '22%' }}
            src={FingerprintIcon}
            alt="Fingerprint Icon showing where to place finger for Quick Setup"
        />
    </div>
);

interface HandsLostProps {
    display: boolean;
    timeToPosSelect?: number;
    handsLostStyle?: CSSProperties;
}

const ReturnToPositionScreenMessage: React.FC<{ timeToPosSelect?: number }> = ({ timeToPosSelect }) => {
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
        return <div style={handsLostStyle}></div>;
    }
    return (
        <div className={classes('hand-not-found-container')} style={handsLostStyle}>
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
            className={classes('interaction-guide')}
            style={getVideoStyle()}
            autoPlay={true}
            loop={true}
            src={CalibrationTutorial}
            onLoadedData={() => setLoaded(true)}
        />
    );
};

const cancelSetupButtonStyle: CSSProperties = {
    width: 'auto',
    height: '4vh',
    borderRadius: '50px',
    padding: '0 30px',
    background: cssVariables.lightGreyGradient,
};

interface CalibrationCancelButtonProps {
    onCancel: (event: TFClickEvent) => void;
    buttonStyle: CSSProperties;
}

export const CalibrationCancelButton: React.FC<CalibrationCancelButtonProps> = ({ onCancel, buttonStyle }) => {
    return (
        <TextButton
            buttonStyle={{ ...cancelSetupButtonStyle, ...buttonStyle }}
            title="Cancel Setup"
            titleStyle={{ margin: 0, padding: 0 }}
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
                origin: { y: 0.7 },
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
                className={classes('setup-practice-button', { 'setup-practice-button-hovered': hovered })}
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
    if (progress === 1) {
        return {
            background: 'linear-gradient(107deg, #e2164d 0%, #d11883 100%)',
        };
    }
    return {
        boxShadow: '0px 0px 30px v.$ultraleap-green',
    };
};

export const FullScreenPrompt: React.FC<{ promptStyle: CSSProperties }> = ({ promptStyle }) => {
    const isFullScreen = useIsFullScreen();
    const [isReady, setIsReady] = useState(false);

    useEffect(() => {
        const timeout = setTimeout(() => setIsReady(true), 200);

        return () => clearTimeout(timeout);
    }, []);

    if (!isReady) {
        return <></>;
    }

    if (!isFullScreen) {
        return (
            <div className={classes('full-screen-prompt')} style={promptStyle}>
                <span>
                    <p>Full screen and 100% zoom recommended: </p>
                    <p style={{ fontWeight: 'bold' }}>Please reset zoom and/or enter full screen</p>
                </span>
            </div>
        );
    }

    return <></>;
};
