import { open } from '@tauri-apps/api/shell';
import { invoke } from '@tauri-apps/api/tauri';

export const openDir = async (dirPath: string) => {
    try {
        await open(dirPath);
    } catch (e) {
        console.warn(e);

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
