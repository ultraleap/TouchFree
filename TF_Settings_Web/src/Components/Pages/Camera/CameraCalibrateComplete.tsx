import '../../../Styles/Camera/Calibrate.css';

import React, { CSSProperties, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import { InputActionManager } from '../../../TouchFree/Plugins/InputActionManager';
import { InputType, TouchFreeInputAction } from '../../../TouchFree/TouchFreeToolingTypes';
import IconTextButton from '../../Controls/IconTextButton';
import { CalibratePracticeButton } from './CalibrationComponents';

const buttonStyle: CSSProperties = {
    width: '50%',
    height: '30%',
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

interface CameraCalibrateCompleteProps {
    onLoad: () => void;
    onRedo: () => void;
}

const CameraCalibrateComplete: React.FC<CameraCalibrateCompleteProps> = ({ onLoad, onRedo }) => {
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
            <div style={{ paddingTop: '25%' }}>
                <h1 className="setupCompleteText">
                    Setup <br />
                    Complete
                </h1>
            </div>
            <div style={{ height: '12%', paddingTop: '12%' }}>
                <CalibratePracticeButton progress={progressToClick} />
            </div>
            <div className="setupCompleteOptionsContainer">
                <IconTextButton
                    buttonStyle={buttonStyle}
                    icon={''}
                    alt=""
                    title="Done"
                    titleStyle={titleStyle}
                    text={''}
                    textStyle={{ display: 'none' }}
                    onClick={() => {
                        navigate('/camera');
                    }}
                />
                <IconTextButton
                    buttonStyle={buttonStyle}
                    icon={''}
                    alt=""
                    title="Redo Auto Calibration"
                    titleStyle={titleStyle}
                    text={''}
                    textStyle={{ display: 'none' }}
                    onClick={() => {
                        onRedo();
                        navigate('/camera/quick/calibrate/top');
                    }}
                />
            </div>
        </div>
    );
};

export default CameraCalibrateComplete;
