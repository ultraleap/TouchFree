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

export class RadioLine extends React.Component<RadioProps, {}> {
    private onChange() {
        // this function is here purely to pass to the input, preventing it becoming ReadOnly
    }

    render() {
        return (
            <label className={interactionsClasses('horizontalContainer', 'input-label-container')}>
                <p className={sliderClasses['sliderLabel']}> {this.props.name} </p>
                {this.props.options.map((option, index) => (
                    <div
                        className={classes['horizontalRadioContainer']}
                        key={option}
                        onPointerDown={() => {
                            this.props.onChange(option);
                        }}
                    >
                        <input
                            type="radio"
                            key={option}
                            name={this.props.name}
                            value={option}
                            onChange={this.onChange}
                            checked={this.props.selected === index}
                        />
                        <span className="checkmark" />
                        <p className={sliderClasses['sliderLabel']}> {option} </p>
                    </div>
                ))}
            </label>
        );
    }
}
