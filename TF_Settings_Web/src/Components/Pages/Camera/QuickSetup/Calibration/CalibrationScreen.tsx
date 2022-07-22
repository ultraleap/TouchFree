/* eslint-disable @typescript-eslint/no-empty-function */
import 'Styles/Camera/Calibrate.scss';
import styles from 'Styles/Camera/Calibrate.scss';

import React, { useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import { ConnectionManager } from 'TouchFree/Connection/ConnectionManager';
import { InputActionManager } from 'TouchFree/Plugins/InputActionManager';
import { InputType, InteractionType, TouchFreeInputAction } from 'TouchFree/TouchFreeToolingTypes';

import {
    CalibrationHandLostMessage,
    CalibrationCancelButton,
    CalibrationInstructions,
    CalibrationTutorialVideo,
    CalibrationProgressCircle,
} from './CalibrationComponents';

const { handNotFoundHeight } = styles;

interface CalibrationScreenProps {
    isHandPresent: boolean;
    onCancel: () => void;
}

export const TIMEOUT_S = 10;
const TIMEOUT_MS = TIMEOUT_S * 1000;
const INITIAL_WAIT_MS = 5000;

const CalibrationScreen: React.FC<CalibrationScreenProps> = ({ isHandPresent, onCancel }) => {
    const [displayHandIndicator, setDisplayHandIndicator] = React.useState(false);
    // ===== Click Progress =====
    const [progressToClick, setProgressToClick] = React.useState(0);
    const [progress, setProgress] = React.useState(0);
    const isNewClick = React.useRef(false);
    // ===== Timeout =====
    const [timeToPosSelect, setTimeToPosSelect] = React.useState(TIMEOUT_S);
    const timeout = React.useRef<NodeJS.Timeout>();
    const interval = React.useRef<NodeJS.Timer>();
    const [initialWaitOver, setInitialWaitOver] = React.useState(false);
    // ===== React Router =====
    const navigate = useNavigate();
    const location = useLocation();

    const handleClick = (path: string) => {
        ConnectionManager.serviceConnection()?.QuickSetupRequest(
            path === '../bottom' ? true : false,
            () => {},
            () => {}
        );
        navigate(path);
    };

    const handleTFInput = (evt: CustomEvent<TouchFreeInputAction>): void => {
        if (evt.detail.InteractionType === InteractionType.HOVER) {
            if (!isNewClick.current) {
                isNewClick.current = evt.detail.ProgressToClick === 0;
                return;
            }

            if (evt.detail.InputType === InputType.MOVE || evt.detail.InputType === InputType.DOWN) {
                setProgressToClick(evt.detail.ProgressToClick);
            }
        }
    };

    useEffect(() => {
        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
        };
    }, []);

    useEffect(() => {
        const initialWait = setTimeout(() => {
            setInitialWaitOver(true);
        }, INITIAL_WAIT_MS);

        return () => {
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
            interval.current = setInterval(() => setTimeToPosSelect((t) => t - 1), 1000);
            timeout.current = setTimeout(() => {
                navigate('../../');
            }, TIMEOUT_MS);
            setProgress(0);
        } else {
            setProgress(progressToClick);
            setTimeToPosSelect(TIMEOUT_S);
            clearInterval(interval.current);
            clearTimeout(timeout.current);
        }

        return () => {
            clearTimeout(timeout.current);
            clearInterval(interval.current);
        };
    }, [displayHandIndicator, progressToClick]);

    if (location.pathname.endsWith('top')) {
        return (
            <div onPointerDown={() => handleClick('../bottom')} className="content-container">
                <CalibrationInstructions progress={progress} containerStyle={{ paddingTop: '5vh' }} />
                <CalibrationProgressCircle progress={progress} style={{ top: '15.5vh' }} />
                {displayHandIndicator ? (
                    <CalibrationHandLostMessage timeToPosSelect={timeToPosSelect} />
                ) : (
                    <div style={{ height: handNotFoundHeight }} />
                )}
                <CalibrationTutorialVideo videoStyle={{ paddingTop: '3vh' }} />
                <CalibrationCancelButton onCancel={onCancel} buttonStyle={{ marginTop: '30.5vh' }} />
            </div>
        );
    }

    return (
        <div onPointerDown={() => handleClick('../complete')} className="content-container">
            <CalibrationTutorialVideo videoStyle={{ paddingTop: '30.5vh' }} />
            <CalibrationInstructions progress={progress} containerStyle={{ paddingTop: '2.5vh' }} />
            <CalibrationProgressCircle progress={progress} style={{ bottom: '15.5vh' }} />
            {displayHandIndicator ? (
                <CalibrationHandLostMessage timeToPosSelect={timeToPosSelect} />
            ) : (
                <div style={{ height: handNotFoundHeight }} />
            )}
            <CalibrationCancelButton onCancel={onCancel} buttonStyle={{ marginTop: '5.5vh' }} />
        </div>
    );
};

export default CalibrationScreen;
