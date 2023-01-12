import './VersionIndicator.scss';

import React from 'react';

interface VersionIndicatorProps {
    touchFreeVersion: string;
}

export const VersionIndicator: React.FC<VersionIndicatorProps> = ({ touchFreeVersion }) => {
    return (
        <div className="version-container">
            <div className="version-information">{touchFreeVersion ? `TouchFree: v${touchFreeVersion}` : ''}</div>
        </div>
    );
};
