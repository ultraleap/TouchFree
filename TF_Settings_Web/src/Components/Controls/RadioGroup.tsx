import React, { ChangeEventHandler } from "react";

import '../../Styles/Controls/RadioButtons.css';

interface RadioProps {
    name: string,
    options: string[],
    selected: number,
    onChange: ChangeEventHandler<HTMLInputElement>
}

export class RadioGroup extends React.Component<RadioProps, {}> {
    render() {
        return(
            <div className="verticalContainer halfWidth">
                {this.props.options.map((option, index) => (
                    <label key={index} className="backgroundLabel">
                        <input
                            type="radio"
                            name={this.props.name}
                            value={option}
                            onChange={this.props.onChange}
                            checked={this.props.selected === index}/>
                        <span className="checkmark"/>
                        <p> {option} </p>
                    </label>
                ))}
            </div>
        );
    }
}
