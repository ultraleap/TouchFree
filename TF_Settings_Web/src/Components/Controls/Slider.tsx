import React, { PointerEvent, RefObject } from 'react';

import '../../Styles/Controls/Sliders.css';

interface SliderProps {
    name: string;
    rangeMin: number;
    rangeMax: number;
    leftLabel: string;
    rightLabel: string;
    value: number;
    onChange: (newValue: number) => void;
    increment?: number;
}

export class Slider extends React.Component<SliderProps, {}> {
    public static defaultProps = {
        increment: 0.1,
    };

    private dragging = false;

    private inputElement: RefObject<HTMLInputElement>;

    constructor(props: SliderProps) {
        super(props);

        this.inputElement = React.createRef();

        document.body.addEventListener('pointerup', this.onUpCancel.bind(this));
        document.body.addEventListener(
            'pointercancel',
            this.onUpCancel.bind(this)
        );
    }

    private onChange() {
        // this function is here purely to pass to the input, preventing it becoming ReadOnly
    }

    private onUpCancel() {
        this.dragging = false;
    }

    private onDown(event: PointerEvent<HTMLInputElement>) {
        this.dragging = true;
        this.setValueByPos(event.nativeEvent.offsetX);
    }

    private onMove(event: PointerEvent<HTMLInputElement>) {
        if (this.dragging) {
            this.setValueByPos(event.nativeEvent.offsetX);
        }
    }

    private setValueByPos(xPos: number) {
        if (this.inputElement.current !== null) {
            // Slider height is currently 0.75rem
            const remValue = this.inputElement.current.clientHeight;

            // Slider control is 1.5rem wide, so half is 1x remValue, full is 2x remValue
            const posInRange: number =
                (xPos - remValue) /
                (this.inputElement.current.clientWidth - 2 * remValue);
            const outputValue: number = this.lerp(
                this.props.rangeMin,
                this.props.rangeMax,
                posInRange
            );

            if (
                this.props.rangeMin < outputValue &&
                outputValue < this.props.rangeMax
            ) {
                this.props.onChange(outputValue);
            }
        }
    }

    private lerp(v0: number, v1: number, t: number): number {
        return v0 * (1 - t) + v1 * t;
    }

    render() {
        return (
            <label className="backgroundLabel">
                <p className="sliderLabel">{this.props.name}</p>
                <div className="sliderContainer">
                    <input
                        type="range"
                        step={this.props.increment}
                        min={this.props.rangeMin}
                        max={this.props.rangeMax}
                        className="slider"
                        onChange={this.onChange}
                        onPointerMove={this.onMove.bind(this)}
                        onPointerDown={this.onDown.bind(this)}
                        onPointerUp={this.onUpCancel.bind(this)}
                        onPointerCancel={this.onUpCancel.bind(this)}
                        value={this.props.value}
                        id="myRange"
                        ref={this.inputElement}
                    />
                    <div className="sliderLabelContainer">
                        <label className="leftSliderLabel">
                            {this.props.leftLabel}
                        </label>
                        <label className="rightSliderLabel">
                            {this.props.rightLabel}
                        </label>
                    </div>
                </div>
            </label>
        );
    }
}
