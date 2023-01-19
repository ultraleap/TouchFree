import classnames from 'classnames/bind';

import styles from './ControlBar.module.scss';

import React from 'react';
import { useLocation } from 'react-router-dom';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { Logo } from '@/Images';

import { StatusIndicator, VersionIndicator } from './';

const classes = classnames.bind(styles);

interface ControlBarProps {
    tfStatus: TrackingServiceState;
    touchFreeVersion: string;
}

const ControlBar: React.FC<ControlBarProps> = ({ tfStatus, touchFreeVersion }) => {
    const { pathname } = useLocation();

    return pathname.includes('calibrate') ? (
        <></>
    ) : (
        <div className={classes('control-bar-container')}>
            <StatusIndicator tfStatus={tfStatus} />
            <img src={Logo} alt="Logo: TouchFree by UltraLeap" className={classes('control-bar-logo')} />
            <VersionIndicator touchFreeVersion={touchFreeVersion} />
        </div>
    );
};

export default ControlBar;
