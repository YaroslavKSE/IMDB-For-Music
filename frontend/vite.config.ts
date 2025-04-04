import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/auth': {
        target: 'http://localhost:5001',
        changeOrigin: true,
      },
      '/user': {
        target: 'http://localhost:5001',
        changeOrigin: true,
      },
      '/catalog': {
        target: 'http://localhost:5002',
        changeOrigin: true,
      },
      '/rating': {
        target: 'http://localhost:5003',
        changeOrigin: true,
      },
      '/spotify': {
        target: 'https://open.spotify.com',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/spotify/, '')
      }
    },
  },
});