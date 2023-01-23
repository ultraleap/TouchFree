import styles from './Header.module.scss';

import classnames from 'classnames/bind';
import React from 'react';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { Logo } from '@/Images';

import { StatusIndicator, VersionIndicator } from '..';

const classes = classnames.bind(styles);

interface HeaderProps {
    trackingStatus: TrackingServiceState;
    touchFreeVersion: string;
}

const Header: React.FC<HeaderProps> = ({ trackingStatus, touchFreeVersion }) => (
    <div className={classes('header-container')}>
        <StatusIndicator trackingStatus={trackingStatus} />
        <img src={Logo} alt="Logo: TouchFree by UltraLeap" className={classes('header-logo')} />
        <VersionIndicator touchFreeVersion={touchFreeVersion} />
    </div>
);

export default Header;
