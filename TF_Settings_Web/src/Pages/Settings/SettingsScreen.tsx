import './Settings.scss';

import React, { useState } from 'react';

const SettingsScreen: React.FC = () => {
    const [tFVersion, setTFVersion] = useState<string>('2.2.1');
    const [trackingVersion, setTrackingVersion] = useState<string>('5.1');
    const [cameraFWVersion, setCameraFWVersion] = useState<string>('3.4');
    const [cameraID, setCameraID] = useState<string>('84237527');

    return (
        <div>
            <div className="title-line">
                <h1> About </h1>
            </div>
            <div className="version-info">
                <VersionInfoEntry title="TouchFree Version" version={tFVersion} />
                <VersionInfoEntry title="Tracking Version" version={trackingVersion} />
                <VersionInfoEntry title="Camera Firmware Version" version={cameraFWVersion} />
                <VersionInfoEntry title="Camera Device ID" version={cameraID} />
            </div>
            <div className="page-divider" />
        </div>
    );
};

interface VersionInfoEntryProps {
    title: string;
    version: string;
}

const VersionInfoEntry: React.FC<VersionInfoEntryProps> = ({ title, version }) => (
    <div>
        <span>{title}</span>
        <span>{version}</span>
    </div>
);

export default SettingsScreen;
