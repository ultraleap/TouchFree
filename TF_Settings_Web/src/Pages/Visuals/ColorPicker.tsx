import './ColorPicker.scss';

import React, { useState, useEffect } from 'react';
import { HexAlphaColorPicker } from 'react-colorful';

import { TabSelector } from '@/Components/Header';

import { CursorStyle } from './VisualsUtils';

interface ColorPickerProps {
    cursorStyle: CursorStyle;
    updateCursorPreview: (style: CursorStyle) => void;
    updateConfigCursorStyle: (style: CursorStyle) => void;
}

const cursorSections = ['Center Fill', 'Outer Fill', 'Center Border'] as const;

const ColorPicker: React.FC<ColorPickerProps> = ({ cursorStyle, updateCursorPreview, updateConfigCursorStyle }) => {
    const [activeTabIndex, setActiveTabIndex] = useState<number>(0);
    const [localCursorStyle, setLocalCursorStyle] = useState<CursorStyle>(cursorStyle);

    useEffect(() => {
        setLocalCursorStyle(cursorStyle);
    }, [cursorStyle]);

    const updateLocalCursorStyle = (style: string) => {
        const newState = { ...localCursorStyle, [activeTabIndex]: style };
        updateCursorPreview(newState);
        setLocalCursorStyle(newState);
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
                <HexAlphaColorPicker
                    color={localCursorStyle[activeTabIndex]}
                    onChange={(color) => updateLocalCursorStyle(color)}
                    onPointerUp={() => updateConfigCursorStyle(localCursorStyle)}
                />
                <input
                    type="text"
                    className={'color-picker__body__text'}
                    value={localCursorStyle[activeTabIndex].toUpperCase()}
                    onChange={(e) => updateLocalCursorStyle(e.target.value)}
                    onBlur={() => updateConfigCursorStyle(localCursorStyle)}
                    onKeyDown={(e) => {
                        if (e.key === 'Enter') {
                            updateConfigCursorStyle(localCursorStyle);
                        }
                    }}
                />
            </div>
        </div>
    );
};

export default ColorPicker;
