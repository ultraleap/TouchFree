/* eslint-disable @typescript-eslint/no-empty-function */
import '../../../Styles/Camera/Calibrate.css';

import React, { ReactElement, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

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
                onClick={() => navigate('../bottom')}
            />
            <CalibrateProgressCircle progress={progressToClick} style={{ marginTop: '10%' }} />
            <CalibrateCancelButton onCancel={onCancel} buttonStyle={{ marginTop: 'calc(593px - 28%)' }} />
        </div>
    );

    const handleClick = () => {
        //SEND TO SERVICE
        navigate('../bottom');
    };

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
                onClick={() => navigate('../complete')}
            />
            <CalibrateInstructions />
            <CalibrateCancelButton onCancel={onCancel} buttonStyle={{ marginTop: '191px' }} />
        </div>
    );

    const handleClick = () => {
        //SEND TO SERVICE
        navigate('../complete');
    };

    return CameraCalibrateScreen(handleClick, content);
};

const CameraCalibrateScreen = (
    handleClick: () => void,
    content: (progressToClick: number) => ReactElement
): ReactElement => {
    const [progressToClick, setProgressToClick] = React.useState<number>(0);
    const isNewClick = React.useRef<boolean>(false);

    useEffect(() => {
        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
        };
    }, []);

    const handleTFInput = (evt: CustomEvent<TouchFreeInputAction>): void => {
        if (!isNewClick.current) {
            isNewClick.current = evt.detail.ProgressToClick === 0;
            return;
        }
        if (evt.detail.InputType === InputType.MOVE || evt.detail.InputType === InputType.DOWN) {
            const roundedProg = Math.floor(evt.detail.ProgressToClick * 10) / 10;
            setProgressToClick(roundedProg);
            if (roundedProg >= 1) {
                handleClick();
            }
        }
    };

    return content(progressToClick);
};
