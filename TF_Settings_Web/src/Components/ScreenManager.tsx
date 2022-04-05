// Provides an "ActiveScreen" / "ActiveTab" component that then contains the correct screen
// Manages the loading/display of the subscreens
// Is the place where the active screen/tab is controlled

import React, { Component, CSSProperties, RefObject } from "react";
import { ControlBar } from "./ControlBar";
import { CursorManager } from "./CursorManager";
import { CameraPage } from "./Pages/CameraPage";
import { InteractionsPage } from "./Pages/InteractionsPage";

const TouchFree = window.TouchFree;

interface ScreenManagerState {
    atTopLevel: boolean,
    tfState: number,
    activeTabName: string,
}

const pages: {[name: string]: typeof Component} = {
    "Camera": CameraPage,
    "Interactions": InteractionsPage,
};

export class ScreenManager extends React.Component<{}, ScreenManagerState> {
    private containerStyle : CSSProperties = {
        display: 'flex',
        flexDirection: 'column',
        height: '100%'
    }

    private timerID: number;

    private cursorManager: CursorManager;
    private cursorParent: RefObject<HTMLDivElement>;

    // TouchFree objects
    private inputSystem: any;
    private connectionManager: any;

    constructor(props: {}) {
        super(props);

        this.timerID = -1;

        TouchFree.Connection.ConnectionManager.init();
        this.inputSystem = new TouchFree.InputControllers.WebInputController();
        this.connectionManager = TouchFree.Connection.ConnectionManager;

        this.cursorManager = new CursorManager();
        this.cursorParent = React.createRef();

        let state = {
            atTopLevel: true,
            tfState: 0,
            activeTabName: "Camera",
        };

        this.state = state;
    }

    componentDidMount() {
        if (this.cursorParent.current !== null)
        {
            this.cursorManager.setElement(this.cursorParent.current);
        }

        this.timerID = window.setInterval(() => {
            this.RequestStatus()
        }, 5000);
    }

    componentWillUnmount() {
        clearInterval(this.timerID);
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

    private RequestStatus(): void {
        this.connectionManager.RequestServiceStatus(this.UpdateStatus.bind(this));
    }

    private UpdateStatus(detail: any) {
        this.setState(() => ({
            tfState: detail.trackingServiceState
        }));
    }

    render () {
        let ThisPage = pages[this.state.activeTabName];

        return (
            <div style={this.containerStyle}
                 ref={this.cursorParent}>
                <ControlBar manager={this}
                            atTopLevel={this.state.atTopLevel}
                            status={this.state.tfState}
                            activeTabName={this.state.activeTabName}/>
                <ThisPage />
            </div>
        );
    }
}