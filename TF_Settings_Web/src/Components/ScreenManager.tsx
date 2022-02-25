// Provides an "ActiveScreen" / "ActiveTab" component that then contains the correct screen
// Manages the loading/display of the subscreens
// Is the place where the active screen/tab is controlled

import React, { Component, CSSProperties, ReactElement } from "react";
import { ControlBar } from "./ControlBar";
import { CameraPage } from "./Pages/CameraPage";
import { InteractionsPage } from "./Pages/InteractionsPage";
import { Page } from "./Pages/Page";

export enum tfStatus {
    CONNECTED,
    NO_CAMERA,
    NO_SERVICE
}

interface ScreenManagerProps {
    atTopLevel: boolean,
    status: tfStatus,
    activeTabName: string,
}

const pages: {[name: string]: typeof Component} = {
    "Camera": CameraPage,
    "Interactions": InteractionsPage,
};

export class ScreenManager extends React.Component<{}, ScreenManagerProps> {
    private containerStyle : CSSProperties = {
        display: 'flex',
        flexDirection: 'column',
        height: '100%'
    }

    constructor(props: {}) {
        super(props);

        let state = {
            atTopLevel: true,
            status: tfStatus.CONNECTED,
            activeTabName: "Camera",
        };

        this.state = state;
    }

    public goToTopLevel(): void {
        // ToDo: Go to parent state
    }

    public setScreenByName(screenName: string): void {
        // Later this should validate that the incoming name is in the list of known screens

        // Also check if the target is top level and set state.atTopLevel
        this.setState(() => ({
            activeTabName: screenName,
        }));

        this.forceUpdate();
    }

    render () {
        console.log("ActiveChild: " + this.state.activeTabName);

        let ThisPage = pages[this.state.activeTabName];

        return (
            <div style={this.containerStyle}>
                <ControlBar manager={this}
                            atTopLevel={this.state.atTopLevel}
                            status={this.state.status}
                            activeTabName={this.state.activeTabName}/>
                <ThisPage />
            </div>
        );
    }
}