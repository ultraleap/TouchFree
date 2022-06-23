import '../../Styles/Controls/TabSelector.css';

import React from 'react';
import { NavLink,useNavigate } from 'react-router-dom';

interface TabSelectorProps {
    name: string;
}

const TabSelector: React.FC<TabSelectorProps> = ({ name }) => {
    const navigate = useNavigate();
    const lowerCaseName = name.toLowerCase();
    return (
        <NavLink
            to={`/${lowerCaseName}`}
            className={({ isActive }) => (isActive ? 'tabButton tabButtonActive' : 'tabButton')}
            onPointerUp={() => navigate(`/${lowerCaseName}`)}
        >
            {name}
        </NavLink>
    );
};

export default TabSelector;
