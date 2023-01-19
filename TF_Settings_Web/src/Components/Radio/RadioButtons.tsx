import classnames from 'classnames/bind';

import classes from './RadioButtons.module.scss';
import sliderClasses from '@/Components/Slider/Sliders.module.scss';
import interactionsStyles from '@/Pages/Interactions/Interactions.module.scss';

import React from 'react';

const interactionsClasses = classnames.bind(interactionsStyles);

interface RadioProps {
    name: string;
    options: string[];
    selected: number;
    onChange: (newValue: string) => void;
}

export const RadioLine: React.FC<RadioProps> = ({ name, options, selected, onChange }) => (
    <label className={interactionsClasses('horizontalContainer', 'input-label-container')}>
        <p className={sliderClasses['sliderLabel']}> {name} </p>
        {options.map((option, index) => (
            <div
                className={classes['horizontalRadioContainer']}
                key={option}
                onPointerDown={() => {
                    onChange(option);
                }}
            >
                <input
                    className={classes['radio-button']}
                    type="radio"
                    key={option}
                    name={name}
                    value={option}
                    onChange={(event) => onChange(event.currentTarget.value)}
                    checked={selected === index}
                />
                <span className="checkmark" />
                <p className={sliderClasses['sliderLabel']}> {option} </p>
            </div>
        ))}
    </label>
);

export const RadioGroup: React.FC<RadioProps> = ({ name, options, selected, onChange }) => (
    <div className={interactionsClasses('verticalContainer')}>
        {options.map((option, index) => (
            <label
                key={index}
                className={interactionsClasses('input-label-container')}
                onPointerDown={() => {
                    onChange(option);
                }}
            >
                <input
                    className={classes['radio-button']}
                    type="radio"
                    name={name}
                    value={option}
                    onChange={(event) => onChange(event.currentTarget.value)}
                    checked={selected === index}
                />
                <span className="checkmark" />
                <p> {option} </p>
            </label>
        ))}
    </div>
);
