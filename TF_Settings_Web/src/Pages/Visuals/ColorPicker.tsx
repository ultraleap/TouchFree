import './ReactColorful.scss';
import styles from './Visuals.module.scss';

import classNames from 'classnames/bind';
import React, { useState } from 'react';
import { useMemo } from 'react';
import { HexAlphaColorPicker } from 'react-colorful';

import { TabSelector } from '@/Components/Header';

const classes = classNames.bind(styles);

export const cursorSections = ['Outer Fill', 'Center Fill', 'Center Border'] as const;

export type CursorSectionColors = {
    [Property in (typeof cursorSections)[number]]: string;
};

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
        <div className={classes('color-picker')}>
            <div className={classes('color-picker__tabs')}>
                {cursorSections.map((name, index) => (
                    <TabSelector
                        name={name}
                        key={index}
                        tabIndex={index}
                        forceHideDivider={index === cursorSections.length - 1}
                        activeTabIndex={activeTabIndex}
                        setAsActiveTab={setActiveTabIndex}
                    />
                ))}
            </div>
            <div className={classes('color-picker__body')} id="color-picker">
                <HexAlphaColorPicker color={cursorColors[currentSection]} onChange={setCurrentColor} />
                <div>
                    <input
                        type="text"
                        id="color-picker__text"
                        value={cursorColors[currentSection]}
                        onChange={(e) => setCurrentColor(e.target.value)}
                    />
                </div>
            </div>
        </div>
    );
};

export default ColorPicker;
