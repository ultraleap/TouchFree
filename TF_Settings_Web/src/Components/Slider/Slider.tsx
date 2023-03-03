import styles from './Sliders.module.scss';
import interactionStyles from '@/Pages/Interactions/Interactions.module.scss';

import classnames from 'classnames/bind';
import React, { PointerEvent, useEffect, useRef, useState } from 'react';

const classes = classnames.bind(styles);
const interactionClasses = classnames.bind(interactionStyles);

interface SliderProps {
    name: string;
    rangeMin: number;
    rangeMax: number;
    leftLabel: string;
    rightLabel: string;
    value: number;
    onChange: (newValue: number) => void;
    increment?: number;
}

const Slider: React.FC<SliderProps> = ({
    name,
    rangeMin,
    rangeMax,
    leftLabel,
    rightLabel,
    value,
    onChange,
    increment = 0.1,
}) => {
    const [dragging, setDragging] = useState<boolean>(false);
    const inputElement = useRef<HTMLInputElement>(null);

    useEffect(() => {
        document.addEventListener('pointerup', onUpCancel);
        document.addEventListener('pointercancel', onUpCancel);

        return () => {
            document.removeEventListener('pointerup', onUpCancel);
            document.removeEventListener('pointercancel', onUpCancel);
        };
    }, []);

    const onUpCancel = () => setDragging(false);

    const onDown = (event: PointerEvent<HTMLInputElement>) => {
        setDragging(true);
        setValueByPos(event.nativeEvent.offsetX);
    };

    const onMove = (event: PointerEvent<HTMLInputElement>) => {
        if (dragging) {
            setValueByPos(event.nativeEvent.offsetX);
        }
    };

    const setValueByPos = (xPos: number) => {
        if (!inputElement.current) return;

        // Slider height is currently 0.75rem
        const remValue = inputElement.current.clientHeight;

        // Slider control is 1.5rem wide, so half is 1x remValue, full is 2x remValue
        const posInRange = (xPos - remValue) / (inputElement.current.clientWidth - 2 * remValue);

        // Lerp value
        const outputValue = rangeMin * (1 - posInRange) + rangeMax * posInRange;

        if (rangeMin < outputValue && outputValue < rangeMax) {
            onChange(outputValue);
        }
    };

    return (
        <label className={interactionClasses('input-label-container')}>
            <p className={interactionClasses('label')}>{name}</p>
            <div className={classes('slider-container')}>
                <input
                    type="range"
                    step={increment}
                    min={rangeMin}
                    max={rangeMax}
                    className={classes('slider-container__slider')}
                    onChange={() => {}}
                    onPointerMove={onMove}
                    onPointerDown={onDown}
                    onPointerUp={onUpCancel}
                    onPointerCancel={onUpCancel}
                    value={value}
                    id="myRange"
                    ref={inputElement}
                />
                <div className={classes('slider-container__labels')}>
                    <label className={interactionClasses('leftLabel')}>{leftLabel}</label>
                    <label className={interactionClasses('rightLabel')}>{rightLabel}</label>
                </div>
            </div>
        </label>
    );
};

export default Slider;
