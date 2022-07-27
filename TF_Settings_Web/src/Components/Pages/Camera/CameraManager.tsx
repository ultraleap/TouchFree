import { Route, Routes } from 'react-router-dom';

import CameraSetupScreen from './CameraSetupScreen';
import { ManualSetupScreen } from './ManualSetupScreen';
import MaskingScreen from './Masking/MaskingScreen';
import QuickSetupManager from './QuickSetup/QuickSetupManager';

const CameraManager = () => {
    return (
        <Routes>
            <Route path="" element={<CameraSetupScreen />} />
            <Route path="quick/*" element={<QuickSetupManager />} />
            <Route path="manual" element={<ManualSetupScreen />} />
            <Route path="masking" element={<MaskingScreen />} />
        </Routes>
    );
};

export default CameraManager;
