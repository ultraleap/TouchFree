import { Route, Routes } from 'react-router-dom';
import { TrackingServiceState } from 'TouchFree/TouchFreeToolingTypes';

import CameraSetupScreen from './CameraSetupScreen';
import { ManualSetupScreen } from './ManualSetupScreen';
import MaskingScreen from './Masking/MaskingScreen';
import QuickSetupManager from './QuickSetup/QuickSetupManager';

interface CameraManagerProps
{
    tfStatus:TrackingServiceState;
}

const CameraManager: React.FC<CameraManagerProps> = (props:CameraManagerProps) => {
    return (
        <Routes>
            <Route path="" element={<CameraSetupScreen />} />
            <Route path="quick/*" element={<QuickSetupManager />} />
            <Route path="manual" element={<ManualSetupScreen />} />
            <Route path="masking" element={<MaskingScreen tfStatus={props.tfStatus} />} />
        </Routes>
    );
};

export default CameraManager;
