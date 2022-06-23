import React from 'react';

import { StatusIndicator } from './StatusIndicator';
import { ScreenManager } from './ScreenManager';
import { TabSelector } from './Controls/TabSelector';

import logo from '../Images/Logo.png';
import backArrow from '../Images/Back_Arrow.png';

import '../Styles/ControlBar.css';

export class ControlBar extends React.Component<{
    manager: ScreenManager;
    atTopLevel: boolean;
    status: number;
    activeTabName: string;
}> {
    render() {
        const topLeftItem = this.props.atTopLevel ? (
            <StatusIndicator status={this.props.status} />
        ) : (
            <button onClick={this.props.manager.goToTopLevel} className="tfButton">
                <img src={backArrow} alt="Arrow pointing back" className="arrowStyle" />
                <p className="textStyle">Back</p>
            </button>
        );

        return (
            <div className="overallContainerStyle">
                <div className="topBarStyle">
                    {topLeftItem}
                    <img src={logo} alt="Logo: TouchFree by UltraLeap" className="horizElement" />
                    <div className="emptyContainer" />
                </div>
                <div className="tabBarStyle">
                    <TabSelector name="Camera" manager={this.props.manager} activeTab={this.props.activeTabName} />
                    <TabSelector
                        name="Interactions"
                        manager={this.props.manager}
                        activeTab={this.props.activeTabName}
                    />
                </div>
            </div>
        );
    }
}
