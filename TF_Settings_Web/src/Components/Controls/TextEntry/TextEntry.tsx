import './TextEntry.scss';

import React, { ChangeEventHandler, MouseEventHandler, PointerEventHandler } from 'react';

interface TextEntryProps {
    name: string;
    value: string;
    selected: boolean;
    onChange: ChangeEventHandler<HTMLInputElement>;
    onClick: MouseEventHandler<HTMLElement>;
    onPointerDown: PointerEventHandler<HTMLElement>;
}

export class TextEntry extends React.Component<TextEntryProps, {}> {
    getClassName(): string {
        return 'textEntryBackgroundLabel ' + (this.props.selected ? 'textEntryBackgroundLabelSelected' : '');
    }

    render() {
        return (
            <label
                onClick={this.props.onClick}
                onPointerDown={this.props.onPointerDown}
                className={this.getClassName()}
            >
                <p className="textEntryLabel">{this.props.name}</p>
                <label className="textEntryContainer">
                    <input className="textEntryText" value={this.props.value} onChange={this.props.onChange} />
                </label>
            </label>
        );
    }
}
