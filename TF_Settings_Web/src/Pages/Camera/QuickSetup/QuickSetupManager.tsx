import '@/Pages/Camera/Camera.scss';

import React from 'react';
import { Route, Routes } from 'react-router-dom';

import { PositionSelectionScreen, CalibrationManager } from '@/Pages';

import { PositionType } from './PositionSelectionScreen';

const QuickSetupManager = () => {
    const [activePosition, setActivePosition] = React.useState<PositionType>(null);

    return (
        <Routes>
            <Route
                path=""
                element={<PositionSelectionScreen activePosition={activePosition} setPosition={setActivePosition} />}
            />
            <Route path="calibrate/*" element={<CalibrationManager activePosition={activePosition} />} />
        </Routes>
    );
};

export default QuickSetupManager;
