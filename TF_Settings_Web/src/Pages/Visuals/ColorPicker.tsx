import './ColorPicker.scss';

import React, { useEffect, useState } from 'react';
import { HexAlphaColorPicker } from 'react-colorful';
import tinycolor from 'tinycolor2';

import { TabSelector } from '@/Components/Header';

import { CursorStyle } from './VisualsUtils';

interface ColorPickerProps {
    cursorStyle: CursorStyle;
    updateCursorStyle: (style: CursorStyle) => void;
    writeVisualsConfigIfNew: () => void;
}

const cursorSections = ['Center Fill', 'Outer Fill', 'Center Border'] as const;

const ColorPicker: React.FC<ColorPickerProps> = ({ cursorStyle, updateCursorStyle, writeVisualsConfigIfNew }) => {
    const [activeTabIndex, setActiveTabIndex] = useState<number>(0);
    const [localHex, setLocalHex] = useState<string>(cursorStyle[activeTabIndex]);

    useEffect(() => setLocalHex(cursorStyle[activeTabIndex]), [cursorStyle, activeTabIndex]);

    return (
        <div className={'color-picker'}>
            <div className={'color-picker__tabs'}>
                {cursorSections.map((name, index) => (
                    <TabSelector
                        name={name}
                        key={index}
                        tabIndex={index}
                        scheme={'light'}
                        forceHideDivider={index === cursorSections.length - 1}
                        activeTabIndex={activeTabIndex}
                        setAsActiveTab={setActiveTabIndex}
                    />
                ))}
            </div>
            <div className={'color-picker__body'}>
                <HexAlphaColorPicker
                    color={cursorStyle[activeTabIndex]}
                    onChange={(color) => {
                        const newStyle: CursorStyle = [...cursorStyle];
                        newStyle[activeTabIndex] = color;
                        updateCursorStyle(newStyle);
                    }}
                />
                <input
                    type="text"
                    className={'color-picker__body__text'}
                    value={localHex.toUpperCase()}
                    onChange={(e) => {
                        const color = e.target.value;
                        if (!tinycolor(color).isValid()) {
                            setLocalHex(color);
                            return;
                        }
                        const newStyle: CursorStyle = [...cursorStyle];
                        newStyle[activeTabIndex] = color;
                        updateCursorStyle(newStyle);
                        writeVisualsConfigIfNew();
                    }}
                />
            </div>
        </div>
    );
};

export default ColorPicker;
