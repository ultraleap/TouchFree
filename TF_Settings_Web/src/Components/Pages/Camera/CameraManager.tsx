import 'Styles/Camera/Camera.css';

import React from 'react';
import { Route, Routes } from 'react-router-dom';

import CameraSetupScreen from './CameraSetupScreen';
import { ManualSetupScreen } from './ManualSetupScreem';
import { PositionType } from './QuickSetup/PositionSelectionScreen';
import QuickSetupManager from './QuickSetup/QuickSetupManager';

const CameraManager = () => {
    const [activePosition, setActivePosition] = React.useState<PositionType>(null);

    return (
        <Routes>
            <Route path="" element={<CameraSetupScreen />} />
            <Route
                path="quick/*"
                element={
                    <QuickSetupManager
                        activePosition={activePosition}
                        setPosition={(position: PositionType) => setActivePosition(position)}
                    />
                }
            />
            <Route path="manual" element={<ManualSetupScreen />} />
        </Routes>
    );
};

export default CameraManager;
