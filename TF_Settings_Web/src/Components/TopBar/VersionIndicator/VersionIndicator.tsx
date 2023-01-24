import styles from './VersionIndicator.module.scss';

import classnames from 'classnames/bind';
import React from 'react';

const classes = classnames.bind(styles);

interface VersionIndicatorProps {
    touchFreeVersion: string;
}

const VersionIndicator: React.FC<VersionIndicatorProps> = ({ touchFreeVersion }) => {
    return (
        <div className={classes('version-container')}>{touchFreeVersion ? `TouchFree: v${touchFreeVersion}` : ''}</div>
    );
};

export default VersionIndicator;
