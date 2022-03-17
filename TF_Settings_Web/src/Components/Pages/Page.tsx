import { Property } from "csstype";
import React, { CSSProperties } from "react";

export class Page extends React.Component<{name: string, color: Property.Color}> {
    private divStyle: CSSProperties = {
        flex: '1 1 auto',
        width: '100%',
        height: '800px',
        backgroundColor: this.props.color,
    };

    render () {
        return (
            <div style={this.divStyle}/>
        );
    }
}
