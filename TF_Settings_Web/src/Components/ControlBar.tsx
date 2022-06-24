import '../Styles/ControlBar.css';

import React, { CSSProperties } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import backArrow from '../Images/Back_Arrow.svg';
import logo from '../Images/Logo.png';
import { TrackingServiceState } from '../TouchFree/TouchFreeToolingTypes';
import IconTextButton from './Controls/IconTextButton';
import TabSelector from './Controls/TabSelector';
import { StatusIndicator } from './StatusIndicator';

interface ControlBarProps {
    tfStatus: TrackingServiceState;
}

const backButtonStyle: CSSProperties = {
    width: '180px',
    height: '60px',
    borderRadius: '33px',
    marginLeft: '1%',
    marginBottom: '2%',
    background: 'transparent linear-gradient(180deg, #5c5c5c 0%, #454545 100%) 0% 0% no-repeat padding-box',
    display: 'flex',
    justifyContent: 'center',
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

const ControlBar: React.FC<ControlBarProps> = ({ tfStatus }) => {
    const { pathname } = useLocation();
    const navigate = useNavigate();

    const hideBar = pathname.includes('calibrate');

    return hideBar ? (
        <></>
    ) : (
        <div className="overallContainerStyle">
            <div className="topBarStyle">
                <StatusIndicator tfStatus={tfStatus} />
                <img src={logo} alt="Logo: TouchFree by UltraLeap" className="horizElement" />
                <div className="emptyContainer" />
            </div>
            <div className="tabBarStyle">
                {pathname === '/camera' || pathname === '/interactions' ? (
                    <div className="tabBarStyle">
                        <TabSelector name="Camera" />
                        <TabSelector name="Interactions" />
                    </div>
                ) : (
                    <IconTextButton
                        buttonStyle={backButtonStyle}
                        icon={backArrow}
                        alt="Arrow pointing back"
                        iconStyle={backButtonIconStyle}
                        title="Back"
                        titleStyle={backButtonTitleStyle}
                        text={''}
                        textStyle={{ display: 'none' }}
                        onClick={() => navigate(-1)}
                    />
                )}
            </div>
        </div>
    );
};

export default ControlBar;
