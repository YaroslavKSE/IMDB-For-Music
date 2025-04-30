import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // User service endpoints
      '/api/v1/users': {
        target: 'http://localhost:5001',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/v1\/users/, '/api/v1')
      },
      '/auth': {
        target: 'http://localhost:5001',
        changeOrigin: true,
      },
      '/users/subscriptions':{
        target: 'http://localhost:5001',
        changeOrigin: true,
      },
      '/public/users':{
        target: 'http://localhost:5001',
        changeOrigin: true,
      },
      // Catalog service endpoints
      '/api/v1/catalog': {
        target: 'http://localhost:5002',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/v1\/catalog/, '/api/v1')
      },

      // ItemHistory and Grading service endpoints
      '/api/v1/interactions': {
        target: 'http://localhost:5003',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/v1\/interactions/, '/api/v1')
      },
      '/api/v1/grading-methods': {
        target: 'http://localhost:5003',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/v1\/grading-methods/, '/api')
      },

      // Spotify preview proxy (for audio previews)
      '/spotify': {
        target: 'https://open.spotify.com',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/spotify/, '')
      }
    },
  },
});