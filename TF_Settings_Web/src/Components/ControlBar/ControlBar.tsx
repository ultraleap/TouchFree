import classes from './ControlBar.module.scss';
import cssVariables from '@/variables.module.scss';

import React, { CSSProperties } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { BackArrow, Logo } from '@/Images';

import { VerticalIconTextButton } from '@/Components';

import { StatusIndicator, TabSelector, VersionIndicator } from './';

interface ControlBarProps {
    tfStatus: TrackingServiceState;
    touchFreeVersion: string;
}

const backButtonStyle: CSSProperties = {
    width: '200px',
    height: '80px',
    borderRadius: '30px',
    marginBottom: '30px',
    background: cssVariables.lightGreyGradient,
    display: 'flex',
    justifyContent: 'center',
    alignSelf: 'flex-end',
    alignItems: 'center',
};

const backButtonIconStyle: CSSProperties = {
    margin: '0',
    marginRight: '0.8rem',
};

const backButtonTitleStyle: CSSProperties = {
    fontSize: '28px',
    padding: '0',
    margin: '0',
};

const getBackLocation = (path: string): string => {
    const lastIndex = path.endsWith('/') ? path.slice(0, -1).lastIndexOf('/') : path.lastIndexOf('/');

    return path.slice(0, lastIndex);
};

const ControlBar: React.FC<ControlBarProps> = ({ tfStatus, touchFreeVersion }) => {
    const { pathname } = useLocation();

    return pathname.includes('calibrate') ? (
        <></>
    ) : (
        <div className={classes['control-bar-container']}>
            <StatusIndicator tfStatus={tfStatus} />
            <img src={Logo} alt="Logo: TouchFree by UltraLeap" className={classes['control-bar-logo']} />
            <VersionIndicator touchFreeVersion={touchFreeVersion} />
        </div>
    );
};

export default ControlBar;
