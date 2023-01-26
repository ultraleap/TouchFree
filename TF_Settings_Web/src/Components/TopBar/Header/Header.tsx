import styles from './Header.module.scss';

import classnames from 'classnames/bind';
import React from 'react';
import { useLocation } from 'react-router-dom';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { Logo } from '@/Images';

import { StatusIndicator } from '@/Components/TopBar';

const classes = classnames.bind(styles);

interface HeaderProps {
    trackingStatus: TrackingServiceState;
}

const Header: React.FC<HeaderProps> = ({ trackingStatus }) => {
    const { pathname } = useLocation();

    if (pathname.includes('calibrate')) return <></>;

    return (
        <div className={classes('header-container')}>
            <StatusIndicator trackingStatus={trackingStatus} />
            <img src={Logo} alt="Logo: TouchFree by UltraLeap" className={classes('header-logo')} />
        </div>
    );
};

export default Header;
