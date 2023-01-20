import styles from './TabSelector.module.scss';

import classnames from 'classnames/bind';
import React, { useCallback, useState, useEffect } from 'react';

const classes = classnames.bind(styles);

interface TabSelectorProps {
    name: string;
    icon?: string;
    hoveredIcon?: string;
    isActiveTab: boolean;
    onClick?: () => void;
}

const TabSelector: React.FC<TabSelectorProps> = ({ icon, name, hoveredIcon, isActiveTab, onClick }) => {
    const [hovered, setHovered] = useState<boolean>(false);
    const [pressed, setPressed] = useState<boolean>(false);
    const [tabContent, setTabContent] = useState<JSX.Element>();

    const handleClick = () => {
        if (!isActiveTab && onClick) onClick();
    };

    const getSpecialClassName = useCallback((): string => {
        if (isActiveTab) return 'tab-button--active';

        return pressed ? 'tab-button--pressed' : hovered ? 'tab-button--hovered' : '';
    }, [isActiveTab, hovered, pressed]);

    useEffect(() => {
        let content: JSX.Element = <span>{name}</span>;
        if (icon) {
            const showHoveredIcon = !isActiveTab && hovered && hoveredIcon;
            content = (
                <div className="icon--container">
                    <img src={showHoveredIcon ? hoveredIcon : icon} />{' '}
                </div>
            );
        }
        setTabContent(content);
    }, [icon, name, hovered, isActiveTab]);

    return (
        <button
            className={classes('tab-button', getSpecialClassName())}
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
            {tabContent}
        </button>
    );
};

export default TabSelector;
