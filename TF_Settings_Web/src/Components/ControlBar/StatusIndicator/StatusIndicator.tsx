import './StatusIndicator.scss';

import React from 'react';

import { TrackingServiceState } from 'TouchFree/src/TouchFreeToolingTypes';

import { CameraStatusIcon, HandIcon } from '@/Images';

interface StatusIndicatorProps {
    trackingStatus: TrackingServiceState;
}
interface StatusIndicator {
    title: string;
    icon: string;
    className: string;
}

const StatusIndicator: React.FC<StatusIndicatorProps> = ({ trackingStatus }) => {
    return (
        <div className="status-container">
            {getStatusIndicators(trackingStatus).map((indicator: StatusIndicator) => (
                <div className="status-icon-container" key={indicator.title}>
                    <img src={indicator.icon} alt={`${indicator.title} Status Icon`} />
                    <div className={`status-dot ${indicator.className}`} />
                </div>
            ))}
        </div>
    );
};

const getStatusIndicators = (trackingStatus: TrackingServiceState): StatusIndicator[] => {
    return [
        {
            title: 'Camera',
            icon: CameraStatusIcon,
            className: trackingStatus === TrackingServiceState.CONNECTED ? 'status-dot-ok' : 'status-dot-bad',
        },
        {
            title: 'Tracking Service',
            icon: HandIcon,
            className: trackingStatus === TrackingServiceState.UNAVAILABLE ? 'status-dot-bad' : 'status-dot-ok',
        },
    ];
};

export default StatusIndicator;
