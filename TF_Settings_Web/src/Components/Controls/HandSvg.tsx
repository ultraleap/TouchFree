import React from 'react';

interface HandSvgCoordinate {
    x: number;
    y: number;
}

interface HandSvgProps {
    indexTip: HandSvgCoordinate;
    indexKnuckle: HandSvgCoordinate;

    middleTip: HandSvgCoordinate;
    middleKnuckle: HandSvgCoordinate;

    ringTip: HandSvgCoordinate;
    ringKnuckle: HandSvgCoordinate;

    littleTip: HandSvgCoordinate;
    littleKnuckle: HandSvgCoordinate;

    thumbTip: HandSvgCoordinate;

    wristA: HandSvgCoordinate;
    wristB: HandSvgCoordinate;

    dotColor: string;
}

export const HandSvg: React.FC<HandSvgProps> = ({
    indexTip,
    indexKnuckle,
    middleTip,
    middleKnuckle,
    ringTip,
    ringKnuckle,
    littleTip,
    littleKnuckle,
    thumbTip,
    wristA,
    wristB,
    dotColor = 'blue'
}) => {
    return (
        <svg xmlns="http://www.w3.org/2000/svg" height="400" width="400">
            <line
                id="inde.X"
                x1={indexTip.x}
                y1={indexTip.y}
                x2={indexKnuckle.x}
                y2={indexKnuckle.y}
                stroke="black"
                strokeWidth="10"
            />
            <line
                id="inde.X-wrist"
                x1={indexKnuckle.x}
                y1={indexKnuckle.y}
                x2={wristA.x}
                y2={wristA.y}
                stroke="black"
                strokeWidth="10"
            />
            <line
                id="inde.X-middle"
                x1={indexKnuckle.x}
                y1={indexKnuckle.y}
                x2={middleKnuckle.x}
                y2={middleKnuckle.y}
                stroke="black"
                strokeWidth="10"
            />
            <circle
                id="inde.X-tip"
                cx={indexTip.x}
                cy={indexTip.y}
                r="10"
                stroke="black"
                strokeWidth="3"
                fill={dotColor}
            />
            <circle
                id="inde.X-knuckle"
                cx={indexKnuckle.x}
                cy={indexKnuckle.y}
                r="10"
                stroke="black"
                strokeWidth="3"
                fill={dotColor}
            />

            <line
                id="middle"
                x1={middleTip.x}
                y1={middleTip.y}
                x2={middleKnuckle.x}
                y2={middleKnuckle.y}
                stroke="black"
                strokeWidth="10"
            />
            <line
                id="middle-ring"
                x1={middleKnuckle.x}
                y1={middleKnuckle.y}
                x2={ringKnuckle.x}
                y2={ringKnuckle.y}
                stroke="black"
                strokeWidth="10"
            />
            <circle
                id="middle-tip"
                cx={middleTip.x}
                cy={middleTip.y}
                r="10"
                stroke="black"
                strokeWidth="3"
                fill={dotColor}
            />
            <circle
                id="middle-knuckle"
                cx={middleKnuckle.x}
                cy={middleKnuckle.y}
                r="10"
                stroke="black"
                strokeWidth="3"
                fill={dotColor}
            />

            <line
                id="ring"
                x1={ringTip.x}
                y1={ringTip.y}
                x2={ringKnuckle.x}
                y2={ringKnuckle.y}
                stroke="black"
                strokeWidth="10"
            />
            <line
                id="ring-little"
                x1={ringKnuckle.x}
                y1={ringKnuckle.y}
                x2={littleKnuckle.x}
                y2={littleKnuckle.y}
                stroke="black"
                strokeWidth="10"
            />
            <circle id="ring-tip" cx={ringTip.x} cy={ringTip.y} r="10" stroke="black" strokeWidth="3" fill={dotColor} />
            <circle
                id="ring-knuckle"
                cx={ringKnuckle.x}
                cy={ringKnuckle.y}
                r="10"
                stroke="black"
                strokeWidth="3"
                fill={dotColor}
            />

            <line
                id="little"
                x1={littleTip.x}
                y1={littleTip.y}
                x2={littleKnuckle.x}
                y2={littleKnuckle.y}
                stroke="black"
                strokeWidth="10"
            />
            <line
                id="little-wrist"
                x1={littleKnuckle.x}
                y1={littleKnuckle.y}
                x2={wristB.x}
                y2={wristB.y}
                stroke="black"
                strokeWidth="10"
            />
            <circle
                id="little-tip"
                cx={littleTip.x}
                cy={littleTip.y}
                r="10"
                stroke="black"
                strokeWidth="3"
                fill={dotColor}
            />
            <circle
                id="little-knuckle"
                cx={littleKnuckle.x}
                cy={littleKnuckle.y}
                r="10"
                stroke="black"
                strokeWidth="3"
                fill={dotColor}
            />

            <line
                id="thumb-wrist"
                x1={thumbTip.x}
                y1={thumbTip.y}
                x2={wristA.x}
                y2={wristA.y}
                stroke="black"
                strokeWidth="10"
            />
            <circle
                id="thumb-tip"
                cx={thumbTip.x}
                cy={thumbTip.y}
                r="10"
                stroke="black"
                strokeWidth="3"
                fill={dotColor}
            />

            <line id="wrist" x1={wristA.x} y1={wristA.y} x2={wristB.x} y2={wristB.y} stroke="black" strokeWidth="10" />
            <circle id="wrist-a" cx={wristA.x} cy={wristA.y} r="10" stroke="black" strokeWidth="3" fill={dotColor} />
            <circle id="wrist-b" cx={wristB.x} cy={wristB.y} r="10" stroke="black" strokeWidth="3" fill={dotColor} />
        </svg>
    );
};
