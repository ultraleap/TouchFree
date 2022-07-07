import 'Styles/Controls/RadioButtons.css';

import React from 'react';

interface RadioProps {
    name: string;
    options: string[];
    selected: number;
    onChange: (newValue: string) => void;
}

export class RadioGroup extends React.Component<RadioProps, {}> {
    private onChange() {
        // this function is here purely to pass to the input, preventing it becoming ReadOnly
    }

    render() {
        return (
            <div className="verticalContainer halfWidth">
                {this.props.options.map((option, index) => (
                    <label
                        key={index}
                        className="backgroundLabel"
                        onPointerDown={() => {
                            this.props.onChange(option);
                        }}
                    >
                        <input
                            type="radio"
                            name={this.props.name}
                            value={option}
                            onChange={this.onChange}
                            checked={this.props.selected === index}
                        />
                        <span className="checkmark" />
                        <p> {option} </p>
                    </label>
                ))}
            </div>
        );
    }
}
