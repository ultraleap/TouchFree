import { open } from '@tauri-apps/api/shell';
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
