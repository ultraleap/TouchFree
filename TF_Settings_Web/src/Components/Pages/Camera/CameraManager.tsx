import { Route, Routes } from 'react-router-dom';

import CameraMaskingScreen from './CameraMaskingScreen';
import CameraSetupScreen from './CameraSetupScreen';
import { ManualSetupScreen } from './ManualSetupScreen';
import QuickSetupManager from './QuickSetup/QuickSetupManager';

const CameraManager = () => {
    return (
        <Routes>
            <Route path="" element={<CameraSetupScreen />} />
            <Route path="quick/*" element={<QuickSetupManager />} />
            <Route path="manual" element={<ManualSetupScreen />} />
            <Route path="masking" element={<CameraMaskingScreen />} />
        </Routes>
    );
};

export default CameraManager;
