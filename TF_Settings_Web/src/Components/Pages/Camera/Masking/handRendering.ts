import { Vector3 } from 'three';

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

export interface FingerData {
    tip: Vector3;
    knuckle: Vector3;
}
export interface HandData {
    fingers: {
        index: FingerData;
        middle: FingerData;
        ring: FingerData;
        little: FingerData;
        thumb: FingerData;
    };
    wrist: Vector3;
    primaryHand: boolean;
}

export class HandState {
    one?: HandData;
    two?: HandData;
}

export const setHandRenderState = (handRenderState: boolean, lens: 'left' | 'right'): void => {
    const requestID = uuidgen();

    const content = new HandRenderDataStateRequest(requestID, handRenderState, lens);
    const request = new CommunicationWrapper(ActionCode.SET_HAND_DATA_STREAM_STATE, content);

    const jsonContent = JSON.stringify(request);

    ConnectionManager.serviceConnection()?.SendMessage(jsonContent, requestID, () => {
        return true;
    });
};

export const INVALD_POSITION = new Vector3(Number.NaN, Number.NaN, Number.NaN);

const translateToCoordinate = (coordinate?: Vector) => {
    if (coordinate === undefined) return INVALD_POSITION;
    const { X, Y, Z } = coordinate;
    // Map Z between 0 and 0.1
    const mappedZ = Z > 600 ? 0 : 1 - MapRangeToRange(Z, 0, 600, 0, 1);
    return new Vector3(1.2 * MapRangeToRange(1 - X, 0, 1, -1, 1), 1.25 * MapRangeToRange(1 - Y, 0, 1, -1, 1), mappedZ);
};

export const rawHandToHandData = (hand: RawHand): HandData => {
    const fingers = hand.Fingers;
    return {
        fingers: {
            thumb: createFingerData(fingers, 0),
            index: createFingerData(fingers, 1),
            middle: createFingerData(fingers, 2),
            ring: createFingerData(fingers, 3),
            little: createFingerData(fingers, 4),
        },
        wrist: translateToCoordinate(hand.WristPosition),
        primaryHand: hand.CurrentPrimary,
    };
};

const tipJointIndex = 3;
const knuckleJointIndex = 1;
const createFingerData = (fingers: RawFinger[], fingerType: number): FingerData => {
    const finger = fingers.find((f: RawFinger) => f.Type == fingerType);
    return {
        tip: translateToCoordinate(finger?.Bones[tipJointIndex]?.NextJoint),
        knuckle: translateToCoordinate(finger?.Bones[knuckleJointIndex]?.PrevJoint),
    };
};

const createEmptyFingerData = (): FingerData => {
    return {
        tip: translateToCoordinate(undefined),
        knuckle: translateToCoordinate(undefined),
    };
};

const emptyHand: HandData = {
    fingers: {
        thumb: createEmptyFingerData(),
        index: createEmptyFingerData(),
        middle: createEmptyFingerData(),
        ring: createEmptyFingerData(),
        little: createEmptyFingerData(),
    },
    wrist: translateToCoordinate(undefined),
    primaryHand: true,
};

export const defaultHandState: HandState = {
    one: { ...emptyHand },
    two: { ...emptyHand, primaryHand: false },
};
