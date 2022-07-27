import 'Styles/Controls/TabSelector.scss';

import React, { useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

interface TabSelectorProps {
    name: string;
    isActiveTab: boolean;
    onClick: () => void;
}

const TabSelector: React.FC<TabSelectorProps> = ({ name, isActiveTab, onClick }) => {
    const [hovered, setHovered] = React.useState<boolean>(false);
    const [pressed, setPressed] = React.useState<boolean>(false);

    const handleClick = () => {
        if (!isActiveTab) {
            navigate(`/settings/${lowerCaseName}`);
            onClick();
        }
    };

    const getSpecialClassName = useCallback((): string => {
        if (isActiveTab) return 'tab-button--active';

        return pressed ? 'tab-button--pressed' : hovered ? 'tab-button--hovered' : '';
    }, [isActiveTab, hovered, pressed]);

    const navigate = useNavigate();
    const lowerCaseName = name.toLowerCase();
    return (
        <button
            className={`tab-button ${getSpecialClassName()}`}
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
            {name}
        </button>
    );
};

export default TabSelector;
