import {
    BufferGeometry,
    DataTexture,
    Mesh,
    MeshBasicMaterial,
    PerspectiveCamera,
    PlaneGeometry,
    Scene,
    WebGLRenderer,
} from 'three';

import { Lens } from './MaskingScreen';

let conversionArraysInitialised = false;
const byteConversionArray = new Uint32Array(256);
const byteConversionArrayOverExposed = new Uint32Array(256);
let cameraBuffer: ArrayBuffer;

let scene: Scene;
let renderer: WebGLRenderer;
let camera: PerspectiveCamera;
let cameraFeedPlane: Mesh<BufferGeometry, MeshBasicMaterial>;
let cameraTexture: DataTexture;

export const updateCanvas = (
    data: ArrayBuffer,
    lens: Lens,
    isCameraReversed: boolean,
    showOverexposedAreas: boolean
) => {
    if (!conversionArraysInitialised) {
        for (let i = 0; i < 256; i++) {
            byteConversionArray[i] = (255 << 24) | (i << 16) | (i << 8) | i;
            // -13434625 = #FFFF0033 in signed 2's complement
            byteConversionArrayOverExposed[i] = i > 128 ? -13434625 : byteConversionArray[i];
        }
        conversionArraysInitialised = true;
    }

    const conversionArrayToUse = showOverexposedAreas ? byteConversionArrayOverExposed : byteConversionArray;

    const offset = 9;
    const startOfBuffer = new DataView(data, 1, offset);

    const dim1 = startOfBuffer.getUint32(0);
    const dim2 = startOfBuffer.getUint32(4);

    const width = Math.min(dim1, dim2);
    const lensHeight = Math.max(dim1, dim2) / 2;

    if (!cameraBuffer) {
        cameraBuffer = new ArrayBuffer(width * lensHeight * 4);
    }
    const buf32 = new Uint32Array(cameraBuffer);

    const rotated90 = dim2 < dim1;

    if (rotated90) {
        processRotatedScreen(data, lens, buf32, conversionArrayToUse, offset, width, lensHeight);
    } else {
        processScreen(data, lens, buf32, conversionArrayToUse, offset, width, lensHeight);
    }

    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : (lensHeight - 1) * width;
    buf32.fill(0xff000000, startOffset, startOffset + width);

    if (!cameraTexture) {
        cameraTexture = new DataTexture(new Uint8Array(cameraBuffer), width, lensHeight);
        cameraTexture.flipY = true;
    } else {
        cameraTexture.image = new ImageData(new Uint8ClampedArray(cameraBuffer), width, lensHeight);
    }
    cameraFeedPlane.material.color.setHex(0xffffff);
    cameraFeedPlane.material.map = cameraTexture;
    cameraFeedPlane.material.needsUpdate = true;
    cameraTexture.needsUpdate = true;
    renderer.render(scene, camera);
};

export const setupRenderScene = (div: HTMLDivElement) => {
    scene = new Scene();
    camera = new PerspectiveCamera(90);
    camera.position.z = 1;

    renderer = new WebGLRenderer();
    renderer.setSize(div.clientWidth, div.clientHeight);
    div.appendChild(renderer.domElement);

    cameraFeedPlane = new Mesh(new PlaneGeometry(2, 2), new MeshBasicMaterial({ color: 0x000000 }));
    console.log(cameraFeedPlane.position.z);
    scene.add(cameraFeedPlane);

    renderer.render(scene, camera);
};

const processRotatedScreen = (
    data: ArrayBuffer,
    lens: Lens,
    buf32: Uint32Array,
    byteConversionArray: Uint32Array,
    offset: number,
    width: number,
    lensHeight: number
) => {
    let rowBase = 0;
    const offsetView = new Uint8Array(data, offset, width * lensHeight * 2);

    if (lens === 'Right') {
        for (let rowIndex = 0; rowIndex < width; rowIndex++) {
            const rowStart = rowBase * 2;

            for (let i = 0; i < lensHeight; i++) {
                buf32[i + rowBase] = byteConversionArray[offsetView[i + rowStart]];
            }
            rowBase += lensHeight;
        }
    } else if (lens === 'Left') {
        for (let rowIndex = 0; rowIndex < width; rowIndex++) {
            const rowStart = rowBase * 2 + lensHeight;

            for (let i = 0; i < lensHeight; i++) {
                buf32[i + rowBase] = byteConversionArray[offsetView[i + rowStart]];
            }
            
            rowBase += lensHeight;
        }
    }
};

const processScreen = (
    data: ArrayBuffer,
    lens: Lens,
    buf32: Uint32Array,
    byteConversionArray: Uint32Array,
    offset: number,
    width: number,
    lensHeight: number
) => {
    const offsetView = new Uint8Array(data, offset + (lens === 'Left' ? width * lensHeight : 0), width * lensHeight);

    for (let i = 0; i < width * lensHeight; i++) {
        buf32[i] = byteConversionArray[offsetView[i]];
    }
};
