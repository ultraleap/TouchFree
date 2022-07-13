/* eslint-disable @typescript-eslint/no-empty-function */
import 'Styles/Camera/Calibrate.css';

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

/**
 * CalibrationTop and CalibrationBottom screens use CalibrationBaseScreen to handle logic and just pass in their content
 */

interface CalibrationBaseScreenProps {
    isHandPresent: boolean;
    onCancel: () => void;
}

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

    const content = (progressToClick: number): ReactElement => {
        const prog = isHandPresent ? progressToClick : 0;
        return (
            <div onPointerDown={handleClick} className="contentContainer">
                <CalibrationInstructions progress={prog} containerStyle={{ paddingTop: '5vh' }} />
                <CalibrationProgressCircle progress={prog} style={{ top: '15.5vh' }} />
                {!isHandPresent ? <CalibrationHandLostMessage /> : <div style={{ height: '3vh' }} />}
                <CalibrationTutorialVideo videoStyle={{ paddingTop: '3vh' }} />
                <CalibrationCancelButton onCancel={onCancel} buttonStyle={{ marginTop: '30vh' }} />
            </div>
        );
    };

    return CalibrationBaseScreen(content);
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

    const content = (progressToClick: number): ReactElement => {
        const prog = isHandPresent ? progressToClick : 0;
        return (
            <div onPointerDown={handleClick} className="contentContainer">
                <CalibrationTutorialVideo videoStyle={{ paddingTop: '30.5vh' }} />
                <CalibrationInstructions progress={prog} containerStyle={{ paddingTop: '2.5vh' }} />
                <CalibrationProgressCircle progress={prog} style={{ bottom: '15.5vh' }} />
                {!isHandPresent ? <CalibrationHandLostMessage /> : <div style={{ height: '3vh' }} />}
                <CalibrationCancelButton onCancel={onCancel} buttonStyle={{ marginTop: '5.5vh' }} />
            </div>
        );
    };

    return CalibrationBaseScreen(content);
};

const CalibrationBaseScreen = (content: (progressToClick: number) => ReactElement): ReactElement => {
    const [progressToClick, setProgressToClick] = React.useState<number>(0);
    const isNewClick = React.useRef<boolean>(false);

    useEffect(() => {
        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
        };
    }, []);

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

    return content(progressToClick);
};
