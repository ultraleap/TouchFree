import styles from './Header.module.scss';

import classnames from 'classnames/bind';
import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';

import { isDesktop } from '@/TauriUtils';
import { useIsFullScreen } from '@/customHooks';

import { TrackingServiceState } from 'touchfree/src/TouchFreeToolingTypes';

import { Logo } from '@/Images';

import { StatusIndicator, TabBar, WindowControls } from '@/Components/Header';

const classes = classnames.bind(styles);

interface HeaderProps {
    trackingStatus: TrackingServiceState;
}

const Header: React.FC<HeaderProps> = ({ trackingStatus }) => {
    const [showTabBar, setShowTabBar] = useState<boolean>(true);
    const { pathname } = useLocation();
    const isFullScreen = useIsFullScreen();

    useEffect(() => {
        setShowTabBar(!pathname.split('/settings/')[1]?.includes('/'));
    }, [pathname]);

    if (pathname.includes('calibrate')) return <></>;

    return (
        <div className={classes('header')}>
            <div className={classes('header__top')}>
                <StatusIndicator trackingStatus={trackingStatus} />
                <img src={Logo} alt="Logo: TouchFree by UltraLeap" className={classes('header__top__logo')} />
                {isDesktop() && isFullScreen && <WindowControls />}
            </div>
            {showTabBar && <TabBar />}
        </div>
    );
};

export default Header;
