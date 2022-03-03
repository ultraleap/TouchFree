import React from "react";
import { ScreenManager } from "../ScreenManager";

// Using this style from the stylesheet as the :hover addon doesn't
// seem to be supported by the CSSProperties type
import '../../Styles/Styles.css'

export class TabSelector extends React.Component<{name: string, manager: ScreenManager, activeTab: String}> {

    onClick(): void {
        this.props.manager.setScreenByName.bind(this.props.manager)(this.props.name);
    }

    render () {
        let className = "tabButton";

        if (this.props.name === this.props.activeTab) {
            className += " tabButtonActive";
        }

        return (
            <button className={className}
                    onClick={this.onClick.bind(this)}
                    onPointerUp={this.onClick.bind(this)}>
                {this.props.name}
            </button>
        );
    }
}
