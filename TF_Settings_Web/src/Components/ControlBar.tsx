import React from "react";
import { CSSProperties } from "react";

import { StatusIndicator } from './StatusIndicator';
import { ScreenManager } from "./ScreenManager";
import { TabSelector } from "./Controls/TabSelector";

import logo from '../Images/Logo.png';
import backArrow from "../Images/Back_Arrow.png";

// Using this style from the stylesheet as the :hover addon doesn't
// seem to be supported by the CSSProperties type
import '../Styles/Styles.css'

export class ControlBar extends React.Component<{ manager: ScreenManager,
                                                  atTopLevel: Boolean,
                                                  status: number,
                                                  activeTabName: String
                                                }> {

    private topBarStyle: CSSProperties = {
        display: 'flex',
        alignItems: 'flex-start',
        flexDirection: 'row',
        justifyContent: 'space-between',
        width: '100%',
        height: '5rem',
        minWidth: '0',
        minHeight: '0',
    };

    private tabBarStyle: CSSProperties = {
        display: 'flex',
        alignItems: 'end',
        flexDirection: 'row',
        justifyContent: 'space-between',
        width: '100%',
        height: '4rem',
        minWidth: '0',
        minHeight: '0',
    };

    private overallContainerStyle: CSSProperties = {
        backgroundImage: 'linear-gradient(180deg, #353535, #2D2D2D)'
    }

    private horizElement: CSSProperties = {
        flex: '1 2 auto',
        alignSelf: 'center',
        minHeight: '0',
        minWidth: '0',
        maxHeight: '4rem',
        maxWidth: '26.1rem'
    };

    private emptyContainer: CSSProperties = {
        width: '125px',
        height: '100%',
    };

    private arrowStyle: CSSProperties = {
        display: "inline-block",
        margin: "0",
        marginRight: "0.3rem",
        verticalAlign: "middle",
    }

    private textStyle: CSSProperties = {
        display: "inline-block",
        margin: "0",
        verticalAlign: "middle",
        fontFamily: "NowRegular, Arial"
    }

    render() {
        let topLeftItem;

        if (this.props.atTopLevel) {
            topLeftItem = <StatusIndicator status={this.props.status}/>;
        }
        else {
            topLeftItem =
                <button
                    onClick={this.props.manager.goToTopLevel}
                    className="tfButton"
                    >
                    <img src={backArrow} alt="Arrow pointing back" style={this.arrowStyle} />
                    <p style={this.textStyle}>
                        Back
                    </p>
                </button>;
        }

        return (
            <div style={this.overallContainerStyle}>
                <div style={this.topBarStyle}>
                    {topLeftItem}
                    <img src={logo} alt="Logo: TouchFree by UltraLeap" style={this.horizElement}/>
                    <div style={this.emptyContainer}/>
                </div>
                <div style={this.tabBarStyle}>
                    <TabSelector name="Camera" manager={this.props.manager} activeTab={this.props.activeTabName}/>
                    <TabSelector name="Interactions" manager={this.props.manager} activeTab={this.props.activeTabName}/>
                </div>
            </div>
        );
    }
}