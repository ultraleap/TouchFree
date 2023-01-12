import ControlBar from '@/Components/ControlBar/ControlBar';
import { InteractionsPage } from '@/Components/Pages/Interactions/InteractionsScreen';

import './App.scss';

import React, { useEffect } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import { ConnectionManager } from 'TouchFree/src/Connection/ConnectionManager';
import { ServiceStatus } from 'TouchFree/src/Connection/TouchFreeServiceTypes';
import { WebInputController } from 'TouchFree/src/InputControllers/WebInputController';
import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { CursorManager } from 'Components/CursorManager';
import CameraManager from 'Components/Pages/Camera/CameraManager';

const App: React.FC = () => {
    const [tfStatus, setTfStatus] = React.useState<TrackingServiceState>(TrackingServiceState.UNAVAILABLE);
    const [touchFreeVersion, setTouchFreeVersion] = React.useState<string>('');

    useEffect(() => {
        ConnectionManager.init();

        const onConnected = () => {
            ConnectionManager.RequestServiceStatus((detail: ServiceStatus) => {
                const status = detail.trackingServiceState;
                if (status) {
                    setTfStatus(status);
                }
            });

            const serviceConnection = ConnectionManager.serviceConnection();
            const tfVersion = serviceConnection?.touchFreeVersion ?? '';
            setTouchFreeVersion(tfVersion);
        };

        ConnectionManager.AddConnectionListener(onConnected);
        ConnectionManager.AddServiceStatusListener(setTfStatus);
        const controller: WebInputController = new WebInputController();

        new CursorManager();

        return () => {
            controller.disconnect();
        };
    }, []);

    return (
        <div className="app">
            <ControlBar tfStatus={tfStatus} touchFreeVersion={touchFreeVersion} />
            <div className="page-content">
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
