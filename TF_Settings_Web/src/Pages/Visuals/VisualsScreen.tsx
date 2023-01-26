import styles from './Visuals.module.scss';

import classNames from 'classnames/bind';
import React from 'react';

import { useIsLinux } from '@/customHooks';

const classes = classNames.bind(styles);

const VisualsScreen: React.FC = () => {
    if (useIsLinux()) return <></>;

    return (
        <div className={classes('container')}>
            <div className={classes('title-line')}>
                <h1> Visuals </h1>
            </div>

            <div className={classes('page-divider')} />
        </div>
    );
};

export default VisualsScreen;
