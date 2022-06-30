/* eslint-disable @typescript-eslint/no-empty-function */
import '../../../Styles/Camera/Calibrate.css';

import React, { ReactElement, useEffect } from 'react';
import { NavigateFunction, useNavigate } from 'react-router-dom';

import InteractionBottomGuide from '../../../Images/Camera/Interaction_Guide_Bottom.png';
import InteractionTopGuide from '../../../Images/Camera/Interaction_Guide_Top.png';
import { InputActionManager } from '../../../TouchFree/Plugins/InputActionManager';
import { InputType, TouchFreeInputAction } from '../../../TouchFree/TouchFreeToolingTypes';
import { CalibrateCancelButton, CalibrateInstructions, CalibrateProgressCircle } from './CalibrationComponents';

interface CameraCalibrateScreenProps {
    onCancel: () => void;
}

export const CameraCalibrateTop: React.FC<CameraCalibrateScreenProps> = ({ onCancel }): ReactElement => {
    const navigate = useNavigate();
    const content = (progressToClick: number): ReactElement => (
        <div style={{ height: '100%', alignItems: 'center', marginTop: '-53px' }}>
            <CalibrateInstructions />
            <img
                className="interactionGuide"
                style={{ marginTop: '150px' }}
                src={InteractionTopGuide}
                alt="Guide demonstrating how to interact with Quick Setup"
                onClick={() => navigate('/camera/quick/calibrate/bottom')}
            />
            <CalibrateProgressCircle progress={progressToClick} style={{ marginTop: '10%' }} />
            <CalibrateCancelButton onCancel={onCancel} buttonStyle={{ marginTop: 'calc(593px - 28%)' }} />
        </div>
    );

    // eslint-disable-next-line unicorn/consistent-function-scoping
    const handleClick = (navigate: NavigateFunction) => navigate('/camera/quick/calibrate/bottom');

    return CameraCalibrateScreen(handleClick, content);
};

export const CameraCalibrateBottom: React.FC<CameraCalibrateScreenProps> = ({ onCancel }): ReactElement => {
    const navigate = useNavigate();
    const content = (progressToClick: number): ReactElement => (
        <div style={{ height: '100%', alignItems: 'center' }}>
            <CalibrateProgressCircle progress={progressToClick} style={{ paddingTop: '57px' }} />
            <img
                className="interactionGuide"
                style={{ paddingTop: 'calc(430px - 28%)', marginBottom: '120px' }}
                src={InteractionBottomGuide}
                alt="Guide demonstrating how to interact with Quick Setup"
                onClick={() => navigate('/camera/quick/calibrate/complete')}
            />
            <CalibrateInstructions />
            <CalibrateCancelButton onCancel={onCancel} buttonStyle={{ marginTop: '191px' }} />
        </div>
    );

    // eslint-disable-next-line unicorn/consistent-function-scoping
    const handleClick = () => navigate('/camera/quick/calibrate/complete');

    return CameraCalibrateScreen(handleClick, content);
};

const CameraCalibrateScreen = (
    handleClick: (navigate: NavigateFunction) => void,
    content: (progressToClick: number) => ReactElement
): ReactElement => {
    const [progressToClick, setProgressToClick] = React.useState<number>(0);
    const navigate = useNavigate();

    useEffect(() => {
        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
        };
    }, []);

    const handleTFInput = (evt: CustomEvent<TouchFreeInputAction>): void => {
        const roundedProg = Math.floor(evt.detail.ProgressToClick * 10) / 10;
        setProgressToClick(roundedProg);
        if (roundedProg >= 1 && evt.detail.InputType === InputType.DOWN) {
            handleClick(navigate);
        }
    };

    return content(progressToClick);
};
