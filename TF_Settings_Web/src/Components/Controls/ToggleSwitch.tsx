import 'Styles/Controls/ToggleSwitch.scss';

import React from 'react';

interface ToggleSwitchProps {
    value: boolean;
    onChange: (newState: boolean) => void;
}

export const ToggleSwitch: React.FC<ToggleSwitchProps> = ({ value, onChange }) => (
    <>
        <input
            type="checkbox"
            style={{ display: 'none' }}
            checked={value}
            onChange={() => {
                onChange(!value);
            }}
        />
        <div className="switch" />
    </>
);

interface LabelledToggleSwitchProps extends ToggleSwitchProps {
    name: string;
}

export const LabelledToggleSwitch: React.FC<LabelledToggleSwitchProps> = ({ name, value, onChange }) => (
    <label className="input-label-container">
        <p className="switch-label">{name}</p>
        <span className="switch-container">
            <ToggleSwitch value={value} onChange={onChange} />
        </span>
    </label>
);
