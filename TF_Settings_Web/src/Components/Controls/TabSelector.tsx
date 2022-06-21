import React from "react";
import { ScreenManager } from "../ScreenManager";

export class TabSelector extends React.Component<{
    name: string;
    manager: ScreenManager;
    activeTab: string;
}> {
    onClick(): void {
        this.props.manager.setScreenByName.bind(this.props.manager)(
            this.props.name,
        );
    }

    render() {
        let className = "tabButton";

        if (this.props.name === this.props.activeTab) {
            className += " tabButtonActive";
        }

        return (
            <button
                className={className}
                onClick={this.onClick.bind(this)}
                onPointerUp={this.onClick.bind(this)}
            >
                {this.props.name}
            </button>
        );
    }
}
