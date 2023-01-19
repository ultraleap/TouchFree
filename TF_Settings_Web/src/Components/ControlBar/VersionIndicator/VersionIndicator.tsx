import classes from './VersionIndicator.module.scss';

import React from 'react';

interface VersionIndicatorProps {
    touchFreeVersion: string;
}

const VersionIndicator: React.FC<VersionIndicatorProps> = ({ touchFreeVersion }) => {
    return (
        <div className={classes['version-container']}>
            <div className={classes['version-information']}>
                {touchFreeVersion ? `TouchFree: v${touchFreeVersion}` : ''}
            </div>
        </div>
    );
};

export default VersionIndicator;
