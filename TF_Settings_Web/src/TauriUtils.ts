import { open as openDialog } from '@tauri-apps/api/dialog';
import { open } from '@tauri-apps/api/shell';
import { invoke } from '@tauri-apps/api/tauri';
import { appWindow } from '@tauri-apps/api/window';

import { VisualsConfig } from '@/Pages/Visuals/VisualsUtils';

// If we are running in a tauri environment (i.e. are a desktop app) then the
// window will have the __TAURI_METADATA__ property
export const isDesktop = () => '__TAURI_METADATA__' in window;

export const toggleFullScreen = async () => {
    const isFullScreen = await appWindow.isFullscreen();
    appWindow.setFullscreen(!isFullScreen);
};

export const closeWindow = async () => {
    appWindow.close();
};

export const minimizeWindow = async () => {
    appWindow.minimize();
};

export const openWithShell = async (path: string, catchCallback?: () => void) => {
    try {
        await open(path);
    } catch (e) {
        catchCallback ? catchCallback() : console.warn(e);
    }
};

export const readVisualsConfig = async (): Promise<VisualsConfig> => {
    const rawConfig: string = await invoke('read_file_to_string', {
        path: 'C:/ProgramData/Ultraleap/TouchFree/Configuration/TouchFreeConfig.json',
    });
    const config = JSON.parse(rawConfig) as VisualsConfig;
    config.ctiFilePath = config.ctiFilePath.replaceAll('\\', '/');
    return config;
};

export const writeVisualsConfig = async (config: VisualsConfig) => {
    invoke('write_string_to_file', {
        path: 'C:/ProgramData/Ultraleap/TouchFree/Configuration/TouchFreeConfig.json',
        contents: JSON.stringify(config, null, 4),
    });
};

export const openFilePicker = async (extensions: string[]): Promise<string | string[] | null> =>
    await openDialog({ directory: false, multiple: false, filters: [{ name: 'filter', extensions }] });
