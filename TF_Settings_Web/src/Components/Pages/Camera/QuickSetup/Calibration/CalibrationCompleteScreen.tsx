import 'Styles/Camera/Calibrate.scss';
import cssVariables from 'Styles/variables.module.scss';

import React, { CSSProperties, useEffect, useRef, useState } from 'react';
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
    const [progressToClick, setProgressToClick] = useState<number>(0);
    const [showButtons, setShowButtons] = useState<boolean>(false);
    const isNewClick = useRef(false);

    useEffect(() => {
        onLoad();

        InputActionManager._instance.addEventListener('TransmitInputAction', handleTFInput as EventListener);

        const timerID = window.setTimeout(() => setShowButtons(true), 1000);

        return () => {
            InputActionManager._instance.removeEventListener('TransmitInputAction', handleTFInput as EventListener);
            window.clearTimeout(timerID);
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
            <CalibrationHandLostMessage
                display={!isHandPresent}
                handsLostStyle={{ height: '3vh', padding: '5px 30px' }}
            />
            <div style={{ paddingTop: '100px' }}>
                <h1 className="setup-complete-title">
                    Setup <br />
                    Complete
                </h1>
            </div>
            <div className="setup-complete-options-container" style={{ pointerEvents: showButtons ? 'all' : 'none' }}>
                <TextButton
                    buttonStyle={buttonStyle}
                    title="Redo Auto Calibration"
                    titleStyle={titleStyle}
                    text={''}
                    textStyle={{ display: 'none' }}
                    onClick={redoClickHandler}
                />
                <TextButton
                    buttonStyle={buttonStyle}
                    title="Done"
                    titleStyle={titleStyle}
                    text={''}
                    textStyle={{ display: 'none' }}
                    onClick={doneClickHandler}
                />
                <CalibrationPracticeButton isHandPresent={isHandPresent} progress={progressToClick} />
            </div>
        </div>
    );
};

export default CalibrationCompleteScreen;
