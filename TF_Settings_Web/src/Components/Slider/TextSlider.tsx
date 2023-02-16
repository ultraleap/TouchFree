import styles from './Sliders.module.scss';
import interactionStyles from '@/Pages/Interactions/Interactions.module.scss';

import classnames from 'classnames/bind';
import React, { PointerEvent, useEffect, useRef, useState } from 'react';

const classes = classnames.bind(styles);
const interactionClasses = classnames.bind(interactionStyles);

interface TextSliderProps {
    name: string;
    rangeMin: number;
    rangeMax: number;
    stepSize?: number;
    leftLabel: string;
    rightLabel: string;
    value: number;
    onChange: (newValue: number) => void;
    onFinish?: () => void;
}

const TextSlider: React.FC<TextSliderProps> = ({
    name,
    rangeMin,
    rangeMax,
    leftLabel,
    rightLabel,
    value,
    onChange,
    onFinish,
    stepSize = 0.05,
}) => {
    const [isDragging, setDragging] = useState<boolean>(false);
    const inputElement = useRef<HTMLInputElement>(null);

    useEffect(() => {
        document.body.addEventListener('pointerup', onUpCancel);
        document.body.addEventListener('pointercancel', onUpCancel);
    }, []);

    const onTextChange = (e: React.FormEvent<HTMLInputElement>): void => {
        const value: number = Number.parseFloat(e.currentTarget.value);
        onChange(value);
    };

    const onUpCancel = () => {
        setDragging(false);
    };

    const onDown = (event: PointerEvent<HTMLInputElement>) => {
        setDragging(true);
        setValueByPos(event.nativeEvent.offsetX);
    };

    const onMove = (event: PointerEvent<HTMLInputElement>) => {
        if (isDragging) {
            setValueByPos(event.nativeEvent.offsetX);
        }
    };

    const setValueByPos = (xPos: number) => {
        if (!inputElement.current) return;
        // Slider height is currently 0.75rem
        const remValue = inputElement.current.clientHeight;

        // Slider control is 1.5rem wide, so half is 1x remValue, full is 2x remValue
        const posInRange = (xPos - remValue) / (inputElement.current.clientWidth - 2 * remValue);
        const outputValue = lerp(rangeMin, rangeMax, posInRange);
        const roundedValue = Math.round(outputValue * (1 / stepSize)) / (1 / stepSize);

        if (rangeMin <= roundedValue && roundedValue <= rangeMax) {
            onChange(roundedValue);
        }
    };

    const lerp = (v0: number, v1: number, t: number): number => {
        return v0 * (1 - t) + v1 * t;
    };

    return (
        <label className={interactionClasses('input-label-container')}>
            <p className={interactionClasses('label')}>{name}</p>
            <div className={classes('sliderContainer')}>
                <input
                    type="range"
                    step={stepSize}
                    min={rangeMin}
                    max={rangeMax}
                    value={value}
                    className={classes('slider')}
                    onChange={() => {}}
                    onPointerMove={onMove}
                    onPointerDown={onDown}
                    onPointerUp={() => {
                        onUpCancel();
                        onFinish?.();
                    }}
                    onPointerCancel={onUpCancel}
                    id="myRange"
                    ref={inputElement}
                />
                <div className={classes('sliderLabelContainer')}>
                    <label className={interactionClasses('leftLabel')}>{leftLabel}</label>
                    <label className={interactionClasses('rightLabel')}>{rightLabel}</label>
                </div>
            </div>
            <label className={classes('sliderTextContainer')}>
                <input
                    type="number"
                    step={stepSize}
                    className={classes('sliderText')}
                    value={value}
                    onChange={onTextChange}
                    onBlur={onFinish}
                    onKeyDown={(e) => {
                        if (e.key === 'Enter') {
                            onFinish?.();
                        }
                    }}
                />
            </label>
        </label>
    );
};

export default TextSlider;
