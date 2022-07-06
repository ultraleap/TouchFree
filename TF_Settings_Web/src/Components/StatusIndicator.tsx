import '../Styles/StatusIndicator.css';

import React, { CSSProperties } from 'react';

import camStatusIcon from '../Images/Camera_Status_Icon.png';
import svcStatusIcon from '../Images/Tracking_Status_Icon.svg';
import { TrackingServiceState } from '../TouchFree/TouchFreeToolingTypes';

interface StatusIndicatorProps {
    tfStatus: TrackingServiceState;
}

const statusDotOK: CSSProperties = {
    backgroundImage: 'linear-gradient(180deg, #00eb86, #00cdcf)',
};

const statusDotBad: CSSProperties = {
    backgroundImage: 'linear-gradient(180deg, #e2164d, #d11883)',
};

export const StatusIndicator: React.FC<StatusIndicatorProps> = ({ tfStatus }) => {
    return (
        <div className="statusContainer">
            <div className="statusIconContainer">
                <img src={camStatusIcon} alt="Camera Status Icon" />
                <div
                    className="statusDot"
                    style={tfStatus === TrackingServiceState.CONNECTED ? statusDotOK : statusDotBad}
                />
            </div>
            <div className="statusIconContainer">
                <img src={svcStatusIcon} alt="Tracking Service Status Icon" />
                <div
                    className="statusDot"
                    style={tfStatus === TrackingServiceState.UNAVAILABLE ? statusDotBad : statusDotOK}
                />
            </div>
        </div>
    );
};
