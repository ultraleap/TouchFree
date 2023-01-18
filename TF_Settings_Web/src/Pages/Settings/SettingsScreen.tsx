import './Settings.scss';

import React, { useState, useEffect } from 'react';

import { ConnectionManager } from 'TouchFree/src/Connection/ConnectionManager';
import { ServiceStatus } from 'TouchFree/src/Connection/TouchFreeServiceTypes';
import TouchFree from 'TouchFree/src/TouchFree';

const SettingsScreen: React.FC = () => {
    const [tFVersion, setTFVersion] = useState<string | null>(null);
    const [trackingVersion, setTrackingVersion] = useState<string | null>(null);
    const [cameraFWVersion, setCameraFWVersion] = useState<string | null>(null);
    const [cameraSerial, setCameraSerial] = useState<string | null>(null);

    useEffect(() => {
        const setVersionInfo = (detail: ServiceStatus) => {
            const { cameraFirmwareVersion, cameraSerial, serviceVersion, trackingVersion } = detail;
            setTFVersion(serviceVersion);
            setTrackingVersion(trackingVersion);
            setCameraFWVersion(cameraFirmwareVersion);
            setCameraSerial(cameraSerial);
        };
        ConnectionManager.RequestServiceStatus(setVersionInfo);

        const onServiceChangeHandler = TouchFree.RegisterEventCallback('OnServiceStatusChange', setVersionInfo);

        return () => {
            onServiceChangeHandler.UnregisterEventCallback();
        };
    }, []);

    return (
        <>
            <div className="title-line">
                <h1> About </h1>
            </div>
            <div className="version-info">
                <VersionInfoEntry title="TouchFree Version" version={tFVersion} />
                <VersionInfoEntry title="Tracking Version" version={trackingVersion} />
                <VersionInfoEntry title="Camera Firmware Version" version={cameraFWVersion} />
                <VersionInfoEntry title="Camera Device ID" version={cameraSerial} />
            </div>
            <div className="page-divider" />
        </>
    );
};

interface VersionInfoEntryProps {
    title: string;
    version: string | null;
}

const VersionInfoEntry: React.FC<VersionInfoEntryProps> = ({ title, version }) => (
    <div className="version-info-entry">
        <span className="version-info-entry__title">{title}:</span>
        <span className="version-info-entry__version">{version}</span>
    </div>
);

export default SettingsScreen;
