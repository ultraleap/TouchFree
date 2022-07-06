/* eslint-disable @typescript-eslint/no-empty-function */
import '../../../Styles/Camera/Calibrate.css';

import React, { ReactElement, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import InteractionBottomGuide from '../../../Images/Camera/Interaction_Guide_Bottom.png';
import InteractionTopGuide from '../../../Images/Camera/Interaction_Guide_Top.png';
import { ConnectionManager } from '../../../TouchFree/Connection/ConnectionManager';
import { InputActionManager } from '../../../TouchFree/Plugins/InputActionManager';
import { InputType, TouchFreeInputAction } from '../../../TouchFree/TouchFreeToolingTypes';
import {
    CalibrateCancelButton,
    CalibrateHandLostMessage,
    CalibrateInstructions,
    CalibrateProgressCircle,
} from './CalibrationComponents';

interface CameraCalibrateScreenProps {
    onCancel: () => void;
    isHandPresent: boolean;
}

export const CameraCalibrateTop: React.FC<CameraCalibrateScreenProps & { isConfigSet: boolean }> = ({
    onCancel,
    isConfigSet,
    isHandPresent,
}): ReactElement => {
    const navigate = useNavigate();
    const content = (progressToClick: number): ReactElement => (
        <div className="contentContainer">
            <CalibrateInstructions progress={progressToClick} containerStyle={{ paddingTop: '100px' }} />
            <CalibrateProgressCircle progress={progressToClick} style={{ top: '294px' }} />
            {!isHandPresent ? <CalibrateHandLostMessage /> : <div style={{ height: '50px' }} />}
            <img
                className="interactionGuide"
                style={{ paddingTop: '50px' }}
                src={InteractionTopGuide}
                alt="Guide demonstrating how to interact with Quick Setup"
                onDoubleClick={() => navigate('../bottom')}
            />
            <CalibrateCancelButton onCancel={onCancel} buttonStyle={{ marginTop: '575px' }} />
        </div>
    );

    const handleClick = () => {
        ConnectionManager.serviceConnection()?.QuickSetupRequest(
            true,
            () => {},
            () => {}
        );
        navigate('../bottom');
    };

    return CameraCalibrateScreen(handleClick, content, isConfigSet);
};

export const CameraCalibrateBottom: React.FC<CameraCalibrateScreenProps> = ({
    onCancel,
    isHandPresent,
}): ReactElement => {
    const navigate = useNavigate();
    const content = (progressToClick: number): ReactElement => (
        <div className="contentContainer">
            <img
                className="interactionGuide"
                style={{ paddingTop: '600px' }}
                src={InteractionBottomGuide}
                alt="Guide demonstrating how to interact with Quick Setup"
                onDoubleClick={() => navigate('../complete')}
            />
            <CalibrateInstructions progress={progressToClick} containerStyle={{ paddingTop: '50px' }} />
            <CalibrateProgressCircle progress={progressToClick} style={{ top: '1446px' }} />
            {!isHandPresent ? <CalibrateHandLostMessage /> : <div style={{ height: '50px' }} />}
            <CalibrateCancelButton onCancel={onCancel} buttonStyle={{ marginTop: '75px' }} />
        </div>
    );

    const handleClick = () => {
        ConnectionManager.serviceConnection()?.QuickSetupRequest(
            false,
            () => {},
            () => {}
        );
        navigate('../complete');
    };

    return CameraCalibrateScreen(handleClick, content, true);
};

const CameraCalibrateScreen = (
    handleClick: () => void,
    content: (progressToClick: number) => ReactElement,
    isConfigSet: boolean
): ReactElement => {
    const [progressToClick, setProgressToClick] = React.useState<number>(0);
    const isNewClick = React.useRef<boolean>(false);
    const readyToInteract = React.useRef<boolean>(false);

    readyToInteract.current = isConfigSet;

    useEffect(() => {
        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
        };
    }, []);

    const handleTFInput = (evt: CustomEvent<TouchFreeInputAction>): void => {
        if (readyToInteract.current === false) {
            return;
        }
        if (!isNewClick.current) {
            isNewClick.current = evt.detail.ProgressToClick === 0;
            return;
        }
        if (evt.detail.InputType === InputType.MOVE || evt.detail.InputType === InputType.DOWN) {
            const roundedProg = Math.floor(evt.detail.ProgressToClick * 100) / 100;
            setProgressToClick(roundedProg);
            if (roundedProg >= 1) {
                handleClick();
            }
        }
    };

    return content(progressToClick);
};
