import styles from './App.module.scss';

import classnames from 'classnames/bind';
import React, { useEffect } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import { isDesktop, toggleFullScreen } from '@/TauriUtils';

import { ConnectionManager } from 'touchfree/src/Connection/ConnectionManager';
import { ServiceStatus } from 'touchfree/src/Connection/TouchFreeServiceTypes';
import TouchFree, { EventHandle } from 'touchfree/src/TouchFree';
import { TrackingServiceState } from 'touchfree/src/TouchFreeToolingTypes';

import { AboutScreen, CameraManager, InteractionsScreen, VisualsScreen } from '@/Pages';

import { Header } from '@/Components';

const classes = classnames.bind(styles);

const App: React.FC = () => {
    const [trackingStatus, setTrackingStatus] = React.useState<TrackingServiceState>(TrackingServiceState.UNAVAILABLE);

    useEffect(() => {
        const [ip, port] = window.location.host.split(':');
        TouchFree.Init({
            initialiseCursor: true,
            // env.MODE is updated by Vite automatically. Can be manually set by running `npm start -- --mode <string>
            address: import.meta.env.MODE !== 'development' ? { ip: ip, port: port } : undefined,
        });

        const setTrackingStatusCallback = (detail: ServiceStatus) => {
            const status = detail.trackingServiceState;
            setTrackingStatus(status ?? TrackingServiceState.UNAVAILABLE);
        };

        let serviceChangeCallback: EventHandle;
        const whenConnectedHandler = TouchFree.RegisterEventCallback('WhenConnected', () => {
            ConnectionManager.RequestServiceStatus(setTrackingStatusCallback);
            serviceChangeCallback = TouchFree.RegisterEventCallback('OnServiceStatusChange', setTrackingStatusCallback);
        });

        const fullScreenListener = (event: KeyboardEvent) => {
            if (event.code === 'F11' || (event.altKey && event.code === 'Enter')) {
                toggleFullScreen();
            }
        };

        if (isDesktop()) {
            window.addEventListener('keydown', fullScreenListener);
        }

        return () => {
            whenConnectedHandler.UnregisterEventCallback();
            serviceChangeCallback.UnregisterEventCallback();
            TouchFree.GetInputController()?.disconnect();
            window.removeEventListener('keydown', fullScreenListener);
        };
    }, []);

    return (
        <div className={classes('app')}>
            <Header trackingStatus={trackingStatus} />
            <Routes>
                <Route path="/settings/camera/*" element={<CameraManager trackingStatus={trackingStatus} />} />
                <Route path="/settings/interactions" element={<InteractionsScreen />} />
                <Route path="/settings/about" element={<AboutScreen />} />
                <Route path="/settings/visuals" element={<VisualsScreen />} />
                <Route path="*" element={<Navigate to="/settings/camera" replace />} />
            </Routes>
        </div>
    );
};

export default App;
