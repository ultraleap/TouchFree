// Shows
// one image if everything is fine
// a different one if camera isn't connected
// one if service isn't connected
// All have tooltips

import React, { CSSProperties } from 'react';
import { TrackingServiceState } from '../TouchFree/TouchFreeToolingTypes';

import camStatusIcon from '../Images/Camera_Status_Icon.png';
import svcStatusIcon from '../Images/Tracking_Status_Icon.png';

export class StatusIndicator extends React.Component<{
    status: TrackingServiceState;
}> {
    private indicatorStyle: CSSProperties = {
        alignSelf: 'center',
        height: '100%',
        minWidth: '6rem',
        maxWidth: '30rem',
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'space-between',
        paddingLeft: '2rem',
    };

    private statusContainerStyle: CSSProperties = {
        alignSelf: 'stretch',
        minHeight: '0',
        minWidth: '0',
        maxHeight: '5rem',
        maxWidth: '30rem',
        position: 'relative',
        display: 'flex',
    };

    private statusIconStyle: CSSProperties = {
        alignSelf: 'center',
        minHeight: '0',
        minWidth: '0',
        maxHeight: '5rem',
        maxWidth: '30rem',
        position: 'relative',
    };

    private statusDotStyleOK: CSSProperties = {
        position: 'absolute',
        right: '-15%',
        top: '1.5rem',
        height: '1rem',
        width: '1rem',
        borderRadius: '.5rem',
        backgroundImage: 'linear-gradient(180deg, #00EB86, #00CDCF)',
    };

    private statusDotStyleBad: CSSProperties = {
        position: 'absolute',
        right: '-15%',
        top: '1.5rem',
        height: '1rem',
        width: '1rem',
        borderRadius: '.5rem',
        // #E2164D (top) to #D11883 (bottom)
        backgroundImage: 'linear-gradient(180deg, #E2164D, #D11883)',
    };

    render() {
        return (
            <div style={this.indicatorStyle}>
                <div style={this.statusContainerStyle}>
                    <img src={camStatusIcon} alt="Camera Status Icon" style={this.statusIconStyle} />
                    <div
                        style={
                            this.props.status === TrackingServiceState.CONNECTED
                                ? this.statusDotStyleOK
                                : this.statusDotStyleBad
                        }
                    />
                </div>
                <div style={this.statusContainerStyle}>
                    <img src={svcStatusIcon} alt="Tracking Service Status Icon" style={this.statusIconStyle} />
                    <div
                        style={
                            this.props.status === TrackingServiceState.UNAVAILABLE
                                ? this.statusDotStyleBad
                                : this.statusDotStyleOK
                        }
                    />
                </div>
            </div>
        );
    }
}
