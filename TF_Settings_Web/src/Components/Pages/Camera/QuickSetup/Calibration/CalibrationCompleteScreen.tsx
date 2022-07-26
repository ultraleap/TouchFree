import 'Styles/Camera/Calibrate.scss';
import cssVariables from 'Styles/_variables.scss';

import React, { CSSProperties, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';

import { InputActionManager } from 'TouchFree/Plugins/InputActionManager';
import { InputType, TouchFreeInputAction } from 'TouchFree/TouchFreeToolingTypes';

import { TextButton } from 'Components/Controls/TFButton';

import { CalibrationHandLostMessage, CalibrationPracticeButton } from './CalibrationComponents';

const buttonStyle: CSSProperties = {
    width: '60%',
    height: '25%',
    borderRadius: '33px',
    background: cssVariables.lightGreyGradient,
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
    const isNewClick = useRef(false);

    useEffect(() => {
        onLoad();

        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
        };
    }, []);

    const handleTFInput = (evt: CustomEvent<TouchFreeInputAction>): void => {
        const { detail } = evt;
        if (!isNewClick.current) {
            isNewClick.current = detail.InputType === InputType.UP;
            return;
        }
        if (detail.InputType === InputType.MOVE || detail.InputType === InputType.DOWN) {
            setProgressToClick(detail.ProgressToClick);
        }
    };

    const doneClickHandler = () => {
        if (!isNewClick) return;
        navigate('/settings/camera');
    };

    const redoClickHandler = () => {
        if (!isNewClick) return;
        onRedo();
        navigate('/settings/camera/quick/calibrate/top');
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
                    onClick={doneClickHandler}
                />
                <TextButton
                    buttonStyle={buttonStyle}
                    title="Redo Auto Calibration"
                    titleStyle={titleStyle}
                    text={''}
                    textStyle={{ display: 'none' }}
                    onClick={redoClickHandler}
                />
            </div>
        </div>
    );
};

export default CalibrationCompleteScreen;
