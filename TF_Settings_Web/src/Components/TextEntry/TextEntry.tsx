import styles from './TextEntry.module.scss';

import classnames from 'classnames/bind';
import React, { ChangeEventHandler, MouseEventHandler, FocusEventHandler, PointerEventHandler } from 'react';

const classes = classnames.bind(styles);

interface TextEntryProps {
    name: string;
    value: string;
    selected: boolean;
    onChange: ChangeEventHandler<HTMLInputElement>;
    onPointerDown: PointerEventHandler<HTMLElement>;
    onClick?: MouseEventHandler<HTMLElement>;
    onBlur: FocusEventHandler<HTMLElement>;
}

const TextEntry: React.FC<TextEntryProps> = ({ name, value, selected, onChange, onClick, onPointerDown, onBlur }) => {
    return (
        <label
            onClick={onClick}
            onPointerDown={onPointerDown}
            className={classes('background-label', { 'background-label--selected': selected })}
            onBlur={onBlur}
        >
            <p className={classes('label')}>{name}</p>
            <label className={classes('container')}>
                <input className={classes('text')} value={value} onChange={onChange} />
            </label>
        </label>
    );
};

export default TextEntry;
