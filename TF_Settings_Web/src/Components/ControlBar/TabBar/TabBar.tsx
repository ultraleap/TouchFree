import styles from './TabBar.module.scss';

import classnames from 'classnames/bind';

import { GearIcon, GearIconGlow } from '@/Images';

import { TabSelector } from '@/Components';

const classes = classnames.bind(styles);

const TabBar = () => (
    <div className={classes('tab-bar')}>
        <TabSelector name="Camera" />
        <TabSelector name="Interactions" />
        <TabSelector name="Settings" icon={GearIcon} hoveredIcon={GearIconGlow} />
    </div>
);

export default TabBar;
