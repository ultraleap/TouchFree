import styles from './ColorPicker.module.scss';

import classNames from 'classnames/bind';
import React, { useState } from 'react';

import { TabSelector } from '@/Components/Header';

const classes = classNames.bind(styles);

const tabNames = ['Outer Fill', 'Outer Border', 'Center Fill', 'Center Border'] as const;

const ColorPicker: React.FC = () => {
    const [activeTabIndex, setActiveTabIndex] = useState<number>(0);

    return (
        <div className={classes('color-picker')}>
            <div className={classes('color-picker__tabs')}>
                {tabNames.map((name, index) => (
                    <TabSelector
                        name={name}
                        key={index}
                        tabIndex={index}
                        forceHideDivider={index === tabNames.length - 1}
                        activeTabIndex={activeTabIndex}
                        setAsActiveTab={setActiveTabIndex}
                    />
                ))}
            </div>
            <div className={classes('color-picker__body')}></div>
        </div>
    );
};

export default ColorPicker;
