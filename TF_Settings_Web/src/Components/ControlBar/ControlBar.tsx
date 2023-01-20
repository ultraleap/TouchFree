import styles from './ControlBar.module.scss';

import classnames from 'classnames/bind';
import React from 'react';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { Logo } from '@/Images';

import { StatusIndicator, VersionIndicator } from './';

const classes = classnames.bind(styles);

interface ControlBarProps {
    trackingStatus: TrackingServiceState;
    touchFreeVersion: string;
}

const ControlBar: React.FC<ControlBarProps> = ({ trackingStatus, touchFreeVersion }) => {
    return (
        <div className={classes('control-bar-container')}>
            <StatusIndicator trackingStatus={trackingStatus} />
            <img src={Logo} alt="Logo: TouchFree by UltraLeap" className={classes('control-bar-logo')} />
            <VersionIndicator touchFreeVersion={touchFreeVersion} />
        </div>
    );
};

export default ControlBar;
