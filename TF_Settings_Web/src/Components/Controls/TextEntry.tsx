import React, { ChangeEventHandler, MouseEventHandler, PointerEventHandler } from "react";

import '../../Styles/Controls/TextEntry.css';

interface TextEntryProps {
    name: string,
    value: number,
    selected: boolean,
    onChange: (ChangeEventHandler<HTMLInputElement>),
    onClick: (MouseEventHandler<HTMLElement>),
    onPointerDown: (PointerEventHandler<HTMLElement>)
}

export class TextEntry extends React.Component<TextEntryProps, {}> {
    getClassName(): string {
        return "textEntryBackgroundLabel " + (this.props.selected ? "textEntryBackgroundLabelSelected" : "");
    }

    render() {
        return(
            <label onClick={this.props.onClick.bind(this)}
                    onPointerDown={this.props.onPointerDown.bind(this)}
                    className={this.getClassName()}>
                <p className="textEntryLabel">{this.props.name}</p>
                <label className="textEntryContainer">
                    <input type="number"
                           className="textEntryText"
                           value={this.props.value}
                           onChange={this.props.onChange.bind(this)}/>
                </label>
            </label>
        );
    }
}