// Provides an "ActiveScreen" / "ActiveTab" component that then contains the correct screen
// Manages the loading/display of the subscreens
// Is the place where the active screen/tab is controlled

import React, { CSSProperties } from 'react';

import { ConnectionManager } from '../TouchFree/Connection/ConnectionManager';
import { ServiceStatus } from '../TouchFree/Connection/TouchFreeServiceTypes';

import { ControlBar } from './ControlBar';
import { InteractionsPage } from './Pages/InteractionsPage';
import { CameraPage } from './Pages/Camera/CameraPage';
import { Page } from './Pages/Page';

export type ScreenName = 'Camera' | 'Interactions';

interface ScreenManagerState {
    atTopLevel: boolean;
    tfState: number;
    activeTabName: ScreenName;
}

const pages: Record<ScreenName, typeof Page> = {
    Camera: CameraPage,
    Interactions: InteractionsPage,
};

export class ScreenManager extends React.Component<{}, ScreenManagerState> {
    private containerStyle: CSSProperties = {
        display: 'flex',
        flexDirection: 'column',
        height: '100%',
    };

    private timerID: number;

    constructor(props: {}) {
        super(props);

        this.timerID = -1;

        const state = {
            atTopLevel: true,
            tfState: 0,
            activeTabName: 'Camera' as ScreenName,
        };

        this.state = state;
    }

    componentDidMount() {
        this.timerID = window.setInterval(() => {
            this.RequestStatus();
        }, 5000);
    }

    componentWillUnmount() {
        clearInterval(this.timerID);
    }

    public goToTopLevel(): void {
        // ToDo: Go to parent state
    }

    public setScreenByName(screenName: ScreenName): void {
        // Also check if the target is top level and set state.atTopLevel
        this.setState(() => ({ activeTabName: screenName }));
    }

    private RequestStatus(): void {
        ConnectionManager.RequestServiceStatus(this.UpdateStatus.bind(this));
    }

    private UpdateStatus(detail: ServiceStatus) {
        const status = detail.trackingServiceState;
        if (status) {
            this.setState(() => ({
                tfState: status,
            }));
        }
    }

    render() {
        const ThisPage = pages[this.state.activeTabName];

        return (
            <div style={this.containerStyle}>
                <ControlBar
                    manager={this}
                    atTopLevel={this.state.atTopLevel}
                    status={this.state.tfState}
                    activeTabName={this.state.activeTabName}
                />
                <ThisPage />
            </div>
        );
    }
}
