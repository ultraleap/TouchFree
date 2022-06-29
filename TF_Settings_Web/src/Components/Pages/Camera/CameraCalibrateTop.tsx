import '../../../Styles/Camera/Calibrate.css';

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import InteractionGuideIcon from '../../../Images/Camera/Interaction_Guide_Top.png';
import { InputActionManager } from '../../../TouchFree/Plugins/InputActionManager';
import { InputType, TouchFreeInputAction } from '../../../TouchFree/TouchFreeToolingTypes';
import { CalibrateCancelButton, CalibrateInstructions, CalibrateProgressCircle } from './CalibrationComponents';
const CameraCalibrateTop = () => {
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
        // Midpoint needs to be 20vh = 384px
        navigate('/camera/quick/calibrate/bottom');
    };

    return (
        <div style={{ height: '100%', alignItems: 'center', marginTop: '-53px' }}>
            <CalibrateInstructions />
            <img
                className="interactionGuide"
                style={{ marginTop: '150px' }}
                src={InteractionGuideIcon}
                alt="Guide demonstrating how to interact with Quick Setup"
                onClick={() => {
                    navigate('/camera/quick/calibrate/bottom');
                }}
            />
            <CalibrateProgressCircle progress={progressToClick} style={{ marginTop: '10%' }} />
            <CalibrateCancelButton buttonStyle={{ marginTop: 'calc(593px - 28%)' }} />
        </div>
    );
};

export default CameraCalibrateTop;
