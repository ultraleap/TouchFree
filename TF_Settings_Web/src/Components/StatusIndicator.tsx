// Shows
    // one image if everything is fine
    // a different one if camera isn't connected
    // one if service isn't connected
    // All have tooltips

import React, { CSSProperties } from "react";
import { tfStatus } from "./ScreenManager";

export class StatusIndicator extends React.Component<{status: tfStatus}> {
    private indicatorStyle: CSSProperties = {
        display: 'flex 1 1 auto',
    };

    render () {
        let message = "Connected";

        switch (this.props.status) {
            case tfStatus.NO_CAMERA:
                message = "No Camera found connected!";
                break;
            case tfStatus.NO_SERVICE:
                message = "TouchFree Service not found";
                break;
        }

        return (
            <div style={this.indicatorStyle}>
                {message}
            </div>
        );
    }
}
