import './Settings.scss';

import React, { useState, useEffect } from 'react';

import { isBRS } from '@/customHooks';

import { ConnectionManager } from 'TouchFree/src/Connection/ConnectionManager';
import { ServiceStatus } from 'TouchFree/src/Connection/TouchFreeServiceTypes';
import TouchFree from 'TouchFree/src/TouchFree';

import { TextButton } from '@/Components';

const SettingsScreen: React.FC = () => {
    const [tFVersion, setTFVersion] = useState<string>('');
    const [trackingVersion, setTrackingVersion] = useState<string>('');
    const [cameraFWVersion, setCameraFWVersion] = useState<string>('');
    const [cameraSerial, setCameraSerial] = useState<string>('');

    useEffect(() => {
        const setVersionInfo = (detail: ServiceStatus) => {
            const { cameraFirmwareVersion, cameraSerial, serviceVersion, trackingVersion } = detail;
            setTFVersion(serviceVersion);
            setTrackingVersion(trackingVersion);
            setCameraFWVersion(cameraFirmwareVersion);
            setCameraSerial(cameraSerial);
        };

        if (TouchFree.IsConnected()) {
            ConnectionManager.RequestServiceStatus(setVersionInfo);
        }

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
            <div className="info-table--versions">
                <InfoTextEntry title="TouchFree Version" text={tFVersion} />
                <InfoTextEntry title="Tracking Version" text={trackingVersion} />
                <InfoTextEntry title="Camera Firmware Version" text={cameraFWVersion} />
                <InfoTextEntry title="Camera Serial Number" text={cameraSerial} />
            </div>
            <div className="page-divider" />
            {!isBRS() && (
                <>
                    <div className="title-line">
                        <h1> Advanced Settings </h1>
                    </div>
                    <div className="info-table--advanced">
                        <InfoButtonEntry
                            title="Tracking Log Files"
                            buttonTitle="Show Tracking Log Files"
                            onClick={() => {
                                console.log('C:/ProgramData/Ultraleap/HandTracker/Logs');
                            }}
                        />
                        <InfoButtonEntry
                            title="TouchFree Log Files"
                            buttonTitle="Show TouchFree Log Files"
                            onClick={() => console.log('C:/ProgramData/Ultraleap/TouchFree/Logs')}
                        />
                    </div>
                </>
            )}
        </>
    );
};

interface InfoTextEntryProps {
    title: string;
    text: string;
}

const InfoTextEntry: React.FC<InfoTextEntryProps> = ({ title, text }) => (
    <div className="info-table__entry">
        <span className="info-table__entry__title">{title}:</span>
        <span className="info-table__entry__text">{text}</span>
    </div>
);

interface InfoButtonEntryProps {
    title: string;
    buttonTitle: string;
    onClick: () => void;
}

const InfoButtonEntry: React.FC<InfoButtonEntryProps> = ({ title, buttonTitle, onClick }) => (
    <div className="info-table__entry">
        <span className="info-table__entry__title">{title}:</span>
        <span className="info-table__entry__button">
            <TextButton title={buttonTitle} text={''} onClick={onClick} />
        </span>
    </div>
);

export default SettingsScreen;
