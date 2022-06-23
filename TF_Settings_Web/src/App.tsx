import './App.css';

import React, { useEffect, useRef } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import ControlBar from './Components/ControlBar';
import { CursorManager } from './Components/CursorManager';
import { CameraPage } from './Components/Pages/Camera/CameraPage';
import { InteractionsPage } from './Components/Pages/InteractionsPage';
import { ConnectionManager } from './TouchFree/Connection/ConnectionManager';
import { ServiceStatus } from './TouchFree/Connection/TouchFreeServiceTypes';
import { WebInputController } from './TouchFree/InputControllers/WebInputController';
import { TrackingServiceState } from './TouchFree/TouchFreeToolingTypes';

const App: React.FC = () => {
    const [tfStatus, setTfStatus] = React.useState<TrackingServiceState>(TrackingServiceState.UNAVAILABLE);

    const cursorParent = useRef<HTMLDivElement>(null);

    useEffect(() => {
        ConnectionManager.init();
        const controller: WebInputController = new WebInputController();

        const timerID = window.setInterval(() => {
            ConnectionManager.RequestServiceStatus((detail: ServiceStatus) => {
                const status = detail.trackingServiceState;
                if (status) {
                    setTfStatus(status);
                }
            });
        }, 5000);

        const cursorManager = new CursorManager();
        if (cursorParent.current !== null) {
            cursorManager.setElement(cursorParent.current);
        }

        return () => {
            controller.disconnect();
            clearInterval(timerID);
        };
    }, []);

    return (
        <div className="app" ref={cursorParent}>
            <ControlBar tfStatus={tfStatus} />
            <div style={{ marginTop: '170px' }}>
                <Routes>
                    <Route path="camera/*" element={<CameraPage />} />
                    <Route path="interactions/*" element={<InteractionsPage />} />
                    <Route path="*" element={<Navigate to="/camera" replace />} />
                </Routes>
            </div>
        </div>
    );
};

export default App;
