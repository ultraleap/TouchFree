import classnames from 'classnames/bind';

import styles from './Calibration.module.scss';

import React, { useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import { ConnectionManager } from 'TouchFree/src/Connection/ConnectionManager';
import { InputActionManager } from 'TouchFree/src/Plugins/InputActionManager';
import { InputType, InteractionType, TouchFreeInputAction } from 'TouchFree/src/TouchFreeToolingTypes';

import { TFClickEvent } from '@/Components';

import {
    CalibrationHandLostMessage,
    CalibrationCancelButton,
    CalibrationInstructions,
    CalibrationTutorialVideo,
    CalibrationProgressCircle,
    FullScreenPrompt,
} from './CalibrationComponents';

const classes = classnames.bind(styles);

interface CalibrationScreenProps {
    isHandPresent: boolean;
    onCancel: (event?: TFClickEvent) => void;
}

export const TIMEOUT_S = 10;
const TIMEOUT_MS = TIMEOUT_S * 1000;
const INITIAL_WAIT_MS = 5000;

const CalibrationScreen: React.FC<CalibrationScreenProps> = ({ isHandPresent, onCancel }) => {
    const [displayHandIndicator, setDisplayHandIndicator] = React.useState(false);
    // ===== React Router =====
    const navigate = useNavigate();
    const location = useLocation();
    const isTop = location.pathname.endsWith('top');
    // ===== Click Progress =====
    const [progressToClick, setProgressToClick] = React.useState(0);
    const [progress, setProgress] = React.useState(0);
    const isNewClick = React.useRef(false);
    // ===== Timeout =====
    const [timeToPosSelect, setTimeToPosSelect] = React.useState(TIMEOUT_S);
    const timeout = React.useRef<number>();
    const interval = React.useRef<number>();
    const [initialWaitOver, setInitialWaitOver] = React.useState(!isTop);

    const handleClick = (path: string) => {
        ConnectionManager.serviceConnection()?.QuickSetupRequest(
            path === '../bottom' ? true : false,
            () => {},
            () => {}
        );
        navigate(path);
    };

    const handleTFInput = (evt: CustomEvent<TouchFreeInputAction>): void => {
        const { detail } = evt;
        if (detail.InteractionType === InteractionType.HOVER) {
            if (!isNewClick.current) {
                isNewClick.current = detail.ProgressToClick === 0;
                return;
            }

            if (detail.InputType === InputType.MOVE || detail.InputType === InputType.DOWN) {
                setProgressToClick(detail.ProgressToClick);
            }

            if (detail.InputType === InputType.DOWN && detail.ProgressToClick === 1) {
                advance();
            }
        }
    };

    const advance = (): void => {
        if (isTop) {
            handleClick('../bottom');
            return;
        }

        handleClick('../complete');
    };

    useEffect(() => {
        let initialWait: number;
        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);
        if (isTop) {
            initialWait = window.setTimeout(() => {
                setInitialWaitOver(true);
            }, INITIAL_WAIT_MS);
        }

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
            clearTimeout(initialWait);
        };
    }, []);

    useEffect(() => {
        if (!isHandPresent && initialWaitOver) {
            setDisplayHandIndicator(true);
        } else {
            setDisplayHandIndicator(false);
        }
    }, [isHandPresent, initialWaitOver]);

    useEffect(() => {
        if (displayHandIndicator) {
            interval.current = window.setInterval(() => setTimeToPosSelect((t) => t - 1), 1000);
            timeout.current = window.setTimeout(() => {
                onCancel();
            }, TIMEOUT_MS);
            setProgress(0);
        } else {
            setProgress(progressToClick);
            setTimeToPosSelect(TIMEOUT_S);
            window.clearInterval(interval.current);
            window.clearTimeout(timeout.current);
        }

        return () => {
            window.clearTimeout(timeout.current);
            window.clearInterval(interval.current);
        };
    }, [displayHandIndicator, progressToClick]);

    const spacebarListener = (event: KeyboardEvent) => {
        if (event.code === 'Space') {
            advance();
        }
    };

    useEffect(() => {
        document.addEventListener('keyup', spacebarListener);

        return () => {
            document.removeEventListener('keyup', spacebarListener);
        };
    }, []);

    if (location.pathname.endsWith('top')) {
        return (
            <div className={classes('content-container')}>
                <FullScreenPrompt promptStyle={{ position: 'fixed', top: '5vh' }} />
                <CalibrationProgressCircle progress={progress} style={{ position: 'fixed', top: '15.5vh' }} />
                <CalibrationInstructions
                    isTop
                    progress={progress}
                    containerStyle={{ position: 'fixed', top: '25vh' }}
                />
                <CalibrationTutorialVideo videoStyle={{ position: 'fixed', top: '40vh' }} />
                <CalibrationHandLostMessage
                    display={displayHandIndicator}
                    timeToPosSelect={timeToPosSelect}
                    handsLostStyle={{ position: 'fixed', bottom: '10vh' }}
                />
                <CalibrationCancelButton onCancel={onCancel} buttonStyle={{ position: 'fixed', bottom: '3vh' }} />
            </div>
        );
    }

    return (
        <div className={classes('content-container')}>
            <CalibrationTutorialVideo videoStyle={{ position: 'fixed', top: '30vh' }} />
            <CalibrationInstructions progress={progress} containerStyle={{ position: 'fixed', bottom: '27vh' }} />
            <CalibrationProgressCircle progress={progress} style={{ position: 'fixed', bottom: '15.5vh' }} />
            <CalibrationHandLostMessage
                display={displayHandIndicator}
                timeToPosSelect={timeToPosSelect}
                handsLostStyle={{ position: 'fixed', bottom: '10vh' }}
            />
            <CalibrationCancelButton onCancel={onCancel} buttonStyle={{ position: 'fixed', bottom: '3vh' }} />
        </div>
    );
};

export default CalibrationScreen;
