import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import mkcert from 'vite-plugin-mkcert';

// https://vitejs.dev/config/
export default defineConfig(({ command }) => {
    const apiPort = '5279';
    const apiTarget = `http://localhost:${apiPort}`;
    const clientPort = 5173;

    if (command === 'serve') {
        return {
            base: '/',
            plugins: [react(), mkcert()],
            server: {
                proxy: {
                    '/api': {
                        target: apiTarget,
                        changeOrigin: true,
                        secure: false,
                        headers: {
                            'Host-Name': 'PriceInsight',
                        },
                    },
                },
                https: true,
                port: clientPort,
            },
        };
    } else {
        return {
            base: '/',
            plugins: [react()],
            build: {
                outDir: 'dist',
                emptyOutDir: true,
                sourcemap: false,
                manifest: true,
                rollupOptions: {
                    output: {
                        manualChunks: {
                            vendor: ['react', 'react-dom'],
                        },
                    },
                },
            },
        };
    }
});
