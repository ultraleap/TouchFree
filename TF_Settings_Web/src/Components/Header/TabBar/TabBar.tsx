import styles from './TabBar.module.scss';

import classnames from 'classnames/bind';
import { useState } from 'react';

import { useIsLinux } from '@/customHooks';

import { GearIcon, GearIconGlow } from '@/Images';

import { TabSelector } from '@/Components/Header';

const classes = classnames.bind(styles);

const TabBar = () => {
    const showVisualsTab = !useIsLinux();
    const [activeTabIndex, setActiveTabIndex] = useState<number>(0);

    return (
        <div className={classes('tab-bar', { 'tab-bar--show-visuals': showVisualsTab })}>
            <TabSelector
                name="Camera"
                tabIndex={0}
                activeTabIndex={activeTabIndex}
                setAsActiveTab={(i) => setActiveTabIndex(i)}
            />
            <TabSelector
                name="Interactions"
                tabIndex={1}
                activeTabIndex={activeTabIndex}
                setAsActiveTab={(i) => setActiveTabIndex(i)}
            />
            {showVisualsTab && (
                <TabSelector
                    name="Visuals"
                    tabIndex={2}
                    activeTabIndex={activeTabIndex}
                    setAsActiveTab={(i) => setActiveTabIndex(i)}
                />
            )}
            <TabSelector
                name="About"
                icon={GearIcon}
                hoveredIcon={GearIconGlow}
                tabIndex={showVisualsTab ? 3 : 2}
                activeTabIndex={activeTabIndex}
                setAsActiveTab={(i) => setActiveTabIndex(i)}
            />
        </div>
    );
};

export default TabBar;
