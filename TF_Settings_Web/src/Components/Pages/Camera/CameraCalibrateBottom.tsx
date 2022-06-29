import '../../../Styles/Camera/Calibrate.css';

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import InteractionGuideIcon from '../../../Images/Camera/Interaction_Guide_Bottom.png';
import { InputActionManager } from '../../../TouchFree/Plugins/InputActionManager';
import { InputType, TouchFreeInputAction } from '../../../TouchFree/TouchFreeToolingTypes';
import { CalibrateCancelButton, CalibrateInstructions, CalibrateProgressCircle } from './CalibrationComponents';

const CameraCalibrateBottom = () => {
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
            handleClick();
        }
    };

    const handleClick = (): void => {
        // Send message to tracking service
        // Midpoint needs to be 80vh = 1536px / 1506 = -51
        navigate('/camera/quick/calibrate/complete');
    };

    return (
        <div style={{ height: '100%', alignItems: 'center' }}>
            <CalibrateProgressCircle progress={progressToClick} style={{ paddingTop: '57px' }} />
            <img
                className="interactionGuide"
                style={{ paddingTop: 'calc(430px - 28%)', marginBottom: '120px' }}
                src={InteractionGuideIcon}
                alt="Guide demonstrating how to interact with Quick Setup"
                onClick={() => {
                    navigate('/camera/quick/calibrate/complete');
                }}
            />
            <CalibrateInstructions />
            <CalibrateCancelButton buttonStyle={{ marginTop: '191px' }} />
        </div>
    );
};

export default CameraCalibrateBottom;
