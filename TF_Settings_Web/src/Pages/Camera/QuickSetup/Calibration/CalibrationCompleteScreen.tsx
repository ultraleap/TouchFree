import styles from './Calibration.module.scss';
import cssVariables from '@/variables.module.scss';

import classnames from 'classnames/bind';
import React, { CSSProperties, useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { InputActionManager } from 'touchfree/src/Plugins/InputActionManager';
import { InputType, TouchFreeInputAction } from 'touchfree/src/TouchFreeToolingTypes';

import { TextButton } from '@/Components';

import { CalibrationHandLostMessage, CalibrationPracticeButton } from './CalibrationComponents';

const classes = classnames.bind(styles);

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
        <div className={classes('setup-complete-container')}>
            <div>
                <CalibrationHandLostMessage
                    display={!isHandPresent}
                    handsLostStyle={{ height: '3vh', padding: '5px 30px' }}
                />
            </div>
            <h1 className={classes('setup-complete-title')}>
                Setup <br />
                Complete
            </h1>
            <div
                className={classes('setup-complete-options-container')}
                style={{ pointerEvents: showButtons ? 'all' : 'none' }}
            >
                <TextButton
                    buttonStyle={buttonStyle}
                    title="Redo Auto Calibration"
                    titleStyle={titleStyle}
                    onClick={redoClickHandler}
                />
                <TextButton buttonStyle={buttonStyle} title="Done" titleStyle={titleStyle} onClick={doneClickHandler} />
                <CalibrationPracticeButton isHandPresent={isHandPresent} progress={progressToClick} />
            </div>
        </div>
    );
};

export default CalibrationCompleteScreen;
