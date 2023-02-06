import { open } from '@tauri-apps/api/shell';

export const openDir = async (dirPath: string) => {
    try {
        await open(dirPath);
    } catch {
        console.warn('No access to file system');
    }
};
