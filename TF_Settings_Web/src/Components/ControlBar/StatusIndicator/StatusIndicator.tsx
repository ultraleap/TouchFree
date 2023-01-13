import './StatusIndicator.scss';

import React from 'react';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { CameraStatusIcon, HandIcon } from '@/Images';

interface StatusIndicatorProps {
    tfStatus: TrackingServiceState;
}
interface StatusIndicator {
    title: string;
    icon: string;
    className: string;
}

const getStatusIndicators = (tfStatus: TrackingServiceState): StatusIndicator[] => {
    return [
        {
            title: 'Camera',
            icon: CameraStatusIcon,
            className: tfStatus === TrackingServiceState.CONNECTED ? 'status-dot-ok' : 'status-dot-bad',
        },
        {
            title: 'Tracking Service',
            icon: HandIcon,
            className: tfStatus === TrackingServiceState.UNAVAILABLE ? 'status-dot-bad' : 'status-dot-ok',
        },
    ];
};

const StatusIndicator: React.FC<StatusIndicatorProps> = ({ tfStatus }) => {
    return (
        <div className="status-container">
            {getStatusIndicators(tfStatus).map((indicator: StatusIndicator) => (
                <div className="status-icon-container" key={indicator.title}>
                    <img src={indicator.icon} alt={`${indicator.title} Status Icon`} />
                    <div className={`status-dot ${indicator.className}`} />
                </div>
            ))}
        </div>
    );
};

export default StatusIndicator;
