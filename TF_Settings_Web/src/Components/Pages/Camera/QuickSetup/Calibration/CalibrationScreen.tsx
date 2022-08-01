/* eslint-disable @typescript-eslint/no-empty-function */
import 'Styles/Camera/Calibrate.scss';

import React, { useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import { ConfigurationManager } from 'TouchFree/Configuration/ConfigurationManager';
import { ConnectionManager } from 'TouchFree/Connection/ConnectionManager';
import { InputActionManager } from 'TouchFree/Plugins/InputActionManager';
import { InputType, InteractionType, TouchFreeInputAction } from 'TouchFree/TouchFreeToolingTypes';

import FingerprintIcon from 'Images/Camera/Fingerprint_Icon.svg';

import { TFClickEvent } from 'Components/SettingsTypes';

import {
    CalibrationHandLostMessage,
    CalibrationCancelButton,
    CalibrationInstructions,
    CalibrationTutorialVideo,
    CalibrationProgressCircle,
    FullScreenPrompt,
} from './CalibrationComponents';

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
                if (isTop) {
                    handleClick('../bottom');
                    return;
                }

                handleClick('../complete');
            }
        }
    };

    const sendScreenSizeToConfig = () => {
        ConfigurationManager.RequestConfigChange(
            null,
            { ScreenWidthPX: window.innerWidth, ScreenHeightPX: window.innerHeight },
            () => {}
        );
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
        if (isTop) {
            sendScreenSizeToConfig();
            window.addEventListener('resize', sendScreenSizeToConfig);
        }

        return () => {
            window.removeEventListener('resize', sendScreenSizeToConfig);
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

    if (location.pathname.endsWith('top')) {
        return (
            <div className="content-container">
                <FullScreenPrompt promptStyle={{ position: 'fixed', top: '5vh' }} />
                <CalibrationProgressCircle progress={progress} style={{ position: 'fixed', top: '15.5vh' }} />
                <img
                    className="touch-circle"
                    style={{ position: 'fixed', top: '17.5vh' }}
                    src={FingerprintIcon}
                    alt="Fingerprint Icon showing where to place finger for Quick Setup"
                />
                <CalibrationInstructions top progress={progress} containerStyle={{ position: 'fixed', top: '25vh' }} />
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
        <div className="content-container">
            <CalibrationTutorialVideo videoStyle={{ position: 'fixed', top: '30vh' }} />
            <CalibrationInstructions progress={progress} containerStyle={{ position: 'fixed', bottom: '27vh' }} />
            <CalibrationProgressCircle progress={progress} style={{ position: 'fixed', bottom: '15.5vh' }} />
            <img
                className="touch-circle"
                style={{ position: 'fixed', bottom: '17.5vh' }}
                src={FingerprintIcon}
                alt="Fingerprint Icon showing where to place finger for Quick Setup"
            />
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
