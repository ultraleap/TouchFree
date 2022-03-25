import React, { ChangeEventHandler, MouseEventHandler, PointerEventHandler } from "react";

import '../../Styles/Controls/TextEntry.css';

interface TextEntryProps {
    name: string,
    value: number,
    onChange: (ChangeEventHandler<HTMLInputElement>),
    onClick: (MouseEventHandler<HTMLElement>),
    onPointerDown: (PointerEventHandler<HTMLElement>)
}

export class TextEntry extends React.Component<TextEntryProps, {}> {
    render() {
        return(
            <label onClick={this.props.onClick.bind(this)} onPointerDown={this.props.onPointerDown.bind(this)} className="backgroundLabel">
                <p className="textEntryLabel">{this.props.name}</p>
                <label className="textEntryContainer">
                    <input type="number"
                           step={0.01}
                           className="textEntryText"
                           value={this.props.value}
                           onChange={this.props.onChange.bind(this)}/>
                </label>
            </label>
        );
    }
}