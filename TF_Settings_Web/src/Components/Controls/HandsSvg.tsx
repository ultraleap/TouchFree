import React from 'react';

export class HandSvgCoordinate {
    constructor(_x: number, _y: number, _z: number) {
        this.x = _x;
        this.y = _y;
        this.z = _z;
    }

    x: number;
    y: number;
    z: number;
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
        _thumbKnuckle: HandSvgCoordinate,
        _wrist: HandSvgCoordinate,
        _primaryHand: boolean,
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
        this.thumbKnuckle = _thumbKnuckle;
        this.wrist = _wrist;

        this.primaryHand = _primaryHand;

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
    thumbKnuckle: HandSvgCoordinate;

    wrist: HandSvgCoordinate;

    primaryHand: boolean;

    dotColor: string;
}

interface DataWrapper {
    data: HandSvgProps;
}

export const HandSvg: React.FC<DataWrapper> = ({ data }) => {
    if (!data?.dotColor) {
        return <svg style={{ marginLeft: '-800px' }} xmlns="http://www.w3.org/2000/svg" height="800" width="800"></svg>;
    }

    const scalingFactor = data.middleKnuckle.z > 600 ? 1 : data.middleKnuckle.z < 100 ? 6 : 600/data.middleKnuckle.z;
    const pointRadius = 5 * scalingFactor;
    const pointLineWidth = 0;
    const strokeWidth = 5 * scalingFactor;

    const dotColor = 'url(\'#dotGradient\')';
    const lineColor = 'white';
    const dotBorderColour = 'black';

    return (
        <svg style={{ marginLeft: '-800px' }} xmlns="http://www.w3.org/2000/svg" height="800" width="800">
            <defs>
            <radialGradient id="dotGradient">
                <stop offset="0%" stopColor="#00EB85" />
                <stop offset="100%" stopColor="#00CCCE" />
            </radialGradient>
            </defs>
            <line
                id="index"
                x1={data.indexTip.x}
                y1={data.indexTip.y}
                x2={data.indexKnuckle.x}
                y2={data.indexKnuckle.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <line
                id="index-wrist"
                x1={data.indexKnuckle.x}
                y1={data.indexKnuckle.y}
                x2={data.thumbKnuckle.x}
                y2={data.thumbKnuckle.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <line
                id="index-middle"
                x1={data.indexKnuckle.x}
                y1={data.indexKnuckle.y}
                x2={data.middleKnuckle.x}
                y2={data.middleKnuckle.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <circle
                id="index-tip"
                cx={data.indexTip.x}
                cy={data.indexTip.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />
            <circle
                id="index-knuckle"
                cx={data.indexKnuckle.x}
                cy={data.indexKnuckle.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />

            <line
                id="middle"
                x1={data.middleTip.x}
                y1={data.middleTip.y}
                x2={data.middleKnuckle.x}
                y2={data.middleKnuckle.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <line
                id="middle-ring"
                x1={data.middleKnuckle.x}
                y1={data.middleKnuckle.y}
                x2={data.ringKnuckle.x}
                y2={data.ringKnuckle.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <circle
                id="middle-tip"
                cx={data.middleTip.x}
                cy={data.middleTip.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />
            <circle
                id="middle-knuckle"
                cx={data.middleKnuckle.x}
                cy={data.middleKnuckle.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />

            <line
                id="ring"
                x1={data.ringTip.x}
                y1={data.ringTip.y}
                x2={data.ringKnuckle.x}
                y2={data.ringKnuckle.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <line
                id="ring-little"
                x1={data.ringKnuckle.x}
                y1={data.ringKnuckle.y}
                x2={data.littleKnuckle.x}
                y2={data.littleKnuckle.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <circle
                id="ring-tip"
                cx={data.ringTip.x}
                cy={data.ringTip.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />
            <circle
                id="ring-knuckle"
                cx={data.ringKnuckle.x}
                cy={data.ringKnuckle.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />

            <line
                id="little"
                x1={data.littleTip.x}
                y1={data.littleTip.y}
                x2={data.littleKnuckle.x}
                y2={data.littleKnuckle.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <line
                id="little-wrist"
                x1={data.littleKnuckle.x}
                y1={data.littleKnuckle.y}
                x2={data.wrist.x}
                y2={data.wrist.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <circle
                id="little-tip"
                cx={data.littleTip.x}
                cy={data.littleTip.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />
            <circle
                id="little-knuckle"
                cx={data.littleKnuckle.x}
                cy={data.littleKnuckle.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />

            <line
                id="thumb"
                x1={data.thumbTip.x}
                y1={data.thumbTip.y}
                x2={data.thumbKnuckle.x}
                y2={data.thumbKnuckle.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <line
                id="thumb-wrist"
                x1={data.thumbKnuckle.x}
                y1={data.thumbKnuckle.y}
                x2={data.wrist.x}
                y2={data.wrist.y}
                stroke={lineColor}
                strokeWidth={strokeWidth}
            />
            <circle
                id="thumb-tip"
                cx={data.thumbTip.x}
                cy={data.thumbTip.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />
            <circle
                id="thumb-knuckle"
                cx={data.thumbKnuckle.x}
                cy={data.thumbKnuckle.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />

            <circle
                id="wrist"
                cx={data.wrist.x}
                cy={data.wrist.y}
                r={pointRadius}
                stroke={dotBorderColour}
                strokeWidth={pointLineWidth}
                fill={dotColor}
            />
        </svg>
    );
};
