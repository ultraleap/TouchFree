import 'Styles/Camera/CameraMasking.scss';

import React, { CSSProperties, PointerEvent, ReactElement, useEffect, useRef, useState } from 'react';

import { Mask } from 'TouchFree/Tracking/TrackingTypes';

export type SliderDirection = keyof Mask;
export interface CanvasInfo {
    size: number;
    offset: number;
}

interface MaskingSliderProps {
    direction: SliderDirection;
    maskingValue: number;
    canvasInfo: CanvasInfo;
    content?: ReactElement;
}

interface SliderPos {
    top: number;
    left: number;
}

export const MaskingSlider: React.FC<MaskingSliderProps> = ({ direction, maskingValue, canvasInfo, content }) => {
    const [initialSliderPos, setInitialSliderPos] = useState<SliderPos>({
        top: Number.NaN,
        left: Number.NaN,
    });

    // ===== Refs =====
    const sliderRef = useRef<HTMLSpanElement>(null);

    // ===== UseEffects =====
    useEffect(() => {
        if (!sliderRef.current) return;
        const top = Number.parseFloat(getComputedStyle(sliderRef.current).top);
        const left = Number.parseFloat(getComputedStyle(sliderRef.current).left);

        setPosition(direction, maskingValue, canvasInfo.size, { top, left }, sliderRef.current);
        setInitialSliderPos({ top, left });
    }, []);

    useEffect(() => {
        if (!sliderRef.current) return;

        setPosition(direction, maskingValue, canvasInfo.size, initialSliderPos, sliderRef.current);
    }, [maskingValue]);

    return (
        <span
            ref={sliderRef}
            className={`masking-slider masking-slider--${direction}`}
            style={
                { '--canvas-size': `${canvasInfo.size}px`, '--close-offset': `${canvasInfo.offset}px` } as CSSProperties
            }
        >
            {content}
        </span>
    );
};

interface MaskingSliderDraggableProps extends MaskingSliderProps {
    clearMasking: () => void;
    onDrag: (direction: SliderDirection, maskingValue: number) => void;
    onDragEnd: () => void;
}

export const MaskingSliderDraggable: React.FC<MaskingSliderDraggableProps> = ({
    direction,
    maskingValue,
    canvasInfo,
    clearMasking,
    onDrag,
    onDragEnd,
}) => {
    // ===== State =====
    const [isHovered, setIsHovered] = useState<boolean>(false);
    const [isDragging, setIsDragging] = useState<boolean>(false);

    const dragStartPos = useRef<{ X: number; Y: number }>({
        X: Number.NaN,
        Y: Number.NaN,
    });

    // ===== UseEffects =====
    useEffect(() => {
        return () => {
            window.removeEventListener('pointermove', onMove);
            window.removeEventListener('pointerup', onEndDrag);
        };
    }, []);
    // ===== Event Handlers =====
    const onStartDrag = (event: PointerEvent<HTMLSpanElement>) => {
        clearMasking();
        setIsDragging(true);
        dragStartPos.current = { X: event.pageX, Y: event.pageY };

        window.addEventListener('pointermove', onMove);
        window.addEventListener('pointerup', onEndDrag);
    };

    const onMove = (event: globalThis.PointerEvent) => {
        let diffValue = 0;

        const { X: startX, Y: startY } = dragStartPos.current;

        switch (direction) {
            case 'left':
                diffValue = event.pageX - startX;
                break;
            case 'right':
                diffValue = startX - event.pageX;
                break;
            case 'upper':
                diffValue = event.pageY - startY;
                break;
            case 'lower':
                diffValue = startY - event.pageY;
                break;
        }

        // Convert value from CSS value range [0..400] to masking range[0..0.5]
        const newMaskingValue = maskingValue + diffValue / canvasInfo.size;

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

    const content = (
        <div
            className={`masking-slider-knob ${isHovered || isDragging ? 'masking-slider-knob--dragging' : ''}`}
            onPointerDown={onStartDrag}
            onPointerEnter={() => setIsHovered(true)}
            onPointerLeave={() => setIsHovered(false)}
        >
            <div className="masking-slider-arrow--one" />
            <div className="masking-slider-arrow--two" />
        </div>
    );

    return (
        <MaskingSlider direction={direction} maskingValue={maskingValue} canvasInfo={canvasInfo} content={content} />
    );
};

const setPosition = (
    direction: SliderDirection,
    maskingValue: number,
    canvasSize: number,
    sliderPos: SliderPos,
    slider: HTMLSpanElement
): void => {
    // Map from masking range [0..0.5] to CSS value range [0..400]
    const value = maskingValue * canvasSize;

    let { top, left } = sliderPos;

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
