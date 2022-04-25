import React from "react";

interface RadioProps {
    name: string,
    options: string[],
    selected: number,
    onChange: (newValue: string) => void,
}

export class RadioLine extends React.Component<RadioProps, {}> {
    private onChange() {
        // this function is here purely to pass to the input, preventing it becoming ReadOnly
    }

    render() {
        return(
            <label className="horizontalContainer backgroundLabel">
                <p className="sliderLabel"> {this.props.name} </p>
                {this.props.options.map((option, index) => (
                    <div className="horizontalRadioContainer"
                        key={option}
                        onPointerDown={() => {
                            this.props.onChange(option);
                        }}>
                        <input
                            type="radio"
                            key={option}
                            name={this.props.name}
                            value={option}
                            onChange={this.onChange}
                            checked={this.props.selected === index}/>
                        <span className="checkmark"/>
                        <p className="sliderLabel"> {option} </p>
                    </div>
                ))}
            </label>
        );
    }
}