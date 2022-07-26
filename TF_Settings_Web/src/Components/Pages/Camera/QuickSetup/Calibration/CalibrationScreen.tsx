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
import { CancelEvent } from 'Components/SettingsTypes';

const { handNotFoundHeight } = styles;

interface CalibrationScreenProps {
    isHandPresent: boolean;
    onCancel: (event: CancelEvent) => void;
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
    const timeout = React.useRef<number>();
    const interval = React.useRef<number>();
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
        const {detail} = evt;
        if (detail.InteractionType === InteractionType.HOVER) {
            if (!isNewClick.current) {
                isNewClick.current = detail.ProgressToClick === 0;
                return;
            }

            if (detail.InputType === InputType.MOVE || detail.InputType === InputType.DOWN) {
                setProgressToClick(detail.ProgressToClick);
            }

            if (detail.InputType === InputType.DOWN && detail.ProgressToClick === 1) {
                if (location.pathname.endsWith('top')) {
                    handleClick('../bottom');
                    return;
                }

                handleClick('../complete');
            }
        }
    };

    useEffect(() => {
        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);
        const initialWait = setTimeout(() => {
            setInitialWaitOver(true);
        }, INITIAL_WAIT_MS);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
            clearTimeout(initialWait);
        };
    }, [location.pathname]);

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
                navigate('../../');
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

    if (location.pathname.endsWith('top')) {
        return (
            <div className="content-container">
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
        <div className="content-container">
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
