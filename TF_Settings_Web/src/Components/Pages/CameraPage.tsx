import { CSSProperties } from "react";

import { Page } from "./Page";

export class CameraPage extends Page {
    private divStyle: CSSProperties = {
        flex: '1 1 auto',
        width: '100%',
        height: '800px',
        backgroundColor: 'red',
    };

    render() {
        return(
            <div style={this.divStyle}/>
        );
    }
}