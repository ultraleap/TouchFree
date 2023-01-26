import styles from './Header.module.scss';

import classnames from 'classnames/bind';
import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { Logo } from '@/Images';

import { StatusIndicator, TabBar } from '@/Components/Header';

const classes = classnames.bind(styles);

interface HeaderProps {
    trackingStatus: TrackingServiceState;
}

const Header: React.FC<HeaderProps> = ({ trackingStatus }) => {
    const [showTabBar, setShowTabBar] = useState<boolean>(true);
    const { pathname } = useLocation();

    useEffect(() => {
        setShowTabBar(!pathname.split('/settings/')[1]?.includes('/'));
    }, [pathname]);

    if (pathname.includes('calibrate')) return <></>;

    return (
        <div className={classes('header')}>
            <div className={classes('header__top')}>
                <StatusIndicator trackingStatus={trackingStatus} />
                <img src={Logo} alt="Logo: TouchFree by UltraLeap" className={classes('header__top__logo')} />
            </div>
            {showTabBar && <TabBar />}
        </div>
    );
};

export default Header;