import 'Styles/Camera/Camera.css';

import React from 'react';
import { Route, Routes } from 'react-router-dom';

import CalibrationManager from './Calibration/CalibrationManager';
import PositionSelectionScreen, { PositionType } from './PositionSelectionScreen';

interface QuickSetupProps {
    activePosition: PositionType;
    setPosition: (position: PositionType) => void;
}

const QuickSetupManager: React.FC<QuickSetupProps> = ({ activePosition, setPosition }) => (
    <Routes>
        <Route
            path=""
            element={<PositionSelectionScreen activePosition={activePosition} setPosition={setPosition} />}
        />
        <Route path="calibrate/*" element={<CalibrationManager activePosition={activePosition} />} />
    </Routes>
);

export default QuickSetupManager;
