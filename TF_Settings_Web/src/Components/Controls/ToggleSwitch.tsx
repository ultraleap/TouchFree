import React, { ChangeEventHandler, RefObject } from "react";

import '../../Styles/Controls/ToggleSwitch.css';

interface ToggleProps {
    name: string,
    value: boolean,
    onChange: (newState: boolean) => void
}

export class ToggleSwitch extends React.Component<ToggleProps, {}> {
    private inputElem: RefObject<HTMLInputElement>;

    constructor(props: ToggleProps) {
        super(props);

        this.inputElem = React.createRef();
    }

    onChange() {
        console.log("GOT A CHANGE?");
    }

    render() {
        return (
            <label className="backgroundLabel"
                onPointerDown={() => {
                    // console.log(`Got an onPointerDown on the outer label`);
                    this.props.onChange.bind(this)(!this.props.value);
                }}
                htmlFor={`ToggleSwitch_for_${this.props.name}`}>
                <p className="switchLabel">{this.props.name}</p>
                <label className="switch"
                    onPointerDown={() => {
                        // console.log(`Got an onPointerDown on the label`);
                    }}>
                    <input type="checkbox"
                        checked={this.props.value}
                        id={`ToggleSwitch_for_${this.props.name}`}
                        ref={this.inputElem}
                        onChange={this.onChange.bind(this)}
                        // onChange={this.props.onChange.bind(this)}
                         />
                    <span className="toggle round"
                        onPointerDown={() => {
                            // console.log(`Got an onPointerDown on the span`);
                        }}
                        ></span>
                </label>
            </label>
        );
    }
}