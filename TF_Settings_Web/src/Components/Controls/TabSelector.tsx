import '../../Styles/Controls/TabSelector.css';

import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom';

interface TabSelectorProps {
    name: string;
}

const TabSelector: React.FC<TabSelectorProps> = ({ name }) => {
    const [hovered, setHovered] = React.useState<boolean>(false);

    const navigate = useNavigate();
    const lowerCaseName = name.toLowerCase();
    return (
        <NavLink
            to={`/${lowerCaseName}`}
            className={({ isActive }) =>
                isActive ? 'tabButton tabButtonActive' : hovered ? 'tabButton tabButtonHovered' : 'tabButton'
            }
            onPointerUp={() => navigate(`/${lowerCaseName}`)}
            onPointerOver={() => setHovered(true)}
            onPointerLeave={() => {
                setHovered(false);
            }}
        >
            {name}
        </NavLink>
    );
};

export default TabSelector;
