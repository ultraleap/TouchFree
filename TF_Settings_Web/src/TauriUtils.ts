import { open } from '@tauri-apps/api/shell';
import { invoke } from '@tauri-apps/api/tauri';
import { appWindow } from '@tauri-apps/api/window';

export const isDesktop = () => '__TAURI_METADATA__' in window;

export const toggleFullScreen = async () => {
    const isFullScreen = await appWindow.isFullscreen();
    appWindow.setFullscreen(!isFullScreen);
};

export const openDir = async (dirPath: string) => {
    try {
        await open(dirPath);
    } catch {
        console.warn('No access to file system');
    }
};

export const readVisualsConfig = async () => {
    try {
        const x = await invoke('read_visuals_config');
        console.log(x);
    } catch (e) {
        console.warn(e);
    }
};
