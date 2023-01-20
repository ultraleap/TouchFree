import styles from './TabSelector.module.scss';

import classnames from 'classnames/bind';
import React, { useState, useEffect, useMemo } from 'react';
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

    const content = useMemo((): JSX.Element => {
        if (!icon) return <span>{name}</span>;

        const showHoveredIcon = !isActiveTab && hovered && hoveredIcon;
        return (
            <div className={classes('icon--container')}>
                <img src={showHoveredIcon ? hoveredIcon : icon} />{' '}
            </div>
        );
    }, [icon, name, hovered, isActiveTab]);

    return (
        <button
            className={classes('tab-button', {
                'tab-button--active': isActiveTab,
                'tab-button--pressed': pressed,
                'tab-button--hovered': hovered,
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
            {content}
        </button>
    );
};

export default TabSelector;
