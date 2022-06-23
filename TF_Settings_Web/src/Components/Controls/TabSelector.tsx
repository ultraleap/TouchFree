import React from 'react';
import { ScreenManager, ScreenName } from '../ScreenManager';

export class TabSelector extends React.Component<{
    name: ScreenName;
    manager: ScreenManager;
    activeTab: ScreenName;
}> {
    onClick(): void {
        this.props.manager.setScreenByName(this.props.name);
    }

    render() {
        let className = 'tabButton';

        if (this.props.name === this.props.activeTab) {
            className += ' tabButtonActive';
        }

        return (
            <button className={className} onClick={this.onClick.bind(this)} onPointerUp={this.onClick.bind(this)}>
                {this.props.name}
            </button>
        );
    }
}
