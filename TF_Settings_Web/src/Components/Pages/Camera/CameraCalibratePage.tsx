/* eslint-disable @typescript-eslint/no-empty-function */
import '../../../Styles/Camera/Calibrate.css';

import React, { useEffect } from 'react';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';

import { ConfigurationManager } from '../../../TouchFree/Configuration/ConfigurationManager';
import { InteractionConfig, PhysicalConfig, Vector } from '../../../TouchFree/Configuration/ConfigurationTypes';
import { ConfigState } from '../../../TouchFree/Connection/TouchFreeServiceTypes';
import { InteractionType } from '../../../TouchFree/TouchFreeToolingTypes';
import CameraCalibrateComplete from './CameraCalibrateComplete';
import { CameraCalibrateBottom, CameraCalibrateTop } from './CameraCalibrateScreens';
import { PositionType } from './CameraPosition';

const calibrationInteractionConfig: Partial<InteractionConfig> = {
    InteractionType: InteractionType.HOVER,
    DeadzoneRadius: 0.007,
    HoverAndHold: {
        HoverStartTimeS: 1,
        HoverCompleteTimeS: 2,
    },
};

interface CameraCalibratePageProps {
    configPosition: PositionType;
}

const CameraCalibratePage: React.FC<CameraCalibratePageProps> = ({ configPosition }) => {
    const [physicalConfig, setPhysicalConfig] = React.useState<PhysicalConfig>();
    const [interactionConfig, setinteractionConfig] = React.useState<InteractionConfig>();

    const navigate = useNavigate();

    useEffect(() => {
        // Save current config then change it to use config for calibration
        ConfigurationManager.RequestConfigState((config: ConfigState) => {
            setinteractionConfig(config.interaction);
            setPhysicalConfig(config.physical);
            ConfigurationManager.RequestConfigChange(
                calibrationInteractionConfig,
                { LeapRotationD: getRotationFromPosition(configPosition) },
                () => {}
            );
        });
    }, []);

    const setCalibrationInteractionConfig = () => {
        ConfigurationManager.RequestConfigChange(calibrationInteractionConfig, {}, () => {});
    };

    const resetConfig = () => {
        ConfigurationManager.RequestConfigChange(interactionConfig ?? null, physicalConfig ?? null, () => {
            navigate('/camera/quick/');
        });
    };

    const resetInteractionConfig = () => {
        ConfigurationManager.RequestConfigChange(interactionConfig ?? null, {}, () => {});
    };

    return (
        <Routes>
            <Route path="top" element={<CameraCalibrateTop onCancel={resetConfig} />} />
            <Route path="bottom" element={<CameraCalibrateBottom onCancel={resetConfig} />} />
            <Route
                path="complete"
                element={
                    <CameraCalibrateComplete onLoad={resetInteractionConfig} onRedo={setCalibrationInteractionConfig} />
                }
            />
            <Route path="*" element={<Navigate to="top" replace />} />
        </Routes>
    );
};

export default CameraCalibratePage;

const getRotationFromPosition = (position: PositionType): Vector => {
    if (position === 'FaceScreen') {
        return { X: 10, Y: 0, Z: 95 };
    }
    if (position === 'FaceUser') {
        return { X: 0, Y: 0, Z: 95 };
    }
    // position === 'FaceUser'
    return { X: 0, Y: 0, Z: 0 };
};
