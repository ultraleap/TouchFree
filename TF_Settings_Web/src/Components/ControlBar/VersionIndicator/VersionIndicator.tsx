import classes from './VersionIndicator.module.scss';

import React from 'react';

interface VersionIndicatorProps {
    touchFreeVersion: string;
}

const VersionIndicator: React.FC<VersionIndicatorProps> = ({ touchFreeVersion }) => {
    return (
        <div className={classes['version-container']}>{touchFreeVersion ? `TouchFree: v${touchFreeVersion}` : ''}</div>
    );
};

export default VersionIndicator;
