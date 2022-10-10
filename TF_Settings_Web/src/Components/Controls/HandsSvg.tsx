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

export class HandState {
    one?: HandSvgProps;
    two?: HandSvgProps;
}

export const HandsSvg: React.FC<HandState> = ({ one, two }) => {
    return (
        <svg xmlns="http://www.w3.org/2000/svg" height="800" width="800" shapeRendering="optimizeSpeed">
            <defs>
                <radialGradient id="dotGradient">
                    <stop offset="0%" stopColor="#00EB85" />
                    <stop offset="100%" stopColor="#00CCCE" />
                </radialGradient>
            </defs>

            <HandSvg data={one} />
            <HandSvg data={two} />
        </svg>
    );
};

export const HandSvg: React.FC<{ data?: HandSvgProps }> = ({ data }) => {
    if (!data) {
        return <g></g>;
    }

    const scalingFactor = data.middleKnuckle.z > 600 ? 1 : data.middleKnuckle.z < 100 ? 6 : 600 / data.middleKnuckle.z;
    const pointRadius = Math.round(5 * scalingFactor);
    const strokeWidth = Math.round(5 * scalingFactor);

    const elements = [
        ...CreateFingerElements({
            id: 'index-' + (data.primaryHand ? 'p' : 's'),
            point1: data.indexTip,
            point2: data.indexKnuckle,
            pointRadius: pointRadius,
            strokeWidth: strokeWidth,
        }),
        ...CreateFingerElements({
            id: 'middle-' + (data.primaryHand ? 'p' : 's'),
            point1: data.middleTip,
            point2: data.middleKnuckle,
            pointRadius: pointRadius,
            strokeWidth: strokeWidth,
        }),
        ...CreateFingerElements({
            id: 'ring-' + (data.primaryHand ? 'p' : 's'),
            point1: data.ringTip,
            point2: data.ringKnuckle,
            pointRadius: pointRadius,
            strokeWidth: strokeWidth,
        }),
        ...CreateFingerElements({
            id: 'little-' + (data.primaryHand ? 'p' : 's'),
            point1: data.littleTip,
            point2: data.littleKnuckle,
            pointRadius: pointRadius,
            strokeWidth: strokeWidth,
        }),
        ...CreateFingerElements({
            id: 'thumb-' + (data.primaryHand ? 'p' : 's'),
            point1: data.thumbTip,
            point2: data.thumbKnuckle,
            pointRadius: pointRadius,
            strokeWidth: strokeWidth,
        }),
        {
            z: data.wrist.z,
            element: (
                <CircleSvg
                    key={'wrist-' + (data.primaryHand ? 'p' : 's')}
                    id="wrist"
                    point={data.wrist}
                    radius={pointRadius}
                />
            ),
        },
    ]
        .sort((a, b) => b.z - a.z)
        .map((x) => x.element);

    return (
        <g opacity={data.primaryHand ? 1 : 0.3}>
            <LineSvg id="index-wrist" point1={data.indexKnuckle} point2={data.thumbKnuckle} strokeWidth={strokeWidth} />
            <LineSvg
                id="index-middle"
                point1={data.indexKnuckle}
                point2={data.middleKnuckle}
                strokeWidth={strokeWidth}
            />
            <LineSvg id="middle-ring" point1={data.middleKnuckle} point2={data.ringKnuckle} strokeWidth={strokeWidth} />
            <LineSvg id="ring-little" point1={data.ringKnuckle} point2={data.littleKnuckle} strokeWidth={strokeWidth} />

            <LineSvg id="little-wrist" point1={data.littleKnuckle} point2={data.wrist} strokeWidth={strokeWidth} />
            <LineSvg id="thumb-wrist" point1={data.thumbKnuckle} point2={data.wrist} strokeWidth={strokeWidth} />

            {elements}
        </g>
    );
};

interface FingerData {
    id: string;
    point1: HandSvgCoordinate;
    point2: HandSvgCoordinate;
    strokeWidth: number;
    pointRadius: number;
}

const CreateFingerElements = (data: FingerData) => {
    const points = [
        {
            z: data.point1.z > data.point2.z ? data.point1.z : data.point2.z,
            element: (
                <LineSvg
                    key={data.id + '-line'}
                    id={data.id + '-line'}
                    point1={data.point1}
                    point2={data.point2}
                    strokeWidth={data.strokeWidth}
                />
            ),
        },
        {
            z: data.point1.z,
            element: (
                <CircleSvg key={data.id + '-tip'} id={data.id + '-tip'} point={data.point1} radius={data.pointRadius} />
            ),
        },
        {
            z: data.point2.z,
            element: (
                <CircleSvg
                    key={data.id + '-knuckle'}
                    id={data.id + '-knuckle'}
                    point={data.point2}
                    radius={data.pointRadius}
                />
            ),
        },
    ];

    return points;
};

interface LineData {
    id: string;
    point1: HandSvgCoordinate;
    point2: HandSvgCoordinate;
    strokeWidth: number;
}

const LineSvg: React.FC<LineData> = (data) => {
    return (
        <line
            id={data.id}
            x1={data.point1.x}
            y1={data.point1.y}
            x2={data.point2.x}
            y2={data.point2.y}
            stroke={'white'}
            strokeWidth={data.strokeWidth}
            strokeLinecap="round"
        />
    );
};

interface CircleData {
    id: string;
    point: HandSvgCoordinate;
    radius: number;
}

const CircleSvg: React.FC<CircleData> = (data) => {
    return <circle id={data.id} cx={data.point.x} cy={data.point.y} r={data.radius} fill={'#00EB85'} />;
};
