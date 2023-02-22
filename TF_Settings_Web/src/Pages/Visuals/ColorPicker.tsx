import './ColorPicker.scss';

import React, { useState } from 'react';
import { HexAlphaColorPicker } from 'react-colorful';

import { TabSelector } from '@/Components/Header';

import { CursorStyle } from './VisualsUtils';

interface ColorPickerProps {
    cursorStyle: CursorStyle;
    updateCursorStyle: (style: CursorStyle) => void;
}

const cursorSections = ['Center Fill', 'Outer Fill', 'Center Border'] as const;

const ColorPicker: React.FC<ColorPickerProps> = ({ cursorStyle, updateCursorStyle }) => {
    const [activeTabIndex, setActiveTabIndex] = useState<number>(0);

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
                    onChange={(color) => updateCursorStyle({ ...cursorStyle, [activeTabIndex]: color })}
                />
                <input
                    type="text"
                    className={'color-picker__body__text'}
                    value={cursorStyle[activeTabIndex].toUpperCase()}
                    onChange={(e) => updateCursorStyle({ ...cursorStyle, [activeTabIndex]: e.target.value })}
                />
            </div>
        </div>
    );
};

export default ColorPicker;
