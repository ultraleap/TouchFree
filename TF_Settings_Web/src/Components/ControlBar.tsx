import React from 'react';

import { StatusIndicator } from './StatusIndicator';
import TabSelector from './Controls/TabSelector';

import logo from '../Images/Logo.png';
import backArrow from '../Images/Back_Arrow.svg';

import '../Styles/ControlBar.css';
import { useLocation, useNavigate } from 'react-router-dom';
import { TrackingServiceState } from '../TouchFree/TouchFreeToolingTypes';

interface ControlBarProps {
    status: TrackingServiceState;
}

const ControlBar: React.FC<ControlBarProps> = ({ status }) => {
    const location = useLocation();
    const navigate = useNavigate();

    return (
        <div className="overallContainerStyle">
            <div className="topBarStyle">
                <StatusIndicator status={status} />
                <img src={logo} alt="Logo: TouchFree by UltraLeap" className="horizElement" />
                <div className="emptyContainer" />
            </div>
            <div className="tabBarStyle">
                {location.pathname === '/camera' || location.pathname === '/interactions' ? (
                    <div className="tabBarStyle">
                        <TabSelector name="Camera" />
                        <TabSelector name="Interactions" />
                    </div>
                ) : (
                    <button onClick={() => navigate(-1)} className="backButtonStyle">
                        <img src={backArrow} alt="Arrow pointing back" className="arrowStyle" />
                        <p className="textStyle">Back</p>
                    </button>
                )}
            </div>
        </div>
    );
};

export default ControlBar;
