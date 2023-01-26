import styles from './TabSelector.module.scss';

import classnames from 'classnames/bind';
import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';

const classes = classnames.bind(styles);

interface TabSelectorProps {
    name: string;
    icon?: string;
    hoveredIcon?: string;
}

const TabSelector: React.FC<TabSelectorProps> = ({ icon, name, hoveredIcon }) => {
    const navigate = useNavigate();
    const { pathname } = useLocation();

    const [hovered, setHovered] = useState<boolean>(false);
    const [pressed, setPressed] = useState<boolean>(false);
    const [isActiveTab, setIsActiveTab] = useState<boolean>(false);

    useEffect(() => {
        setIsActiveTab(pathname.endsWith(name.toLowerCase()));
    }, [pathname]);

    const handleClick = () => {
        if (!isActiveTab) {
            navigate(`/settings/${name.toLowerCase()}`);
        }
    };

    return (
        <div className={classes('tab')}>
            <button
                className={classes('tab__button', {
                    'tab__button--active': isActiveTab,
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
        </div>
    );
};

const TabContent: React.FC<TabSelectorProps> = ({ name, icon, hoveredIcon }) => {
    // return <span>{name}</span>;

    if (!icon) return <span>{name}</span>;

    return <img className={classes('tab__button__icon')} src={hoveredIcon ?? icon} />;
};

export default TabSelector;
