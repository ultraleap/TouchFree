// Shows
    // one image if everything is fine
    // a different one if camera isn't connected
    // one if service isn't connected
    // All have tooltips

import React, { CSSProperties } from "react";

import camStatusIcon from '../Images/Camera_Status_Icon.png';
import svcStatusIcon from '../Images/Tracking_Status_Icon.png';

const TouchFree = window.TouchFree;

const trackingState = TouchFree.TouchFreeToolingTypes.TrackingServiceState;

export class StatusIndicator extends React.Component<{status: number}> {
    private indicatorStyle: CSSProperties = {
        alignSelf: 'center',
        height: "100%",
        minWidth: '6rem',
        maxWidth: '30rem',
        display: "flex",
        flexDirection: "row",
        justifyContent: 'space-between',
        paddingLeft: "2rem",
    };

    private statusIconStyle: CSSProperties = {
        alignSelf: 'center',
        minHeight: '0',
        minWidth: '0',
        maxHeight: '5rem',
        maxWidth: '30rem',
        position: "relative",
    };

    private statusDotStyleOK: CSSProperties = {
        position: "absolute",
        right: "-15%",
        top: "-.5rem",
        height: "1rem",
        width: "1rem",
        borderRadius: ".5rem",
        backgroundColor: "green"
    }

    private statusDotStyleBad: CSSProperties = {
        position: "absolute",
        right: "-15%",
        top: "-.5rem",
        height: "1rem",
        width: "1rem",
        borderRadius: ".5rem",
        backgroundColor: "red"
    }

    render () {
        console.log(`status was: ${trackingState[this.props.status]}`)

        return (
            <div style={this.indicatorStyle}>
                <div style={this.statusIconStyle}>
                    <img src={camStatusIcon} alt="Camera Status Icon" style={this.statusIconStyle}/>
                    <div style={(this.props.status === trackingState.CONNECTED) ? this.statusDotStyleOK : this.statusDotStyleBad}/>
                </div>
                <div style={this.statusIconStyle}>
                    <img src={svcStatusIcon} alt="Tracking Service Status Icon" style={this.statusIconStyle}/>
                    <div style={(this.props.status === trackingState.UNAVAILABLE) ? this.statusDotStyleBad : this.statusDotStyleOK}/>
                </div>
            </div>
        );
    }
}
