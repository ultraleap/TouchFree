import '../../../Styles/Camera/CameraPage.css';

import React, { useEffect } from 'react';
import { Route, Routes } from 'react-router-dom';

import { ConfigurationManager } from '../../../TouchFree/Configuration/ConfigurationManager';
import { ConnectionManager } from '../../../TouchFree/Connection/ConnectionManager';
import { ConfigState } from '../../../TouchFree/Connection/TouchFreeServiceTypes';
import CameraCalibrateTop from './CalibrateTop';
import CameraPositionPage from './CameraPosition';
import CameraSetupSelection from './CameraSetupSelection';
import { ManualSetup } from './ManualSetup';

export type PositionType = 'FaceUser' | 'FaceScreen' | 'Below' | null;

const getPositionFromConfig = (config: ConfigState): PositionType => {
    const leapRotation = config.physical.LeapRotationD;
    if (Math.abs(leapRotation.Z) > 90) {
        if (leapRotation.X <= 0) {
            return 'FaceUser';
        }
        return 'FaceScreen';
    }
    return 'Below';
};

const CameraPage = () => {
    const [activePosition, setActivePosition] = React.useState<PositionType>(null);

    useEffect(() => {
        ConnectionManager.AddConnectionListener(() => {
            ConfigurationManager.RequestConfigFileState((config: ConfigState) => {
                setActivePosition(getPositionFromConfig(config));
            });
        });
    }, []);

    return (
        <>
            <Routes>
                <Route path="" element={<CameraSetupSelection />} />
                <Route path="quick" element={<CameraPositionPage configPosition={activePosition} />} />
                <Route path="quick/:position/calibrateTop" element={<CameraCalibrateTop />} />

                <Route path="manual" element={<ManualSetup />} />
            </Routes>
        </>
    );
};

export default CameraPage;
