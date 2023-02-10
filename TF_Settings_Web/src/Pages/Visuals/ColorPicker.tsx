import './ColorPicker.scss';

import React, { useState } from 'react';
import { HexAlphaColorPicker } from 'react-colorful';

import { TabSelector } from '@/Components/Header';

import { CursorColors } from './VisualsUtils';

interface ColorPickerProps {
    cursorColors: CursorColors;
    updateCursorColors: (colors: CursorColors) => void;
}

const cursorSections = ['Outer Fill', 'Center Fill', 'Center Border'] as const;

const ColorPicker: React.FC<ColorPickerProps> = ({ cursorColors, updateCursorColors }) => {
    const [activeTabIndex, setActiveTabIndex] = useState<number>(0);

    const setCurrentColor = (color: string) => {
        const newState: CursorColors = { ...cursorColors };
        newState[activeTabIndex] = color;
        updateCursorColors(newState);
    };

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
                <HexAlphaColorPicker color={cursorColors[activeTabIndex]} onChange={setCurrentColor} />
                <input
                    type="text"
                    className={'color-picker__body__text'}
                    value={cursorColors[activeTabIndex]}
                    onChange={(e) => setCurrentColor(e.target.value)}
                />
            </div>
        </div>
    );
};

export default ColorPicker;
