import './App.scss';

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
        ConnectionManager.init();

        const requestServiceStatus = () => ConnectionManager.RequestServiceStatus((detail: ServiceStatus) => {
            const status = detail.trackingServiceState;
            if (status) {
                setTfStatus(status);
            }
        });

        const updateTfStatus = (serviceStatus:TrackingServiceState) => {
            setTfStatus(serviceStatus);
        };

        ConnectionManager.AddConnectionListener(requestServiceStatus);
        ConnectionManager.AddServiceStatusListener(updateTfStatus);
        const controller: WebInputController = new WebInputController();

        new CursorManager();

        return () => {
            controller.disconnect();
        };
    }, []);

    return (
        <div className="app">
            <ControlBar tfStatus={tfStatus} />
            <div className="page-content">
                <Routes>
                    <Route path="/settings/camera/*" element={<CameraManager tfStatus={tfStatus} />} />
                    <Route path="/settings/interactions" element={<InteractionsPage />} />
                    <Route path="*" element={<Navigate to="/settings/camera" replace />} />
                </Routes>
            </div>
        </div>
    );
};

export default App;
