import { v4 as uuidgen } from 'uuid';

import { Vector } from 'TouchFree/Configuration/ConfigurationTypes';
import { ConnectionManager } from 'TouchFree/Connection/ConnectionManager';
import {
    ActionCode,
    CommunicationWrapper,
    HandRenderDataStateRequest,
} from 'TouchFree/Connection/TouchFreeServiceTypes';
import { RawFinger, RawHand } from 'TouchFree/TouchFreeToolingTypes';
import { MapRangeToRange } from 'TouchFree/Utilities';

import { HandState, HandSvgCoordinate, HandSvgProps } from 'Components/Controls/HandsSvg';

export const setHandRenderState = (handRenderState: boolean, lens: 'left' | 'right'): void => {
    const requestID = uuidgen();

    const content = new HandRenderDataStateRequest(requestID, handRenderState, lens);
    const request = new CommunicationWrapper(ActionCode.SET_HAND_DATA_STREAM_STATE, content);

    const jsonContent = JSON.stringify(request);

    ConnectionManager.serviceConnection()?.SendMessage(jsonContent, requestID, () => {
        return true;
    });
};

const zLimits = [100, 600];
const zMax = 0.1;

const translateToCoordinate = (coordinate?: Vector) => {
    if (coordinate === undefined) return new HandSvgCoordinate(-1, -1, -1);
    const { X, Y, Z } = coordinate;
    // Map Z between 0 and 0.1
    const mappedZ = Z < zLimits[0] ? zMax : Z > zLimits[1] ? 0 : zMax - Z / (zLimits[1] / zMax);
    return new HandSvgCoordinate(
        1.25 * MapRangeToRange(1 - X, 0, 1, -1, 1) - 0.125,
        1.25 * MapRangeToRange(1 - Y, 0, 1, -1, 1) - 0.125,
        mappedZ
    );
};

const tipJointIndex = 3;
const knuckleJointIndex = 1;

export const handToSvgData = (hand: RawHand, handIndex: number): HandSvgProps => {
    const indexFinger = hand.Fingers.find((f: RawFinger) => f.Type == 1);
    const middleFinger = hand.Fingers.find((f: RawFinger) => f.Type == 2);
    const ringFinger = hand.Fingers.find((f: RawFinger) => f.Type == 3);
    const littleFinger = hand.Fingers.find((f: RawFinger) => f.Type == 4);
    const thumbFinger = hand.Fingers.find((f: RawFinger) => f.Type == 0);
    const wrist = hand.WristPosition;
    return {
        indexTip: translateToCoordinate(indexFinger?.Bones[tipJointIndex]?.NextJoint),
        indexKnuckle: translateToCoordinate(indexFinger?.Bones[knuckleJointIndex]?.PrevJoint),
        middleTip: translateToCoordinate(middleFinger?.Bones[tipJointIndex]?.NextJoint),
        middleKnuckle: translateToCoordinate(middleFinger?.Bones[knuckleJointIndex]?.PrevJoint),
        ringTip: translateToCoordinate(ringFinger?.Bones[tipJointIndex]?.NextJoint),
        ringKnuckle: translateToCoordinate(ringFinger?.Bones[knuckleJointIndex]?.PrevJoint),
        littleTip: translateToCoordinate(littleFinger?.Bones[tipJointIndex]?.NextJoint),
        littleKnuckle: translateToCoordinate(littleFinger?.Bones[knuckleJointIndex]?.PrevJoint),
        thumbTip: translateToCoordinate(thumbFinger?.Bones[tipJointIndex]?.NextJoint),
        thumbKnuckle: translateToCoordinate(thumbFinger?.Bones[knuckleJointIndex]?.PrevJoint),
        wrist: translateToCoordinate(wrist),
        primaryHand: hand.CurrentPrimary,
        dotColor: handIndex === 0 ? 'blue' : 'red',
    };
};

export const defaultHandState: HandState = {
    one: {
        indexTip: translateToCoordinate(undefined),
        indexKnuckle: translateToCoordinate(undefined),
        middleTip: translateToCoordinate(undefined),
        middleKnuckle: translateToCoordinate(undefined),
        ringTip: translateToCoordinate(undefined),
        ringKnuckle: translateToCoordinate(undefined),
        littleTip: translateToCoordinate(undefined),
        littleKnuckle: translateToCoordinate(undefined),
        thumbTip: translateToCoordinate(undefined),
        thumbKnuckle: translateToCoordinate(undefined),
        wrist: translateToCoordinate(undefined),
        primaryHand: true,
        dotColor: 'blue',
    },
    two: {
        indexTip: translateToCoordinate(undefined),
        indexKnuckle: translateToCoordinate(undefined),
        middleTip: translateToCoordinate(undefined),
        middleKnuckle: translateToCoordinate(undefined),
        ringTip: translateToCoordinate(undefined),
        ringKnuckle: translateToCoordinate(undefined),
        littleTip: translateToCoordinate(undefined),
        littleKnuckle: translateToCoordinate(undefined),
        thumbTip: translateToCoordinate(undefined),
        thumbKnuckle: translateToCoordinate(undefined),
        wrist: translateToCoordinate(undefined),
        primaryHand: false,
        dotColor: 'red',
    },
};
