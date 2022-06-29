import '../../../Styles/Camera/CameraPage.css';

import React from 'react';
import { Route, Routes } from 'react-router-dom';

import CameraCalibrateComplete from './CameraCalibrateComplete';
import CameraCalibratePage from './CameraCalibratePage';
import CameraPosition, { PositionType } from './CameraPosition';
import CameraSetupSelection from './CameraSetupSelection';
import { ManualSetup } from './ManualSetup';

const CameraPage = () => {
    const [activePosition, setActivePosition] = React.useState<PositionType>(null);

    return (
        <>
            <Routes>
                <Route path="" element={<CameraSetupSelection />} />
                <Route
                    path="quick"
                    element={
                        <CameraPosition
                            configPosition={activePosition}
                            setPosition={(position: PositionType) => setActivePosition(position)}
                        />
                    }
                />
                <Route path="quick/calibrate/*" element={<CameraCalibratePage configPosition={activePosition} />} />
                <Route path="quick/calibrate/complete" element={<CameraCalibrateComplete />} />
                <Route path="manual" element={<ManualSetup />} />
            </Routes>
        </>
    );
};

export default CameraPage;
