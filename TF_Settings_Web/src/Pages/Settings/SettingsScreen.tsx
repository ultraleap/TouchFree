import './Settings.scss';

import React, { useState, useEffect } from 'react';

import { TextButton } from '@/Components';

const SettingsScreen: React.FC = () => {
    const [tFVersion, setTFVersion] = useState<string>('');
    const [trackingVersion, setTrackingVersion] = useState<string>('');
    const [cameraFWVersion, setCameraFWVersion] = useState<string>('');
    const [cameraSerial, setCameraSerial] = useState<string>('');

    useEffect(() => {
        // Query from service once added in TF-930
        setTFVersion('2.2.1');
        setTrackingVersion('5.1');
        setCameraFWVersion('3.4');
        setCameraSerial('84237527');
    }, []);

    return (
        <>
            <div className="title-line">
                <h1> About </h1>
            </div>
            <div className="info-table--versions">
                <InfoTextEntry title="TouchFree Version" text={tFVersion} />
                <InfoTextEntry title="Tracking Version" text={trackingVersion} />
                <InfoTextEntry title="Camera Firmware Version" text={cameraFWVersion} />
                <InfoTextEntry title="Camera Serial Number" text={cameraSerial} />
            </div>
            <div className="page-divider" />
            <div className="title-line">
                <h1> Advanced Settings </h1>
            </div>
            <div className="info-table--advanced">
                <InfoButtonEntry
                    title="Tracking Log Files"
                    buttonTitle="Show Tracking Log Files"
                    onClick={() => console.log('Track')}
                />
                <InfoButtonEntry
                    title="TouchFree Log Files"
                    buttonTitle="Show TouchFree Log Files"
                    onClick={() => console.log('Touch')}
                />
            </div>
        </>
    );
};

interface InfoTextEntryProps {
    title: string;
    text: string;
}

const InfoTextEntry: React.FC<InfoTextEntryProps> = ({ title, text }) => (
    <div className="info-entry">
        <span className="info-entry__title">{title}:</span>
        <span className="info-entry__text">{text}</span>
    </div>
);

interface InfoButtonEntryProps {
    title: string;
    buttonTitle: string;
    onClick: () => void;
}

const InfoButtonEntry: React.FC<InfoButtonEntryProps> = ({ title, buttonTitle, onClick }) => (
    <div className="info-entry">
        <span className="info-entry__title">{title}:</span>
        <span className="info-entry__button">
            <TextButton title={buttonTitle} text={''} onClick={onClick} />
        </span>
    </div>
);

export default SettingsScreen;
