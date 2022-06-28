/* eslint-disable @typescript-eslint/no-empty-function */
import '../../../Styles/Camera/Calibrate.css';

import React, { useEffect } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import { ConfigurationManager } from '../../../TouchFree/Configuration/ConfigurationManager';
import { InteractionConfig } from '../../../TouchFree/Configuration/ConfigurationTypes';
import { ConfigState } from '../../../TouchFree/Connection/TouchFreeServiceTypes';
import { InteractionType } from '../../../TouchFree/TouchFreeToolingTypes';
import CameraCalibrateBottom from './CameraCalibrateBottom';
import CameraCalibrateComplete from './CameraCalibrateComplete';
import CameraCalibrateTop from './CameraCalibrateTop';
import { PositionType } from './CameraPosition';

interface CameraCalibratePageProps {
    configPosition: PositionType;
}

const CameraCalibratePage: React.FC<CameraCalibratePageProps> = ({ configPosition }) => {
    useEffect(() => {
        let interactionConfig: InteractionConfig | null = null;

        // Save current config then change it to use config for calibration
        ConfigurationManager.RequestConfigState((config: ConfigState) => {
            interactionConfig = config.interaction;
            ConfigurationManager.RequestConfigChange(
                {
                    InteractionType: InteractionType.HOVER,
                    DeadzoneRadius: 0.0055,
                    HoverAndHold: {
                        HoverStartTimeS: 1,
                        HoverCompleteTimeS: 5,
                    },
                },
                null,
                () => {}
            );
        });

        return () => ConfigurationManager.RequestConfigChange(interactionConfig, null, () => {});
    }, []);

    return (
        <>
            <Routes>
                <Route path="top" element={<CameraCalibrateTop />} />
                <Route path="bottom" element={<CameraCalibrateBottom />} />
                <Route path="complete" element={<CameraCalibrateComplete />} />
                <Route path="*" element={<Navigate to="top" replace />} />
            </Routes>
        </>
    );
};

export default CameraCalibratePage;
