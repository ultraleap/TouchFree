import './App.css';

import React, { useEffect } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import ControlBar from './Components/ControlBar';
import { CursorManager } from './Components/CursorManager';
import CameraPage from './Components/Pages/Camera/CameraPage';
import { InteractionsPage } from './Components/Pages/InteractionsPage';
import { ConnectionManager } from './TouchFree/Connection/ConnectionManager';
import { ServiceStatus } from './TouchFree/Connection/TouchFreeServiceTypes';
import { WebInputController } from './TouchFree/InputControllers/WebInputController';
import { TrackingServiceState } from './TouchFree/TouchFreeToolingTypes';

const App: React.FC = () => {
    const [tfStatus, setTfStatus] = React.useState<TrackingServiceState>(TrackingServiceState.UNAVAILABLE);

    useEffect(() => {
        ConnectionManager.init();

        ConnectionManager.AddConnectionListener(() => {
            ConnectionManager.RequestServiceStatus((detail: ServiceStatus) => {
                const status = detail.trackingServiceState;
                if (status) {
                    setTfStatus(status);
                }
            });
        });
        const controller: WebInputController = new WebInputController();

        const timerID = window.setInterval(() => {
            ConnectionManager.RequestServiceStatus((detail: ServiceStatus) => {
                const status = detail.trackingServiceState;
                if (status) {
                    setTfStatus(status);
                }
            });
        }, 5000);

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
                    <Route path="/settings/camera/*" element={<CameraPage />} />
                    <Route path="/settings/interactions/*" element={<InteractionsPage />} />
                    <Route path="*" element={<Navigate to="/settings/camera" replace />} />
                </Routes>
            </div>
        </div>
    );
};

export default App;
