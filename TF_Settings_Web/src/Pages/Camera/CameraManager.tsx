import { Route, Routes } from 'react-router-dom';

import { CameraSetupScreen, ManualSetupScreen, MaskingScreen, QuickSetupManager } from '@/Pages';

const CameraManager: React.FC = () => {
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
