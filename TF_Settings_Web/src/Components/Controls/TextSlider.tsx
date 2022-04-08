import React, { ChangeEventHandler } from "react";

import '../../Styles/Controls/ToggleSwitch.css';

interface TextSliderProps {
    name: string,
    rangeMin: number,
    rangeMax: number,
    leftLabel: string,
    rightLabel: string,
    value: number,
    onChange: ChangeEventHandler<HTMLInputElement>,
}

export class TextSlider extends React.Component<TextSliderProps, {}> {
    public static defaultProps = {
        increment: 0.1
    };

    render() {
        return(
            <label className="backgroundLabel">
                <p className="sliderLabel">{this.props.name}</p>
                <div className="sliderContainer">
                    <input type="range"
                        step={0.05}
                        min={this.props.rangeMin}
                        max={this.props.rangeMax}
                        value={this.props.value}
                        className="slider"
                        onChange={this.props.onChange}
                        id="myRange"/>
                    <div className="sliderLabelContainer">
                        <label className="leftSliderLabel">{this.props.leftLabel}</label>
                        <label className="rightSliderLabel">{this.props.rightLabel}</label>
                    </div>
                </div>
                <label className="sliderTextContainer">
                    <input type="number"
                           step={0.05}
                           className="sliderText"
                           onChange={this.props.onChange}
                           value={this.props.value} />
                </label>
            </label>
        );
    }
}