import {
    BufferGeometry,
    CircleBufferGeometry,
    DataTexture,
    DynamicDrawUsage,
    InstancedMesh,
    Mesh,
    MeshBasicMaterial,
    Object3D,
    PerspectiveCamera,
    PlaneGeometry,
    Scene,
    Vector3,
    WebGLRenderer,
} from 'three';
import { Line2 } from 'three/examples/jsm/lines/Line2.js';
import { LineGeometry } from 'three/examples/jsm/lines/LineGeometry';
import { LineMaterial } from 'three/examples/jsm/lines/LineMaterial';

import cssVariables from 'Styles/_variables.scss';

import { FingerType } from 'TouchFree/TouchFreeToolingTypes';
import { MapRangeToRange } from 'TouchFree/Utilities';

import { FingerData, HandData, HandState } from './createHandData';

export type BasicMesh = Mesh<BufferGeometry, MeshBasicMaterial>;

interface FingerMesh {
    visible: boolean;
    primary: boolean;
    tip: number;
    knuckle: number;
    lineToTip: Line2;
    lineToNextKnuckle: Line2;
}

interface WristMesh {
    visible: boolean;
    primary: boolean;
    point: number;
    lineToThumb: Line2;
}

interface HandMesh {
    fingers: {
        [FingerType.TYPE_THUMB]: FingerMesh;
        [FingerType.TYPE_INDEX]: FingerMesh;
        [FingerType.TYPE_MIDDLE]: FingerMesh;
        [FingerType.TYPE_RING]: FingerMesh;
        [FingerType.TYPE_PINKY]: FingerMesh;
    };
    wrist: WristMesh;
    circleMeshes: InstancedMesh<CircleBufferGeometry, MeshBasicMaterial>;
}

let _scene: Scene;
let _renderer: WebGLRenderer;
let _camera: PerspectiveCamera;
let _cameraFeedMesh: BasicMesh;
let _cameraFeedTexture: DataTexture;

let _primaryHandMesh: HandMesh;
let _secondaryHandMesh: HandMesh;

const dummy = new Object3D();

const BASE_LINE_THICKNESS = 0.005;

export const setupRenderScene = (div: HTMLDivElement) => {
    _scene = new Scene();
    _camera = new PerspectiveCamera(90);
    _camera.position.z = 2;

    _renderer = new WebGLRenderer();
    _renderer.setPixelRatio(window.devicePixelRatio);
    _renderer.setSize(div.clientWidth, div.clientHeight);
    div.appendChild(_renderer.domElement);

    _cameraFeedMesh = new Mesh(new PlaneGeometry(4, 4), new MeshBasicMaterial());
    _scene.add(_cameraFeedMesh);

    _primaryHandMesh = {
        fingers: {
            [FingerType.TYPE_THUMB]: addBasicFingerMesh(_scene, 0, true),
            [FingerType.TYPE_INDEX]: addBasicFingerMesh(_scene, 1, true),
            [FingerType.TYPE_MIDDLE]: addBasicFingerMesh(_scene, 2, true),
            [FingerType.TYPE_RING]: addBasicFingerMesh(_scene, 3, true),
            [FingerType.TYPE_PINKY]: addBasicFingerMesh(_scene, 4, true),
        },
        wrist: addBasicWristMesh(_scene, 10, true),
        circleMeshes: addBasicCircleMeshes(_scene, true),
    };

    _secondaryHandMesh = {
        fingers: {
            [FingerType.TYPE_THUMB]: addBasicFingerMesh(_scene, 0, false),
            [FingerType.TYPE_INDEX]: addBasicFingerMesh(_scene, 1, false),
            [FingerType.TYPE_MIDDLE]: addBasicFingerMesh(_scene, 2, false),
            [FingerType.TYPE_RING]: addBasicFingerMesh(_scene, 3, false),
            [FingerType.TYPE_PINKY]: addBasicFingerMesh(_scene, 4, false),
        },
        wrist: addBasicWristMesh(_scene, 10, false),
        circleMeshes: addBasicCircleMeshes(_scene, false),
    };
};

export const renderScene = () => _renderer.render(_scene, _camera);

export const updateCameraRender = (data: Uint8Array, width: number, height: number, handData: HandState) => {
    if (_cameraFeedTexture) _cameraFeedTexture.dispose();

    _cameraFeedTexture = new DataTexture(data, width, height);
    _cameraFeedTexture.flipY = true;
    _cameraFeedTexture.needsUpdate = true;

    _cameraFeedMesh.material.map = _cameraFeedTexture;
    _cameraFeedMesh.material.color.convertSRGBToLinear();

    if (handData?.one?.primaryHand) {
        updateHandMesh(_primaryHandMesh, handData?.one);
        updateHandMesh(_secondaryHandMesh, handData?.two);
    } else {
        updateHandMesh(_primaryHandMesh, handData?.two);
        updateHandMesh(_secondaryHandMesh, handData?.one);
    }
};

const addBasicFingerMesh = (scene: Scene, fingerIndex: number, isPrimary: boolean): FingerMesh => ({
    visible: false,
    primary: isPrimary,
    tip: fingerIndex * 2,
    knuckle: fingerIndex * 2 + 1,
    lineToTip: addBasicLine(scene),
    lineToNextKnuckle: addBasicLine(scene),
});

const addBasicWristMesh = (scene: Scene, circleIndex: number, isPrimary: boolean): WristMesh => ({
    visible: false,
    primary: isPrimary,
    point: circleIndex,
    lineToThumb: addBasicLine(scene),
});

const addBasicCircleMeshes = (
    scene: Scene,
    isPrimary: boolean
): InstancedMesh<CircleBufferGeometry, MeshBasicMaterial> => {
    const color = isPrimary ? cssVariables.ultraleapGreen : cssVariables.ultraleapGreenHalfOpacity;

    const mesh = new InstancedMesh(
        new CircleBufferGeometry(0.02, 10),
        new MeshBasicMaterial({ color, transparent: true }),
        22
    );
    mesh.instanceMatrix.setUsage(DynamicDrawUsage);
    mesh.visible = true;
    scene.add(mesh);
    return mesh;
};

const addBasicLine = (scene: Scene): Line2 => {
    const geometry = new LineGeometry();
    const line = new Line2(
        geometry,
        new LineMaterial({ color: 0xffffff, linewidth: BASE_LINE_THICKNESS, transparent: true })
    );
    line.visible = false;
    scene.add(line);
    return line;
};

const updateHandMesh = (handMesh: HandMesh, handData?: HandData) => {
    const isPrimary = handData?.primaryHand ?? false;

    let scale = 1;
    if (handData !== undefined) {
        scale = MapRangeToRange(handData.fingers[FingerType.TYPE_PINKY].tip.z, 0, 0.1, 1, 3);
    }
    Object.keys(handMesh.fingers).forEach((finger) => {
        const fingerType = Number(finger) as Exclude<FingerType, FingerType.TYPE_UNKNOWN>;
        let nextKnucklePos: Vector3 | undefined;
        if (fingerType === FingerType.TYPE_PINKY) {
            nextKnucklePos = handData?.wrist;
        } else {
            nextKnucklePos =
                handData?.fingers[(fingerType + 1) as Exclude<FingerType, FingerType.TYPE_UNKNOWN>].knuckle;
        }
        updateFingerMesh(
            handMesh.circleMeshes,
            handMesh.fingers[fingerType],
            isPrimary,
            handData?.fingers[fingerType],
            nextKnucklePos,
            scale
        );
    });

    updateWristMesh(
        handMesh.circleMeshes,
        handMesh.wrist,
        isPrimary,
        handData?.wrist,
        handData?.fingers[FingerType.TYPE_THUMB].knuckle,
        scale
    );
};

const updateFingerMesh = (
    circleMeshes: InstancedMesh,
    fingerMesh: FingerMesh,
    isPrimary: boolean,
    finger?: FingerData,
    nextKnucklePos?: Vector3,
    scale?: number
) => {
    if (finger && nextKnucklePos) {
        const scaleToUse = scale ?? 1;
        moveMesh(circleMeshes, fingerMesh.tip, finger.tip, scaleToUse);
        moveMesh(circleMeshes, fingerMesh.knuckle, finger.knuckle, scaleToUse);
        moveLine(fingerMesh.lineToTip, finger.knuckle, finger.tip, isPrimary, scaleToUse);
        moveLine(fingerMesh.lineToNextKnuckle, finger.knuckle, nextKnucklePos, isPrimary, scaleToUse);
        if (!fingerMesh.visible) {
            circleMeshes.visible = true;
            fingerMesh.lineToTip.visible = true;
            fingerMesh.lineToNextKnuckle.visible = true;
            fingerMesh.visible = true;
        }
    } else if (fingerMesh.visible) {
        circleMeshes.visible = false;
        fingerMesh.lineToTip.visible = false;
        fingerMesh.lineToNextKnuckle.visible = false;
        fingerMesh.visible = false;
    }
};

const updateWristMesh = (
    circleMeshes: InstancedMesh,
    wristMesh: WristMesh,
    isPrimary: boolean,
    wrist?: Vector3,
    thumbKnucklePos?: Vector3,
    scale?: number
) => {
    const scaleToUse = scale ?? 1;
    if (wrist && thumbKnucklePos) {
        moveMesh(circleMeshes, wristMesh.point, wrist, scaleToUse);
        moveLine(wristMesh.lineToThumb, wrist, thumbKnucklePos, isPrimary, scaleToUse);
        if (!wristMesh.visible) {
            wristMesh.lineToThumb.visible = true;
            wristMesh.visible = true;
        }
    } else if (wristMesh.visible) {
        wristMesh.lineToThumb.visible = false;
        wristMesh.visible = false;
    }
};

const moveLine = (line: Line2, start: Vector3, end: Vector3, isPrimary: boolean, scale: number) => {
    line.geometry.setPositions([start.x, start.y, start.z, end.x, end.y, end.z]);
    //const scale = MapRangeToRange((start.z + end.z) / 2, 0, 0.1, 1, 3);
    line.material.linewidth = BASE_LINE_THICKNESS * scale;
    line.material.opacity = isPrimary ? 1 : 0.5;
    line.material.needsUpdate = true;
};

const moveMesh = (circleMeshes: InstancedMesh, meshIndex: number, position: Vector3, scale: number) => {
    dummy.position.set(position.x / scale, position.y / scale, position.z + 0.001);
    dummy.updateMatrix();

    circleMeshes.scale.set(scale, scale, 1);
    circleMeshes.setMatrixAt(meshIndex, dummy.matrix);
    circleMeshes.instanceMatrix.needsUpdate = true;
};
