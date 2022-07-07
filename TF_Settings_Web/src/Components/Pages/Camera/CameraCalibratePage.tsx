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

const calibInteractionConfig: Partial<InteractionConfig> = {
    InteractionType: InteractionType.HOVER,
    DeadzoneRadius: 0.007,
    HoverAndHold: {
        HoverStartTimeS: 1,
        HoverCompleteTimeS: 5,
    },
};

interface CameraCalibratePageProps {
    activePosition: PositionType;
}

const CameraCalibratePage: React.FC<CameraCalibratePageProps> = ({ activePosition }) => {
    const [physicalConfig, setPhysicalConfig] = React.useState<PhysicalConfig>();
    const [interactionConfig, setInteractionConfig] = React.useState<InteractionConfig>();
    const [isCalibConfigActive, setIsCalibConfigActive] = React.useState<boolean>(false);

    const navigate = useNavigate();

    useEffect(() => {
        setCursorDisplay(false);
        // Save current config then change it to use config for calibration
        ConfigurationManager.RequestConfigState((config: ConfigState) => {
            setInteractionConfig(config.interaction);
            setPhysicalConfig(config.physical);

            ConfigurationManager.RequestConfigChange(
                calibInteractionConfig,
                { LeapRotationD: getRotationFromPosition(activePosition) },
                () => setIsCalibConfigActive(true)
            );
        });
    }, []);

    const setCalibInteractionConfig = (): void =>
        ConfigurationManager.RequestConfigChange(calibInteractionConfig, {}, () => {});

    const resetCalibConfig = (): void =>
        ConfigurationManager.RequestConfigChange(interactionConfig ?? null, physicalConfig ?? null, () => {
            navigate('/settings/camera/quick/');
        });

    const resetCalibInteractionConfig = (): void =>
        ConfigurationManager.RequestConfigChange(interactionConfig ?? null, {}, () => {});

    return (
        <Routes>
            <Route
                path="top"
                element={
                    <CameraCalibrateTop
                        isConfigSet={isCalibConfigActive}
                        onCancel={() => {
                            setCursorDisplay(true);
                            resetCalibConfig();
                        }}
                    />
                }
            />
            <Route
                path="bottom"
                element={
                    <CameraCalibrateBottom
                        onCancel={() => {
                            setCursorDisplay(true);
                            resetCalibConfig();
                        }}
                    />
                }
            />
            <Route
                path="complete"
                element={
                    <CameraCalibrateComplete
                        onLoad={() => {
                            setCursorDisplay(true);
                            resetCalibInteractionConfig();
                        }}
                        onRedo={() => {
                            setCursorDisplay(false);
                            setCalibInteractionConfig();
                        }}
                    />
                }
            />
            <Route path="*" element={<Navigate to="top" replace />} />
        </Routes>
    );
};

export default CameraCalibratePage;

const setCursorDisplay = (show: boolean) => {
    const svgCanvas = document.querySelector('#svg-cursor') as HTMLElement;
    if (!svgCanvas) return;

    svgCanvas.style.opacity = show ? '1' : '0';
};

// Need better defaults??
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
