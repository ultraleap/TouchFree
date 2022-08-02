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
    maskingValue: number;
    onDrag: (direction: SliderDirection, maskingValue: number) => void;
    onDragEnd: () => void;
}

const MaskingSlider: React.FC<MaskingSliderProps> = ({ direction, maskingValue, onDrag, onDragEnd }) => {
    // ===== State =====
    const [isHovered, setIsHovered] = useState<boolean>(false);
    const [isDragging, setIsDragging] = useState<boolean>(false);

    // ===== Refs =====
    const maskingValueRef = useRef<number>(maskingValue);
    const sliderRef = useRef<HTMLSpanElement>(null);

    const startPosInfo = useRef<SliderPosInfo>({
        X: Number.NaN,
        Y: Number.NaN,
        initialTop: Number.NaN,
        initialLeft: Number.NaN,
    });

    // ===== UseEffects =====
    useEffect(() => {
        if (sliderRef.current) {
            const top = Number.parseFloat(getComputedStyle(sliderRef.current).top);
            const left = Number.parseFloat(getComputedStyle(sliderRef.current).left);

            startPosInfo.current = { ...startPosInfo.current, initialTop: top, initialLeft: left };

            setPosition(direction, startPosInfo.current, maskingValue, sliderRef.current);
        }

        return () => {
            window.removeEventListener('pointermove', onMove);
            window.removeEventListener('pointerup', onEndDrag);
        };
    }, []);

    useEffect(() => {
        if (!sliderRef.current) return;

        maskingValueRef.current = maskingValue;

        setPosition(direction, startPosInfo.current, maskingValue, sliderRef.current);
    }, [maskingValue]);

    // ===== Event Handlers =====
    const onStartDrag = (event: PointerEvent<HTMLSpanElement>) => {
        if (!sliderRef.current) return;

        setIsDragging(true);
        startPosInfo.current = { ...startPosInfo.current, X: event.pageX, Y: event.pageY };

        window.addEventListener('pointermove', onMove);
        window.addEventListener('pointerup', onEndDrag);
    };

    const onMove = (event: globalThis.PointerEvent) => {
        if (!sliderRef.current) return;

        let diffValue = 0;

        switch (direction) {
            case 'left':
                diffValue = event.pageX - startPosInfo.current.X;
                break;
            case 'right':
                diffValue = startPosInfo.current.X - event.pageX;
                break;
            case 'upper':
                diffValue = event.pageY - startPosInfo.current.Y;
                break;
            case 'lower':
                diffValue = startPosInfo.current.Y - event.pageY;
                break;
        }

        // Convert value from CSS value range [0..400] to masking range[0..0.5]
        const newMaskingValue = maskingValue + diffValue / 800;

        // Limit masking value to [0..0.5]
        const limitedNewMaskingValue = Math.min(0.5, Math.max(0, newMaskingValue));

        onDrag(direction, limitedNewMaskingValue);
    };

    const onEndDrag = () => {
        setIsDragging(false);
        window.removeEventListener('pointermove', onMove);
        window.removeEventListener('pointerup', onEndDrag);

        onDragEnd();
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

const setPosition = (
    direction: SliderDirection,
    startPosInfo: SliderPosInfo,
    maskingValue: number,
    slider: HTMLSpanElement
): void => {
    let { initialLeft: left, initialTop: top } = startPosInfo;

    // Map from masking range [0..0.5] to CSS value range [0..400]
    const value = maskingValue * 800;

    switch (direction) {
        case 'left':
            left += value;
            break;
        case 'right':
            left -= value;
            break;
        case 'upper':
            top += value;
            break;
        case 'lower':
            top -= value;
            break;
    }

    slider.style.left = `${left}px`;
    slider.style.top = `${top}px`;
};

export default MaskingSlider;
