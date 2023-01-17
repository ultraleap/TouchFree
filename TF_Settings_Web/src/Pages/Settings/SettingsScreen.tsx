import './Settings.scss';

import React, { useState, useEffect } from 'react';

const SettingsScreen: React.FC = () => {
    const [tFVersion, setTFVersion] = useState<string>('');
    const [trackingVersion, setTrackingVersion] = useState<string>('');
    const [cameraFWVersion, setCameraFWVersion] = useState<string>('');
    const [cameraID, setCameraID] = useState<string>('');

    useEffect(() => {
        // Query from service once added in TF-930
        setTFVersion('2.2.1');
        setTrackingVersion('5.1');
        setCameraFWVersion('3.4');
        setCameraID('84237527');
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
                <VersionInfoEntry title="Camera Device ID" version={cameraID} />
            </div>
            <div className="page-divider" />
        </>
    );
};

interface VersionInfoEntryProps {
    title: string;
    version: string;
}

const VersionInfoEntry: React.FC<VersionInfoEntryProps> = ({ title, version }) => (
    <div className="version-info-entry">
        <span className="version-info-entry__title">{title}:</span>
        <span className="version-info-entry__version">{version}</span>
    </div>
);

export default SettingsScreen;
