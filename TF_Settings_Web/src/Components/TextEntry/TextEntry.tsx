import classNames from 'classnames/bind';

import styles from './TextEntry.module.scss';

import React, { ChangeEventHandler, MouseEventHandler, PointerEventHandler } from 'react';

const classes = classNames.bind(styles);

interface TextEntryProps {
    name: string;
    value: string;
    selected: boolean;
    onChange: ChangeEventHandler<HTMLInputElement>;
    onClick: MouseEventHandler<HTMLElement>;
    onPointerDown: PointerEventHandler<HTMLElement>;
}

const TextEntry: React.FC<TextEntryProps> = ({ name, value, selected, onChange, onClick, onPointerDown }) => {
    return (
        <label
            onClick={onClick}
            onPointerDown={onPointerDown}
            className={classes('background-label', { 'background-label--selected': selected })}
        >
            <p className={classes('label')}>{name}</p>
            <label className={classes('container')}>
                <input className={classes('text')} value={value} onChange={onChange} />
            </label>
        </label>
    );
};

export default TextEntry;
