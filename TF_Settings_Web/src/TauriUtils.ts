import { open as openDialog } from '@tauri-apps/api/dialog';
import { open } from '@tauri-apps/api/shell';
import { invoke } from '@tauri-apps/api/tauri';
import { appWindow } from '@tauri-apps/api/window';

import { VisualsConfig } from './Pages/Visuals/VisualsUtils';

export const isDesktop = () => '__TAURI_METADATA__' in window;

export const toggleFullScreen = async () => {
    const isFullScreen = await appWindow.isFullscreen();
    appWindow.setFullscreen(!isFullScreen);
};

export const openDir = async (dirPath: string) => {
    try {
        await open(dirPath);
    } catch (e) {
        console.warn(e);
    }
};

export const readVisualsConfig = async (): Promise<VisualsConfig> => {
    const rawConfig: string = await invoke('read_file_to_string', {
        path: 'C:/ProgramData/Ultraleap/TouchFree/Configuration/TouchFreeConfig.json',
    });
    return JSON.parse(rawConfig) as VisualsConfig;
};

export const writeVisualsConfig = async (config: VisualsConfig) => {
    invoke('write_string_to_file', {
        path: 'C:/ProgramData/Ultraleap/TouchFree/Configuration/TouchFreeConfig.json',
        contents: JSON.stringify(config, null, 4),
    });
};

export const openFilePicker = async (extensions: string[]): Promise<string | string[] | null> =>
    await openDialog({ directory: false, multiple: false, filters: [{ name: 'filter', extensions }] });
