import React from 'react';
import { Route, Routes } from 'react-router-dom';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { CameraSetupScreen, ManualSetupScreen, MaskingScreen, QuickSetupManager } from '@/Pages';

interface CameraManagerProps {
    trackingStatus: TrackingServiceState;
}

const CameraManager: React.FC<CameraManagerProps> = ({ trackingStatus }) => {
    return (
        <Routes>
            <Route path="" element={<CameraSetupScreen trackingStatus={trackingStatus} />} />
            <Route path="quick/*" element={<QuickSetupManager />} />
            <Route path="manual" element={<ManualSetupScreen />} />
            <Route path="masking" element={<MaskingScreen />} />
        </Routes>
    );
};

export default CameraManager;
