import './App.css';

import React, { useEffect } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import ControlBar from './Components/ControlBar';
import { CursorManager } from './Components/CursorManager';
import CameraManager from './Components/Pages/Camera/CameraManager';
import { InteractionsPage } from './Components/Pages/InteractionsPage';
import { ConnectionManager } from './TouchFree/Connection/ConnectionManager';
import { ServiceStatus } from './TouchFree/Connection/TouchFreeServiceTypes';
import { WebInputController } from './TouchFree/InputControllers/WebInputController';
import { TrackingServiceState } from './TouchFree/TouchFreeToolingTypes';

const App: React.FC = () => {
    const [tfStatus, setTfStatus] = React.useState<TrackingServiceState>(TrackingServiceState.UNAVAILABLE);

    useEffect(() => {
        /* eslint-disable @typescript-eslint/no-var-requires */
        /* eslint-disable unicorn/prefer-module */
        try {
            const VideoModeConfigurationClass = require('@brightsign/videomodeconfiguration');
            if (VideoModeConfigurationClass) {
                const videoConfig = new VideoModeConfigurationClass();
                videoConfig.setMode({ interlaced: true });
            }
        } catch {
            /*intentionally empty*/
        }
        /* eslint-enable @typescript-eslint/no-var-requires */
        /* eslint-enable unicorn/prefer-module */

        const updateTfStatus = () => {
            ConnectionManager.RequestServiceStatus((detail: ServiceStatus) => {
                const status = detail.trackingServiceState;
                if (status) {
                    setTfStatus(status);
                }
            });
        };

        ConnectionManager.init();

        ConnectionManager.AddConnectionListener(updateTfStatus);
        const controller: WebInputController = new WebInputController();

        const timerID = window.setInterval(updateTfStatus, 5000);

        new CursorManager();

        return () => {
            controller.disconnect();
            clearInterval(timerID);
        };
    }, []);

    return (
        <div className="app">
            <ControlBar tfStatus={tfStatus} />
            <div className="pageContent">
                <Routes>
                    <Route path="/settings/camera/*" element={<CameraManager />} />
                    <Route path="/settings/interactions" element={<InteractionsPage />} />
                    <Route path="*" element={<Navigate to="/settings/camera" replace />} />
                </Routes>
            </div>
        </div>
    );
};

export default App;
