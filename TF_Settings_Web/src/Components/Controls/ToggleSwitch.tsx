import React, { ChangeEventHandler } from "react";

import '../../Styles/Controls/ToggleSwitch.css';

interface ToggleProps {
    name: string,
    onChange: ChangeEventHandler<HTMLInputElement>
}

export class ToggleSwitch extends React.Component<ToggleProps, {}> {
    render() {
        return(
            <label className="backgroundLabel">
                <p className="switchLabel">{this.props.name}</p>
                <label className="switch">
                    <input type="checkbox"/>
                    <span className="toggle round"></span>
                </label>
            </label>
        );
    }
}