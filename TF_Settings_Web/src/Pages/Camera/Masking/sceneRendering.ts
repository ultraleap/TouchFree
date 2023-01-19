import {
    BufferGeometry,
    CircleBufferGeometry,
    DataTexture,
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

import cssVariables from '@/variables.module.scss';

import { FingerType } from 'TouchFree/src/TouchFreeToolingTypes';
import { MapRangeToRange } from 'TouchFree/src/Utilities';

import { FingerData, HandData, HandState } from './createHandData';

export type BasicMesh = Mesh<BufferGeometry, MeshBasicMaterial>;

interface FingerMeshData {
    visible: boolean;
    tip: number;
    knuckle: number;
    lineToTip: Line2;
    lineToNextKnuckle: Line2;
}

interface WristMeshData {
    visible: boolean;
    point: number;
    lineToThumb: Line2;
}

interface MeshUpdateData {
    meshIndices: number[];
    positions: Vector3[];
}

interface HandMeshData {
    fingers: {
        [FingerType.TYPE_THUMB]: FingerMeshData;
        [FingerType.TYPE_INDEX]: FingerMeshData;
        [FingerType.TYPE_MIDDLE]: FingerMeshData;
        [FingerType.TYPE_RING]: FingerMeshData;
        [FingerType.TYPE_PINKY]: FingerMeshData;
    };
    wrist: WristMeshData;
    circleMeshes: InstancedMesh<CircleBufferGeometry, MeshBasicMaterial>;
}

let _scene: Scene;
let _renderer: WebGLRenderer;
let _camera: PerspectiveCamera;
let _cameraFeedMesh: BasicMesh;
let _cameraFeedTexture: DataTexture;

let _primaryHandMesh: HandMeshData;
let _secondaryHandMesh: HandMeshData;

let _compiled = false;

const dummy = new Object3D();

const BASE_LINE_THICKNESS = 0.005;

export const setupRenderScene = (div: HTMLDivElement) => {
    for (const child of div.children) {
        div.removeChild(child);
    }
    _scene = new Scene();
    _camera = new PerspectiveCamera(90);
    _camera.position.z = 2;

    _renderer = new WebGLRenderer();
    _renderer.setPixelRatio(window.devicePixelRatio);
    _renderer.setSize(div.clientWidth, div.clientHeight);
    div.appendChild(_renderer.domElement);

    _cameraFeedMesh = new Mesh(new PlaneGeometry(4, 4), new MeshBasicMaterial());
    _scene.add(_cameraFeedMesh);

    _primaryHandMesh = createHandMesh(_scene, true);
    _secondaryHandMesh = createHandMesh(_scene, false);
    _compiled = false;

    return _renderer;
};

export const renderScene = () => {
    if (!_compiled) {
        _compiled = true;
        _renderer.compile(_scene, _camera);
    }

    _renderer.render(_scene, _camera);
};

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

const createHandMesh = (scene: Scene, primary: boolean): HandMeshData => {
    return {
        fingers: {
            [FingerType.TYPE_THUMB]: addBasicFingerMesh(scene, 0, primary),
            [FingerType.TYPE_INDEX]: addBasicFingerMesh(scene, 1, primary),
            [FingerType.TYPE_MIDDLE]: addBasicFingerMesh(scene, 2, primary),
            [FingerType.TYPE_RING]: addBasicFingerMesh(scene, 3, primary),
            [FingerType.TYPE_PINKY]: addBasicFingerMesh(scene, 4, primary),
        },
        wrist: addBasicWristMesh(scene, 10, primary),
        circleMeshes: addBasicCircleMeshes(scene, primary),
    };
};

const addBasicFingerMesh = (scene: Scene, fingerIndex: number, isPrimary: boolean): FingerMeshData => ({
    visible: false,
    tip: fingerIndex * 2,
    knuckle: fingerIndex * 2 + 1,
    lineToTip: addBasicLine(scene, isPrimary),
    lineToNextKnuckle: addBasicLine(scene, isPrimary),
});

const addBasicWristMesh = (scene: Scene, circleIndex: number, isPrimary: boolean): WristMeshData => ({
    visible: false,
    point: circleIndex,
    lineToThumb: addBasicLine(scene, isPrimary),
});

const addBasicCircleMeshes = (
    scene: Scene,
    isPrimary: boolean
): InstancedMesh<CircleBufferGeometry, MeshBasicMaterial> => {
    const mesh = new InstancedMesh(
        new CircleBufferGeometry(0.02, 10),
        new MeshBasicMaterial({
            color: cssVariables.ultraleapGreen,
            transparent: !isPrimary,
            opacity: isPrimary ? 1 : 0.5,
        }),
        11
    );

    mesh.visible = false;
    scene.add(mesh);
    return mesh;
};

const addBasicLine = (scene: Scene, isPrimary: boolean): Line2 => {
    const line = new Line2(new LineGeometry(), isPrimary ? lineMaterialPrimary : lineMaterialSecondary);
    line.visible = false;
    scene.add(line);
    return line;
};

const lineMaterialPrimary = new LineMaterial({
    color: 0xffffff,
    linewidth: BASE_LINE_THICKNESS,
    transparent: false,
    opacity: 1,
});

const lineMaterialSecondary = new LineMaterial({
    color: 0xffffff,
    linewidth: BASE_LINE_THICKNESS,
    transparent: true,
    opacity: 0.5,
});

const updateHandMesh = (handMesh: HandMeshData, handData?: HandData) => {
    const meshUpdateData: MeshUpdateData = {
        meshIndices: [],
        positions: [],
    };

    let scale = 1;
    if (handData !== undefined) {
        // Map to range and round to 1dp
        scale = Math.round(MapRangeToRange(handData.fingers[FingerType.TYPE_PINKY].tip.z, 0, 0.1, 1, 3) * 10) / 10;
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
        const newData = updateFingerMesh(
            handMesh.fingers[fingerType],
            scale,
            handData?.fingers[fingerType],
            nextKnucklePos
        );

        meshUpdateData.meshIndices = meshUpdateData.meshIndices.concat(...newData.meshIndices);
        meshUpdateData.positions = meshUpdateData.positions.concat(...newData.positions);
    });

    const wristData = updateWristMesh(
        handMesh.wrist,
        scale,
        handData?.wrist,
        handData?.fingers[FingerType.TYPE_THUMB].knuckle
    );

    meshUpdateData.meshIndices = meshUpdateData.meshIndices.concat(...wristData.meshIndices);
    meshUpdateData.positions = meshUpdateData.positions.concat(...wristData.positions);

    updateCircleMeshes(handMesh.circleMeshes, meshUpdateData, scale);
};

const updateFingerMesh = (
    fingerMesh: FingerMeshData,
    scale: number,
    finger?: FingerData,
    nextKnucklePos?: Vector3
): MeshUpdateData => {
    const data: MeshUpdateData = {
        meshIndices: [],
        positions: [],
    };

    if (finger && nextKnucklePos) {
        const scaleToUse = scale ?? 1;
        data.meshIndices = [fingerMesh.tip, fingerMesh.knuckle];
        data.positions = [finger.tip, finger.knuckle];
        moveLine(fingerMesh.lineToTip, finger.knuckle, finger.tip, scaleToUse);
        moveLine(fingerMesh.lineToNextKnuckle, finger.knuckle, nextKnucklePos, scaleToUse);
        if (!fingerMesh.visible) {
            fingerMesh.lineToTip.visible = true;
            fingerMesh.lineToNextKnuckle.visible = true;
            fingerMesh.visible = true;
        }
    } else if (fingerMesh.visible) {
        fingerMesh.lineToTip.visible = false;
        fingerMesh.lineToNextKnuckle.visible = false;
        fingerMesh.visible = false;
    }

    return data;
};

const updateWristMesh = (
    wristMesh: WristMeshData,
    scale: number,
    wrist?: Vector3,
    thumbKnucklePos?: Vector3
): MeshUpdateData => {
    const data: MeshUpdateData = {
        meshIndices: [],
        positions: [],
    };

    if (wrist && thumbKnucklePos) {
        data.meshIndices = [wristMesh.point];
        data.positions = [wrist];

        moveLine(wristMesh.lineToThumb, wrist, thumbKnucklePos, scale ?? 1);
        if (!wristMesh.visible) {
            wristMesh.lineToThumb.visible = true;
            wristMesh.visible = true;
        }
    } else if (wristMesh.visible) {
        wristMesh.lineToThumb.visible = false;
        wristMesh.visible = false;
    }

    return data;
};

const moveLine = (line: Line2, start: Vector3, end: Vector3, scale: number) => {
    line.geometry.setPositions([start.x, start.y, start.z, end.x, end.y, end.z]);
    line.material.linewidth = BASE_LINE_THICKNESS * scale;
    line.material.needsUpdate = true;
};

const updateCircleMeshes = (circleMeshes: InstancedMesh, updateData: MeshUpdateData, scale: number) => {
    if (updateData.positions.length > 0) {
        updateData.positions.forEach((position, index) => {
            dummy.position.set(position.x / scale, position.y / scale, position.z + 0.001);
            dummy.updateMatrix();
            circleMeshes.setMatrixAt(updateData.meshIndices[index], dummy.matrix);
        });

        circleMeshes.scale.set(scale, scale, 1);
        circleMeshes.instanceMatrix.needsUpdate = true;

        if (!circleMeshes.visible) {
            circleMeshes.visible = true;
        }
    } else if (circleMeshes.visible) {
        circleMeshes.visible = false;
    }
};
