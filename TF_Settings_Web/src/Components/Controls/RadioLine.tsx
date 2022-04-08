import React, { ChangeEventHandler } from "react";

interface RadioProps {
    name: string,
    options: string[],
    selected: number,
    onChange: ChangeEventHandler<HTMLInputElement>
}

export class RadioLine extends React.Component<RadioProps, {}> {
    render() {
        return(
            <label className="horizontalContainer backgroundLabel">
                <p className="sliderLabel"> {this.props.name} </p>
                {this.props.options.map((option, index) => (
                    <div className="horizontalRadioContainer"
                        key={option}
                        >
                        <input
                            type="radio"
                            key={option}
                            name={this.props.name}
                            value={option}
                            onChange={this.props.onChange}
                            checked={this.props.selected === index}/>
                        <span className="checkmark"/>
                        <p className="sliderLabel"> {option} </p>
                    </div>
                ))}
            </label>
        );
    }
}