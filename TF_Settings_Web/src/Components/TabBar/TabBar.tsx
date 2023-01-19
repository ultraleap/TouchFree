import classes from './TabBar.module.scss';

import { useLocation } from 'react-router-dom';

import { TabSelector } from '@/Components';

const TabBar = () => {
    const { pathname } = useLocation();

    return (
        <div className={classes['tab-bar']}>
            <TabSelector name="Camera" isActiveTab={pathname.endsWith('camera')} />
            <TabSelector name="Interactions" isActiveTab={pathname.endsWith('interactions')} />
        </div>
    );
};

export default TabBar;
