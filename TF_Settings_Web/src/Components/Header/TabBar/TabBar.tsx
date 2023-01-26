import styles from './TabBar.module.scss';

import classnames from 'classnames/bind';

import { useIsLinux } from '@/customHooks';

import { GearIcon, GearIconGlow } from '@/Images';

import { TabSelector } from '@/Components/Header';

const classes = classnames.bind(styles);

const TabBar = () => {
    const showVisualsTab = !useIsLinux();

    return (
        <div className={classes('tab-bar', { 'tab-bar--show-visuals': showVisualsTab })}>
            <TabSelector name="Camera" />
            <TabSelector name="Interactions" />
            {showVisualsTab && <TabSelector name="Visuals" />}
            <TabSelector name="About" icon={GearIcon} hoveredIcon={GearIconGlow} />
        </div>
    );
};

export default TabBar;
