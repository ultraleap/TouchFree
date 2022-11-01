import { v4 as uuidgen } from 'uuid';

import { Vector } from 'TouchFree/src/Configuration/ConfigurationTypes';
import { ConnectionManager } from 'TouchFree/src/Connection/ConnectionManager';
import {
    ActionCode,
    CommunicationWrapper,
    HandRenderDataStateRequest,
} from 'TouchFree/src/Connection/TouchFreeServiceTypes';
import { RawFinger, RawHand } from 'TouchFree/src/TouchFreeToolingTypes';

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

const translateToCoordinate = (coordinate?: Vector) => {
    if (coordinate === undefined) return new HandSvgCoordinate(-1, -1, 600);
    return new HandSvgCoordinate(1000 * (1 - coordinate.X * 1) - 100, 1000 * coordinate.Y - 100, coordinate.Z);
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
