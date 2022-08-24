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
        cameraBuffer = new ArrayBuffer(width * lensHeight);
    }
    const buf8 = new Uint8Array(cameraBuffer);
    const buf32 = new Uint32Array(cameraBuffer);

    const rotated90 = dim2 < dim1;

    if (rotated90) {
        processRotatedScreen(data, lens, buf32, conversionArrayToUse, offset, width, lensHeight);
    } else {
        processScreen(data, lens, buf32, conversionArrayToUse, offset, width, lensHeight);
    }

    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : ((lensHeight / 2 - 1) * width) / 2;
    buf32.fill(0xff000000, startOffset, startOffset + width);

    cameraTexture = new DataTexture(buf8, width / 2, lensHeight / 2);
    cameraTexture.flipY = true;
    cameraFeedPlane.material.color.setHex(0xffffff);
    cameraFeedPlane.material.map = cameraTexture;
    cameraFeedPlane.material.needsUpdate = true;
    cameraTexture.needsUpdate = true;
    renderer.render(scene, camera);
};

export const setupRenderScene = (div: HTMLDivElement) => {
    scene = new Scene();
    camera = new PerspectiveCamera(50, 1, 0.1, 1000);
    camera.position.z = 5;

    renderer = new WebGLRenderer();
    renderer.setSize(div.clientWidth, div.clientHeight);
    div.appendChild(renderer.domElement);

    cameraFeedPlane = new Mesh(new PlaneGeometry(4.6, 4.6), new MeshBasicMaterial({ color: 0x000000 }));
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
    const resultWidth = width / 2;
    const resultHeight = lensHeight / 2;

    for (let rowIndex = 0; rowIndex < resultWidth; rowIndex++) {
        const resultStartIndex = rowBase / 8;

        if (lens === 'Right') {
            for (let i = 0; i < resultHeight; i++) {
                buf32[i + resultStartIndex] = byteConversionArray[offsetView[i * 2 + rowBase]];
            }
        }

        rowBase += width;
        if (lens === 'Left') {
            for (let i = 0; i < resultHeight; i++) {
                buf32[i + resultStartIndex] = byteConversionArray[offsetView[i * 2 + rowBase]];
            }
        }

        rowBase += width;

        // Skip entire row
        rowBase += width * 2;
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

    for (let i = 0; i < width / 2; i++) {
        for (let j = 0; j < lensHeight / 2; j++) {
            buf32[(i * lensHeight) / 2 + j] = byteConversionArray[offsetView[i * lensHeight * 2 + j * 2]];
        }
    }
};
