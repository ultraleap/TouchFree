import styles from './TabBar.module.scss';

import classnames from 'classnames/bind';
import { useState } from 'react';

import { useIsDesktop } from '@/customHooks';

import { GearIcon, GearIconGlow } from '@/Images';

import { TabSelector } from '@/Components/Header';

const classes = classnames.bind(styles);

const TabBar = () => {
    const showVisualsTab = useIsDesktop();
    const [activeTabIndex, setActiveTabIndex] = useState<number>(0);

    return (
        <div className={classes('tab-bar', { 'tab-bar--show-visuals': showVisualsTab })}>
            <TabSelector
                name="Camera"
                tabIndex={0}
                redirect
                activeTabIndex={activeTabIndex}
                setAsActiveTab={setActiveTabIndex}
            />
            <TabSelector
                name="Interactions"
                tabIndex={1}
                redirect
                activeTabIndex={activeTabIndex}
                setAsActiveTab={setActiveTabIndex}
            />
            {showVisualsTab && (
                <TabSelector
                    name="Visuals"
                    tabIndex={2}
                    redirect
                    activeTabIndex={activeTabIndex}
                    setAsActiveTab={setActiveTabIndex}
                />
            )}
            <TabSelector
                name="About"
                icon={GearIcon}
                hoveredIcon={GearIconGlow}
                tabIndex={showVisualsTab ? 3 : 2}
                redirect
                forceHideDivider
                activeTabIndex={activeTabIndex}
                setAsActiveTab={setActiveTabIndex}
            />
        </div>
    );
};

export default TabBar;
