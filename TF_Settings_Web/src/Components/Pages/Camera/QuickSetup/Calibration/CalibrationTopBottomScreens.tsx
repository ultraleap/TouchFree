/* eslint-disable @typescript-eslint/no-empty-function */
import 'Styles/Camera/Calibrate.scss';
import styles from 'Styles/Camera/Calibrate.scss';

import React, { ReactElement, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

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

/**
 * CalibrationTop and CalibrationBottom screens use CalibrationBaseScreen to handle logic and just pass in their content
 */

interface CalibrationBaseScreenProps {
    isHandPresent: boolean;
    onCancel: () => void;
}

type Content = (progressToClick: number, timeToPosSelect: number) => ReactElement;

export const CalibrationTopScreen: React.FC<CalibrationBaseScreenProps> = ({
    isHandPresent,
    onCancel,
}): ReactElement => {
    const navigate = useNavigate();

    const handleClick = () => {
        ConnectionManager.serviceConnection()?.QuickSetupRequest(
            true,
            () => {},
            () => {}
        );
        navigate('../bottom');
    };

    const content: Content = (progressToClick, timeToPosSelect) => {
        const prog = isHandPresent ? progressToClick : 0;
        return (
            <div onPointerDown={handleClick} className="content-container">
                <CalibrationInstructions progress={prog} containerStyle={{ paddingTop: '5vh' }} />
                <CalibrationProgressCircle progress={prog} style={{ top: '15.5vh' }} />
                {!isHandPresent ? (
                    <CalibrationHandLostMessage timeToPosSelect={timeToPosSelect} />
                ) : (
                    <div style={{ height: handNotFoundHeight }} />
                )}
                <CalibrationTutorialVideo videoStyle={{ paddingTop: '3vh' }} />
                <CalibrationCancelButton onCancel={onCancel} buttonStyle={{ marginTop: '30.5vh' }} />
            </div>
        );
    };

    return CalibrationBaseScreen(content, isHandPresent);
};

export const CalibrationBottomScreen: React.FC<CalibrationBaseScreenProps> = ({
    isHandPresent,
    onCancel,
}): ReactElement => {
    const navigate = useNavigate();

    const handleClick = () => {
        ConnectionManager.serviceConnection()?.QuickSetupRequest(
            false,
            () => {},
            () => {}
        );
        navigate('../complete');
    };

    const content: Content = (progressToClick, timeToPosSelect) => {
        const prog = isHandPresent ? progressToClick : 0;
        return (
            <div onPointerDown={handleClick} className="content-container">
                <CalibrationTutorialVideo videoStyle={{ paddingTop: '30.5vh' }} />
                <CalibrationInstructions progress={prog} containerStyle={{ paddingTop: '2.5vh' }} />
                <CalibrationProgressCircle progress={prog} style={{ bottom: '15.5vh' }} />
                {!isHandPresent ? (
                    <CalibrationHandLostMessage timeToPosSelect={timeToPosSelect} />
                ) : (
                    <div style={{ height: handNotFoundHeight }} />
                )}
                <CalibrationCancelButton onCancel={onCancel} buttonStyle={{ marginTop: '5.5vh' }} />
            </div>
        );
    };

    return CalibrationBaseScreen(content, isHandPresent);
};

const TIMEOUT_S = 10;
const TIMEOUT_MS = TIMEOUT_S * 1000;

const CalibrationBaseScreen = (content: Content, isHandPresent: boolean): ReactElement => {
    const [progressToClick, setProgressToClick] = React.useState<number>(0);
    const isNewClick = React.useRef<boolean>(false);
    const [timeToPosSelect, setTimeToPosSelect] = React.useState(TIMEOUT_S);
    const timeout = React.useRef<NodeJS.Timeout>();
    const interval = React.useRef<NodeJS.Timer>();
    const nav = useNavigate();

    useEffect(() => {
        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
        };
    }, []);

    useEffect(() => {
        if (!isHandPresent) {
            interval.current = setInterval(() => setTimeToPosSelect((t) => t - 1), 1000);
            timeout.current = setTimeout(() => {
                nav('../../');
            }, TIMEOUT_MS);
        } else {
            setTimeToPosSelect(TIMEOUT_S);
            clearInterval(interval.current);
            clearTimeout(timeout.current);
        }

        return () => {
            clearTimeout(timeout.current);
            clearInterval(interval.current);
        };
    }, [isHandPresent]);

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

    return content(progressToClick, timeToPosSelect);
};
