import 'Styles/Camera/CameraMasking.scss';

import React, { PointerEvent, useEffect, useRef, useState } from 'react';

import { Mask } from 'TouchFree/Tracking/TrackingTypes';

export type SliderDirection = keyof Mask;

interface SliderPosInfo {
    X: number;
    Y: number;
    initialTop: number;
    initialLeft: number;
}

interface MaskingSliderProps {
    direction: SliderDirection;
    value: number;
    setMaskingValue: (direction: SliderDirection, value: number) => void;
}

const MaskingSlider: React.FC<MaskingSliderProps> = ({ direction, value, setMaskingValue }) => {
    const [isHovered, setIsHovered] = useState<boolean>(false);
    const [isDragging, setIsDragging] = useState<boolean>(false);

    const sliderRef = useRef<HTMLSpanElement>(null);
    const maskingValueRef = useRef<number>(value);

    useEffect(() => {
        maskingValueRef.current = value;
    }, [value]);

    const startPosInfo = useRef<SliderPosInfo>({
        X: Number.NaN,
        Y: Number.NaN,
        initialTop: Number.NaN,
        initialLeft: Number.NaN,
    });

    useEffect(() => {
        if (sliderRef.current) {
            const top = Number.parseFloat(getComputedStyle(sliderRef.current).top);
            const left = Number.parseFloat(getComputedStyle(sliderRef.current).left);

            startPosInfo.current = { ...startPosInfo.current, initialTop: top, initialLeft: left };

            // sliderRef.current.style.left = `${top + mapMaskingRangeToValue(value)}px`;
            // sliderRef.current.style.top = `${startPosInfo.current.initialTop}px`;
        }
    }, []);

    const onStartDrag = (event: PointerEvent<HTMLSpanElement>) => {
        if (!sliderRef.current) return;

        setIsDragging(true);
        if (Number.isNaN(startPosInfo.current.X) || Number.isNaN(startPosInfo.current.Y)) {
            startPosInfo.current = { ...startPosInfo.current, X: event.pageX, Y: event.pageY };
        }

        window.addEventListener('pointermove', onMove);
        window.addEventListener('pointerup', onEndDrag);
    };

    const onMove = (event: globalThis.PointerEvent) => {
        if (!sliderRef.current) return;

        console.log(direction + ': ' + maskingValueRef.current);

        let diffX = 0;
        let diffY = 0;

        switch (direction) {
            case 'left':
                diffX = limitVal(event.pageX - startPosInfo.current.X);
                setMaskingValue(direction, mapValueToMaskingRange(diffX));
                break;
            case 'right':
                diffX = -limitVal(startPosInfo.current.X - event.pageX);
                setMaskingValue(direction, mapValueToMaskingRange(diffX));
                break;
            case 'upper':
                diffY = limitVal(event.pageY - startPosInfo.current.Y);
                setMaskingValue(direction, mapValueToMaskingRange(diffY));
                break;
            case 'lower':
                diffY = -limitVal(startPosInfo.current.Y - event.pageY);
                setMaskingValue(direction, mapValueToMaskingRange(diffY));
                break;
        }

        sliderRef.current.style.left = `${startPosInfo.current.initialLeft + diffX}px`;
        sliderRef.current.style.top = `${startPosInfo.current.initialTop + diffY}px`;
    };

    const onEndDrag = () => {
        setIsDragging(false);
        window.removeEventListener('pointermove', onMove);
        window.removeEventListener('pointerup', onEndDrag);
    };

    return (
        <span ref={sliderRef} className={`masking-slider--${direction}`}>
            <div
                className={`masking-slider-knob ${isHovered || isDragging ? 'masking-slider-knob--dragging' : ''}`}
                onPointerDown={onStartDrag}
                onPointerEnter={() => setIsHovered(true)}
                onPointerLeave={() => setIsHovered(false)}
            >
                <div className="masking-slider-arrow--one" />
                <div className="masking-slider-arrow--two" />
            </div>
        </span>
    );
};

// Map from CSS value [0..400] to Masking Range [0..5]
const mapValueToMaskingRange = (val: number): number => (Math.abs(val) / 400) * 0.5;

// Map from Masking Range [0..5] to CSS value [0..400]
const mapMaskingRangeToValue = (maskingVal: number): number => maskingVal * 2 * 400;

const limitVal = (val: number): number => Math.min(400, Math.max(0, val));

export default MaskingSlider;
