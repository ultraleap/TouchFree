import 'Styles/Controls/ToggleSwitch.scss';

import React from 'react';

interface ToggleSwitchProps {
    value: boolean;
}

export const ToggleSwitch: React.FC<ToggleSwitchProps> = ({ value }) => (
    <>
        {/* eslint-disable-next-line @typescript-eslint/no-empty-function */}
        <input type="checkbox" style={{ display: 'none' }} checked={value} onChange={() => {}} />
        <div className="switch" />
    </>
);

interface LabelledToggleSwitchProps extends ToggleSwitchProps {
    name: string;
    onChange: (newState: boolean) => void;
}

export const LabelledToggleSwitch: React.FC<LabelledToggleSwitchProps> = ({ name, value, onChange }) => (
    <label className="input-label-container" onPointerDown={() => onChange(!value)}>
        <p className="switch-label">{name}</p>
        <span className="switch-container">
            <ToggleSwitch value={value} />
        </span>
    </label>
);
