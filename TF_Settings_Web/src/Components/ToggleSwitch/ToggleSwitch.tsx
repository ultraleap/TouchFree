import styles from './ToggleSwitch.module.scss';
import interactionStyles from '@/Pages/Interactions/Interactions.module.scss';

import classnames from 'classnames/bind';
import React from 'react';

const classes = classnames.bind(styles);
const interactionClasses = classnames.bind(interactionStyles);

interface ToggleSwitchProps {
    value: boolean;
}

// ToggleSwitch doesn't react to onChange, it only displays the current value.
// Changing the value should be handle by the <label> parent of the ToggleSwitch.
export const ToggleSwitch: React.FC<ToggleSwitchProps> = ({ value }) => (
    <>
        <input type="checkbox" style={{ display: 'none' }} checked={value} onChange={() => {}} />
        <div className={classes('switch')} />
    </>
);

interface LabelledToggleSwitchProps extends ToggleSwitchProps {
    name: string;
    onChange: (newState: boolean) => void;
}

export const LabelledToggleSwitch: React.FC<LabelledToggleSwitchProps> = ({ name, value, onChange }) => (
    <label className={interactionClasses('input-label-container')} onPointerDown={() => onChange(!value)}>
        <p className={classes('switch-label')}>{name}</p>
        <span className={classes('switch-container')}>
            <ToggleSwitch value={value} />
        </span>
    </label>
);
