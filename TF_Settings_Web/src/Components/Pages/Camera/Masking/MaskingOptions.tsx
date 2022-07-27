import 'Styles/Camera/CameraMasking.scss';

import React from 'react';

import { ToggleSwitch } from 'Components/Controls/ToggleSwitch';

interface MaskingOptionProps {
    title: string;
    description: string;
    value: boolean;
    onChange: (value: boolean) => void;
}

const MaskingOption: React.FC<MaskingOptionProps> = ({ title, description, value, onChange }) => (
    <label
        className="cam-feeds-option"
        onPointerDown={() => {
            onChange(!value);
        }}
    >
        <div className="cam-feeds-option-text">
            <h1>{title}</h1>
            <p>{description}</p>
        </div>
        <div className="cam-feeds-option-toggle">
            {/* eslint-disable-next-line @typescript-eslint/no-empty-function */}
            <ToggleSwitch value={value} onChange={() => {}} />
        </div>
    </label>
);

export default MaskingOption;
