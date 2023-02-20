import styles from './About.module.scss';

import classNames from 'classnames/bind';
import React, { useState, useEffect } from 'react';

import { openWithShell, isDesktop } from '@/TauriUtils';

import { ConnectionManager } from 'touchfree/src/Connection/ConnectionManager';
import { ServiceStatus } from 'touchfree/src/Connection/TouchFreeServiceTypes';
import TouchFree from 'touchfree/src/TouchFree';

import { TextButton } from '@/Components';

const classes = classNames.bind(styles);

const AboutScreen: React.FC = () => {
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
        <div className={classes('container')}>
            <div className={classes('title-line')}>
                <h1> About </h1>
            </div>
            <div className={classes('info-table')}>
                <InfoTextEntry title="TouchFree Version" text={tFVersion} />
                <InfoTextEntry title="Tracking Version" text={trackingVersion} />
                <InfoTextEntry title="Camera Firmware Version" text={cameraFWVersion} />
                <InfoTextEntry title="Camera Serial Number" text={cameraSerial} />
            </div>
            <div className={classes('page-divider')} />
            {isDesktop() && (
                <>
                    <div className={classes('title-line')}>
                        <h1> Advanced </h1>
                    </div>
                    <div className={classes('info-table')}>
                        <InfoButtonEntry
                            title="Tracking Log Files"
                            buttonTitle="Show Tracking Log Files"
                            onClick={() => openWithShell('C:/ProgramData/Ultraleap/HandTracker/Logs')}
                        />
                        <InfoButtonEntry
                            title="TouchFree Log Files"
                            buttonTitle="Show TouchFree Log Files"
                            onClick={() => openWithShell('C:/ProgramData/Ultraleap/TouchFree/Logs')}
                        />
                    </div>
                </>
            )}
        </div>
    );
};

interface InfoTextEntryProps {
    title: string;
    text: string;
}

const InfoTextEntry: React.FC<InfoTextEntryProps> = ({ title, text }) => (
    <div className={classes('info-table__entry')}>
        <span className={classes('info-table__entry__title')}>{title}:</span>
        <span className={classes('info-table__entry__text')}>{text}</span>
    </div>
);

interface InfoButtonEntryProps {
    title: string;
    buttonTitle: string;
    onClick: () => void;
}

const InfoButtonEntry: React.FC<InfoButtonEntryProps> = ({ title, buttonTitle, onClick }) => (
    <div className={classes('info-table__entry')}>
        <span className={classes('info-table__entry__title')}>{title}:</span>
        <span className={classes('info-table__entry__button')}>
            <TextButton title={buttonTitle} onClick={onClick} />
        </span>
    </div>
);

export default AboutScreen;
