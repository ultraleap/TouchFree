import 'Styles/Controls/TabSelector.css';

import React from 'react';
import { useNavigate } from 'react-router-dom';

interface TabSelectorProps {
    name: string;
    isActiveTab: boolean;
    onClick: () => void;
}

const TabSelector: React.FC<TabSelectorProps> = ({ name, isActiveTab, onClick }) => {
    const [hovered, setHovered] = React.useState<boolean>(false);

    const handleClick = () => {
        if (!isActiveTab) {
            navigate(`/settings/${lowerCaseName}`);
            onClick();
        }
    };

    const navigate = useNavigate();
    const lowerCaseName = name.toLowerCase();
    return (
        <button
            className={isActiveTab ? 'tabButton tabButtonActive' : hovered ? 'tabButton tabButtonHovered' : 'tabButton'}
            onPointerUp={handleClick}
            onKeyDown={(keyEvent) => {
                if (keyEvent.key === 'Enter') handleClick();
            }}
            onPointerOver={() => setHovered(true)}
            onPointerLeave={() => {
                setHovered(false);
            }}
        >
            {name}
        </button>
    );
};

export default TabSelector;
