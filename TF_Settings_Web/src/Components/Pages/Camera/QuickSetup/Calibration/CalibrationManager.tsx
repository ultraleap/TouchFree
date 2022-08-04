/* eslint-disable @typescript-eslint/no-empty-function */
import 'Styles/Camera/Calibrate.scss';

import React, { useEffect } from 'react';
import { Route, Routes, useNavigate } from 'react-router-dom';

import { ConfigurationManager } from 'TouchFree/Configuration/ConfigurationManager';
import { InteractionConfig, PhysicalConfig, Vector } from 'TouchFree/Configuration/ConfigurationTypes';
import { ConnectionManager } from 'TouchFree/Connection/ConnectionManager';
import { ConfigState, HandPresenceState } from 'TouchFree/Connection/TouchFreeServiceTypes';
import { InteractionType } from 'TouchFree/TouchFreeToolingTypes';

import { PositionType } from 'Components/Pages/Camera/QuickSetup/PositionSelectionScreen';
import { TFClickEvent } from 'Components/SettingsTypes';

import CalibrationCompleteScreen from './CalibrationCompleteScreen';
import CalibrationScreen from './CalibrationScreen';

const handEventTypes = ['HandsLost', 'HandFound'];

const calibInteractionConfig: Partial<InteractionConfig> = {
    InteractionType: InteractionType.HOVER,
    DeadzoneRadius: 0.007,
    HoverAndHold: {
        HoverStartTimeS: 1,
        HoverCompleteTimeS: 5,
    },
};

interface CalibrationManagerProps {
    activePosition: PositionType;
}

const CalibrationManager: React.FC<CalibrationManagerProps> = ({ activePosition }) => {
    const [physicalConfig, setPhysicalConfig] = React.useState<PhysicalConfig>();
    const [interactionConfig, setInteractionConfig] = React.useState<InteractionConfig>();
    const [isHandPresent, setIsHandPresent] = React.useState<boolean>(
        ConnectionManager.GetCurrentHandPresence() === HandPresenceState.HAND_FOUND
    );

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
                () => {}
            );
        });

        for (const eventType of handEventTypes) {
            ConnectionManager.instance.addEventListener(eventType, setHandPresence);
        }

        return () => {
            for (const eventType of handEventTypes) {
                ConnectionManager.instance.removeEventListener(eventType, setHandPresence);
            }
        };
    }, []);

    const sendScreenSizeToConfig = () => {
        ConfigurationManager.RequestConfigChange(
            null,
            { ScreenWidthPX: window.innerWidth, ScreenHeightPX: window.innerHeight },
            () => {}
        );
    };

    useEffect(() => {
        sendScreenSizeToConfig();
        window.addEventListener('resize', sendScreenSizeToConfig);

        return () => {
            window.removeEventListener('resize', sendScreenSizeToConfig);
        };
    }, []);

    const setHandPresence = (evt: Event) => {
        setIsHandPresent(evt.type === 'HandFound');
    };

    const setCalibInteractionConfig = (): void =>
        ConfigurationManager.RequestConfigChange(calibInteractionConfig, {}, () => {});

    const resetCalibConfig = (): void =>
        ConfigurationManager.RequestConfigChange(interactionConfig ?? null, physicalConfig ?? null, () => {
            navigate('/settings/camera/quick/');
        });

    const resetCalibInteractionConfig = (): void =>
        ConfigurationManager.RequestConfigChange(interactionConfig ?? null, {}, () => {});

    const onCancel = (event?: TFClickEvent) => {
        event?.stopPropagation();
        setCursorDisplay(true);
        resetCalibConfig();
    };

    return (
        <Routes>
            <Route
                path="top"
                element={<CalibrationScreen key="top" isHandPresent={isHandPresent} onCancel={onCancel} />}
            />
            <Route
                path="bottom"
                element={<CalibrationScreen key="bottom" isHandPresent={isHandPresent} onCancel={onCancel} />}
            />
            <Route
                path="complete"
                element={
                    <CalibrationCompleteScreen
                        isHandPresent={isHandPresent}
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
        </Routes>
    );
};

export default CalibrationManager;

const setCursorDisplay = (show: boolean) => {
    const svgCanvas = document.querySelector('#svg-cursor') as HTMLElement;
    if (!svgCanvas) return;

    // Add an opacity of 0 to hide the cursor and remove this opacity to show the cursor
    svgCanvas.style.opacity = show ? '' : '0';
};

const getRotationFromPosition = (position: PositionType): Vector => {
    if (position === 'FaceScreen') {
        return { X: 20, Y: 0, Z: 180 };
    }
    if (position === 'FaceUser') {
        return { X: -20, Y: 0, Z: 180 };
    }
    // position === 'Below' (Desktop)
    return { X: 0, Y: 0, Z: 0 };
};
