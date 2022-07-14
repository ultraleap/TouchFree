import React from 'react';

export class HandSvgCoordinate {
    constructor(_x: number, _y: number) {
        this.x = _x;
        this.y = _y;
    }

    x: number;
    y: number;
}

export class HandSvgProps {
    constructor(
        _indexTip: HandSvgCoordinate,
        _indexKnuckle: HandSvgCoordinate,
        _middleTip: HandSvgCoordinate,
        _middleKnuckle: HandSvgCoordinate,
        _ringTip: HandSvgCoordinate,
        _ringKnuckle: HandSvgCoordinate,
        _littleTip: HandSvgCoordinate,
        _littleKnuckle: HandSvgCoordinate,
        _thumbTip: HandSvgCoordinate,
        _wrist: HandSvgCoordinate,
        _dotColor: string
    ) {
        this.indexTip = _indexTip;
        this.indexKnuckle = _indexKnuckle;
        this.middleTip = _middleTip;
        this.middleKnuckle = _middleKnuckle;
        this.ringTip = _ringTip;
        this.ringKnuckle = _ringKnuckle;
        this.littleTip = _littleTip;
        this.littleKnuckle = _littleKnuckle;
        this.thumbTip = _thumbTip;
        this.wrist = _wrist;

        this.dotColor = _dotColor;
    }

    indexTip: HandSvgCoordinate;
    indexKnuckle: HandSvgCoordinate;

    middleTip: HandSvgCoordinate;
    middleKnuckle: HandSvgCoordinate;

    ringTip: HandSvgCoordinate;
    ringKnuckle: HandSvgCoordinate;

    littleTip: HandSvgCoordinate;
    littleKnuckle: HandSvgCoordinate;

    thumbTip: HandSvgCoordinate;

    wrist: HandSvgCoordinate;

    dotColor: string;
}

interface DataWrapper {
    data: HandSvgProps;
}

export const HandSvg: React.FC<DataWrapper> = ({ data }) => {
    const pointRadius = 5;
    const pointLineWidth = 1;
    const strokeWidth = 5;

    if (!data?.dotColor) {
        return <svg xmlns="http://www.w3.org/2000/svg" height="400" width="400"></svg>;
    }
    return (
        <svg xmlns="http://www.w3.org/2000/svg" height="400" width="400">
            <line
                id="inde.X"
                x1={data.indexTip.x}
                y1={data.indexTip.y}
                x2={data.indexKnuckle.x}
                y2={data.indexKnuckle.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <line
                id="inde.X-wrist"
                x1={data.indexKnuckle.x}
                y1={data.indexKnuckle.y}
                x2={data.wrist.x}
                y2={data.wrist.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <line
                id="inde.X-middle"
                x1={data.indexKnuckle.x}
                y1={data.indexKnuckle.y}
                x2={data.middleKnuckle.x}
                y2={data.middleKnuckle.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <circle
                id="inde.X-tip"
                cx={data.indexTip.x}
                cy={data.indexTip.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />
            <circle
                id="inde.X-knuckle"
                cx={data.indexKnuckle.x}
                cy={data.indexKnuckle.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />

            <line
                id="middle"
                x1={data.middleTip.x}
                y1={data.middleTip.y}
                x2={data.middleKnuckle.x}
                y2={data.middleKnuckle.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <line
                id="middle-ring"
                x1={data.middleKnuckle.x}
                y1={data.middleKnuckle.y}
                x2={data.ringKnuckle.x}
                y2={data.ringKnuckle.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <circle
                id="middle-tip"
                cx={data.middleTip.x}
                cy={data.middleTip.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />
            <circle
                id="middle-knuckle"
                cx={data.middleKnuckle.x}
                cy={data.middleKnuckle.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />

            <line
                id="ring"
                x1={data.ringTip.x}
                y1={data.ringTip.y}
                x2={data.ringKnuckle.x}
                y2={data.ringKnuckle.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <line
                id="ring-little"
                x1={data.ringKnuckle.x}
                y1={data.ringKnuckle.y}
                x2={data.littleKnuckle.x}
                y2={data.littleKnuckle.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <circle
                id="ring-tip"
                cx={data.ringTip.x}
                cy={data.ringTip.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />
            <circle
                id="ring-knuckle"
                cx={data.ringKnuckle.x}
                cy={data.ringKnuckle.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />

            <line
                id="little"
                x1={data.littleTip.x}
                y1={data.littleTip.y}
                x2={data.littleKnuckle.x}
                y2={data.littleKnuckle.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <line
                id="little-wrist"
                x1={data.littleKnuckle.x}
                y1={data.littleKnuckle.y}
                x2={data.wrist.x}
                y2={data.wrist.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <circle
                id="little-tip"
                cx={data.littleTip.x}
                cy={data.littleTip.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />
            <circle
                id="little-knuckle"
                cx={data.littleKnuckle.x}
                cy={data.littleKnuckle.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />

            <line
                id="thumb-wrist"
                x1={data.thumbTip.x}
                y1={data.thumbTip.y}
                x2={data.wrist.x}
                y2={data.wrist.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <circle
                id="thumb-tip"
                cx={data.thumbTip.x}
                cy={data.thumbTip.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />

            <circle
                id="wrist"
                cx={data.wrist.x}
                cy={data.wrist.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />

            {/* <line
                id="wrist"
                x1={data.wristA.x}
                y1={data.wristA.y}
                x2={data.wristB.x}
                y2={data.wristB.y}
                stroke="black"
                strokeWidth={strokeWidth}
            />
            <circle
                id="wrist-a"
                cx={data.wristA.x}
                cy={data.wristA.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            />
            <circle
                id="wrist-b"
                cx={data.wristB.x}
                cy={data.wristB.y}
                r={pointRadius}
                stroke="black"
                strokeWidth={pointLineWidth}
                fill={data.dotColor}
            /> */}
        </svg>
    );
};
