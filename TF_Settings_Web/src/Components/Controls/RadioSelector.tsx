import React, { ChangeEventHandler } from "react";

interface RadioProps {
    name: string,
    options: string[],
    selected: string,
    onChange: ChangeEventHandler<HTMLInputElement>
}

export class RadioSelector extends React.Component<RadioProps, {}> {
    // To get the value:
    // document.querySelector('input[name = this.props.name]:checked').value;

    render() {
        return(
            <div className="verticalContainer">
                {this.props.options.map((option, index) =>
                (
                    <label key={index} className="container">
                        <input
                            type="radio"
                            name={this.props.name}
                            value={option}
                            onChange={this.props.onChange}
                            checked={this.props.selected === option}/>
                        <span className="checkmark"/>
                        <p> {option} </p>
                    </label>
                ))}
            </div>
        );
    }
}