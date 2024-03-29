import path from 'path';
import { defineConfig } from 'vite';

import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@': path.join(__dirname, 'src'),
        },
    },
    server: {
        port: 3000,
        fs: {
            allow: [
                './', // Include this folder (default)
                '../node_modules/', // Allow any module dependencies
                '../TF_Tooling_Web/', // Allow serving locally linked tooling
            ],
        },
    },
    base: '/settings/',
});
