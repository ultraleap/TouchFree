import styles from './TabSelector.module.scss';

import classnames from 'classnames/bind';
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

const classes = classnames.bind(styles);

interface TabSelectorProps {
    name: string;
    tabIndex: number;
    activeTabIndex: number;
    setAsActiveTab: (i: number) => void;
    icon?: string;
    hoveredIcon?: string;
    redirect?: boolean;
    forceHideDivider?: boolean;
    scheme?: 'light' | 'dark';
}

const TabSelector: React.FC<TabSelectorProps> = ({
    icon,
    name,
    hoveredIcon,
    tabIndex,
    activeTabIndex,
    setAsActiveTab,
    redirect = false,
    forceHideDivider = false,
    scheme = 'dark',
}) => {
    const [hovered, setHovered] = useState<boolean>(false);
    const [pressed, setPressed] = useState<boolean>(false);
    const [showDivider, setShowDivider] = useState<boolean>(forceHideDivider);

    const navigate = useNavigate();
    const isActiveTab = tabIndex === activeTabIndex;

    useEffect(() => {
        if (forceHideDivider) return;
        setShowDivider(isActiveTab || tabIndex + 1 === activeTabIndex);
    }, [icon, activeTabIndex]);

    const handleClick = () => {
        if (!isActiveTab) {
            setAsActiveTab(tabIndex);
            if (redirect) {
                navigate(`/settings/${name.toLowerCase()}`);
            }
        }
    };

    return (
        <div
            className={classes('tab', {
                'tab--light': scheme === 'light',
                'tab--active': isActiveTab,
            })}
        >
            <button
                className={classes('tab__button', {
                    'tab__button--pressed': !isActiveTab && pressed,
                    'tab__button--hovered': !isActiveTab && hovered,
                })}
                onPointerOver={() => setHovered(true)}
                onPointerLeave={() => {
                    setHovered(false);
                    setPressed(false);
                }}
                onPointerDown={() => {
                    setPressed(true);
                }}
                onPointerUp={() => {
                    if (pressed) {
                        handleClick();
                    }
                    setPressed(false);
                }}
                onKeyDown={(keyEvent) => {
                    if (keyEvent.key === 'Enter') handleClick();
                }}
            >
                <TabContent
                    name={name}
                    icon={icon}
                    hoveredIcon={!isActiveTab && hovered && hoveredIcon ? hoveredIcon : undefined}
                />
            </button>
            {<div className={classes('tab__divider', { 'tab__divider--hidden': showDivider })} />}
        </div>
    );
};

interface TabContentProps {
    name: string;
    icon?: string;
    hoveredIcon?: string;
}

const TabContent: React.FC<TabContentProps> = ({ name, icon, hoveredIcon }) => {
    if (!icon) return <span>{name}</span>;

    return <img className={classes('tab__button__icon')} src={hoveredIcon ?? icon} />;
};

export default TabSelector;
