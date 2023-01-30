import styles from './ColorPicker.module.scss';

import classNames from 'classnames/bind';
import React, { useState } from 'react';

import { TabSelector } from '@/Components/Header';

const classes = classNames.bind(styles);

const ColorPicker: React.FC = () => {
    const [activeTabIndex, setActiveTabIndex] = useState<number>(0);

    return (
        <div className={classes('color-picker')}>
            <div className={classes('color-picker__tabs')}>
                <TabSelector
                    name="Outer Fill"
                    tabIndex={0}
                    activeTabIndex={activeTabIndex}
                    setAsActiveTab={setActiveTabIndex}
                />
                <TabSelector
                    name="Outer Border"
                    tabIndex={1}
                    activeTabIndex={activeTabIndex}
                    setAsActiveTab={setActiveTabIndex}
                />
                <TabSelector
                    name="Center Fill"
                    tabIndex={2}
                    activeTabIndex={activeTabIndex}
                    setAsActiveTab={setActiveTabIndex}
                />
                <TabSelector
                    name="Center Border"
                    tabIndex={3}
                    forceHideDivider
                    activeTabIndex={activeTabIndex}
                    setAsActiveTab={setActiveTabIndex}
                />
            </div>
            <div className={classes('color-picker__body')}></div>
        </div>
    );
};

export default ColorPicker;
