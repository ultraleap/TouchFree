import 'Styles/Camera/Calibrate.scss';

import React, { CSSProperties, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import { InputActionManager } from 'TouchFree/Plugins/InputActionManager';
import { InputType, TouchFreeInputAction } from 'TouchFree/TouchFreeToolingTypes';

import { TextButton } from 'Components/Controls/TFButton';

import { CalibrationHandLostMessage, CalibrationPracticeButton } from './CalibrationComponents';

const buttonStyle: CSSProperties = {
    width: '60%',
    height: '25%',
    borderRadius: '33px',
    background: 'transparent linear-gradient(180deg, #5c5c5c 0%, #454545 100%) 0% 0% no-repeat padding-box',
};

const titleStyle: CSSProperties = {
    fontSize: '2rem',
    padding: '0',
    margin: '0',
    display: 'inline-flex',
    justifyContent: 'center',
    alignItems: 'center',
};

interface CalibrationCompleteProps {
    isHandPresent: boolean;
    onLoad: () => void;
    onRedo: () => void;
}

const CalibrationCompleteScreen: React.FC<CalibrationCompleteProps> = ({ onLoad, onRedo, isHandPresent }) => {
    const [progressToClick, setProgressToClick] = React.useState<number>(0);

    useEffect(() => {
        onLoad();

        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
        };
    }, []);

    const handleTFInput = (evt: CustomEvent<TouchFreeInputAction>): void => {
        if (evt.detail.InputType === InputType.MOVE || evt.detail.InputType === InputType.DOWN) {
            setProgressToClick(evt.detail.ProgressToClick);
        }
    };

    const navigate = useNavigate();
    return (
        <div style={{ height: '100%', alignItems: 'center' }}>
            {!isHandPresent ? <CalibrationHandLostMessage /> : <div style={{ height: '3vh' }} />}
            <div style={{ paddingTop: '200px' }}>
                <h1 className="setup-complete-title">
                    Setup <br />
                    Complete
                </h1>
            </div>
            <div className="setup-complete-options-container">
                <CalibrationPracticeButton isHandPresent={isHandPresent} progress={progressToClick} />
                <TextButton
                    buttonStyle={buttonStyle}
                    title="Done"
                    titleStyle={titleStyle}
                    text={''}
                    textStyle={{ display: 'none' }}
                    onClick={() => navigate('/settings/camera')}
                />
                <TextButton
                    buttonStyle={buttonStyle}
                    title="Redo Auto Calibration"
                    titleStyle={titleStyle}
                    text={''}
                    textStyle={{ display: 'none' }}
                    onClick={() => {
                        onRedo();
                        navigate('/settings/camera/quick/calibrate/top');
                    }}
                />
            </div>
        </div>
    );
};

export default CalibrationCompleteScreen;
