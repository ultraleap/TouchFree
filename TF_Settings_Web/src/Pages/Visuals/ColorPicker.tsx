import './ColorPicker.scss';

import React, { useState } from 'react';
import { useMemo } from 'react';
import { HexAlphaColorPicker } from 'react-colorful';

import { TabSelector } from '@/Components/Header';

import { CursorSectionColors, cursorSections } from './CursorColorDefaults';

interface ColorPickerProps {
    cursorColors: CursorSectionColors;
    updateCursorColors: (colors: CursorSectionColors) => void;
}

const ColorPicker: React.FC<ColorPickerProps> = ({ cursorColors, updateCursorColors }) => {
    const [activeTabIndex, setActiveTabIndex] = useState<number>(0);

    const currentSection = useMemo(() => cursorSections[activeTabIndex], [activeTabIndex]);

    const setCurrentColor = (color: string) => {
        const newState = { ...cursorColors };
        newState[currentSection] = color;
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
                <HexAlphaColorPicker color={cursorColors[currentSection]} onChange={setCurrentColor} />
                <input
                    type="text"
                    className={'color-picker__body__text'}
                    value={cursorColors[currentSection]}
                    onChange={(e) => setCurrentColor(e.target.value)}
                />
            </div>
        </div>
    );
};

export default ColorPicker;
