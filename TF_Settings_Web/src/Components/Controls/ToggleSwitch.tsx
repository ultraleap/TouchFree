import '../../Styles/Controls/ToggleSwitch.css';

import React from 'react';

interface ToggleProps {
    name: string;
    value: boolean;
    onChange: (newState: boolean) => void;
}

export class ToggleSwitch extends React.Component<ToggleProps, {}> {
    private onChange() {
        // this function is here purely to pass to the input, preventing it becoming ReadOnly
    }

    render() {
        return (
            <label
                className="backgroundLabel"
                onPointerDown={() => {
                    this.props.onChange(!this.props.value);
                }}
            >
                <p className="switchLabel">{this.props.name}</p>
                <label className="switch">
                    <input type="checkbox" checked={this.props.value} onChange={this.onChange} />
                    <span className="toggle round" />
                </label>
            </label>
        );
    }
}
