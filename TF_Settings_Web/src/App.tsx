import styles from './App.module.scss';

import classnames from 'classnames/bind';
import React, { useEffect } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import { ConnectionManager } from 'TouchFree/src/Connection/ConnectionManager';
import { ServiceStatus } from 'TouchFree/src/Connection/TouchFreeServiceTypes';
import TouchFree, { EventHandle } from 'TouchFree/src/TouchFree';
import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { AboutScreen, CameraManager, InteractionsScreen } from '@/Pages';

import { Header } from '@/Components/TopBar';

const classes = classnames.bind(styles);

const App: React.FC = () => {
    const [trackingStatus, setTrackingStatus] = React.useState<TrackingServiceState>(TrackingServiceState.UNAVAILABLE);
    const [touchFreeVersion, setTouchFreeVersion] = React.useState<string>('');

    useEffect(() => {
        TouchFree.Init({ initialiseCursor: true });

        const setTrackingStatusCallback = (detail: ServiceStatus) => {
            const status = detail.trackingServiceState;
            setTrackingStatus(status ?? TrackingServiceState.UNAVAILABLE);
        };

        let serviceChangeCallback: EventHandle;
        const whenConnectedHandler = TouchFree.RegisterEventCallback('WhenConnected', () => {
            ConnectionManager.RequestServiceStatus(setTrackingStatusCallback);
            serviceChangeCallback = TouchFree.RegisterEventCallback('OnServiceStatusChange', setTrackingStatusCallback);

            const serviceConnection = ConnectionManager.serviceConnection();
            const tfVersion = serviceConnection?.touchFreeVersion ?? '';
            setTouchFreeVersion(tfVersion);
        });

        return () => {
            whenConnectedHandler.UnregisterEventCallback();
            serviceChangeCallback.UnregisterEventCallback();
            TouchFree.GetInputController()?.disconnect();
        };
    }, []);

    return (
        <div className={classes('app')}>
            <Header trackingStatus={trackingStatus} touchFreeVersion={touchFreeVersion} />
            <Routes>
                <Route path="/settings/camera/*" element={<CameraManager trackingStatus={trackingStatus} />} />
                <Route path="/settings/interactions" element={<InteractionsScreen />} />
                <Route path="/settings/about" element={<AboutScreen />} />
                <Route path="*" element={<Navigate to="/settings/camera" replace />} />
            </Routes>
        </div>
    );
};

export default App;
