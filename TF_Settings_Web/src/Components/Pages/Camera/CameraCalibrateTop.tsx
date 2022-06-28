import '../../../Styles/Camera/Calibrate.css';

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import InteractionGuideIcon from '../../../Images/Camera/Interaction_Guide_Top.png';
import { InputActionManager } from '../../../TouchFree/Plugins/InputActionManager';
import { InputType, TouchFreeInputAction } from '../../../TouchFree/TouchFreeToolingTypes';
import { CalibrateCancelButton, CalibrateInstructions } from './CalibrationComponents';

const CameraCalibrateTop = () => {
    const [progressToClick, setProgressToClick] = React.useState(0);
    const navigate = useNavigate();

    useEffect(() => {
        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
        };
    }, []);

    const handleTFInput = (evt: CustomEvent<TouchFreeInputAction>): void => {
        const detail = evt.detail;
        const roundedProg = Math.floor(detail.ProgressToClick * 10) / 10;
        setProgressToClick(roundedProg);
        if (roundedProg >= 1) {
            handleClick();
        }
    };

    const handleClick = (): void => {
        // Send message to tracking service
        // Top point = (130 + 145 + 60 + 20) = [355,445] -> midpoint = 400
        navigate('/camera/quick/calibrate/bottom');
    };

    return (
        <div style={{ height: '100%', alignItems: 'center', marginTop: '-40px' }}>
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
            <h1 style={{ color: 'white' }}>{progressToClick}</h1>
            <CalibrateCancelButton buttonStyle={{ marginTop: '580px' }} />
        </div>
    );
};

export default CameraCalibrateTop;
