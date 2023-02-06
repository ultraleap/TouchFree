import styles from './Header.module.scss';

import classnames from 'classnames/bind';
import React from 'react';
import { useLocation } from 'react-router-dom';

import { TrackingServiceState } from 'touchfree/src/TouchFreeToolingTypes';

import { Logo } from '@/Images';

import { StatusIndicator, VersionIndicator } from '..';

const classes = classnames.bind(styles);

interface HeaderProps {
    trackingStatus: TrackingServiceState;
    touchFreeVersion: string;
}

const Header: React.FC<HeaderProps> = ({ trackingStatus, touchFreeVersion }) => {
    const { pathname } = useLocation();

    if (pathname.includes('calibrate')) return <></>;

    return (
        <div className={classes('header-container')}>
            <StatusIndicator trackingStatus={trackingStatus} />
            <img src={Logo} alt="Logo: TouchFree by UltraLeap" className={classes('header-logo')} />
            <VersionIndicator touchFreeVersion={touchFreeVersion} />
        </div>
    );
};

export default Header;
