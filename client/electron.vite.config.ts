import { resolve } from 'path'
import { defineConfig, externalizeDepsPlugin } from 'electron-vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { loadEnv } from 'vite' // Import this to read your .env file

export default defineConfig(({ mode }) => {
  // 1. Load env variables based on the current mode
  const env = loadEnv(mode, process.cwd())
  
  // 2. Get the API URL from .env, or default to localhost
  const fullApiUrl = env.VITE_API_URL || 'https://localhost:7126/api'

  // 3. Create the clean URLs needed for CSP (Remove '/api' and create 'wss://')
  const hostUrl = fullApiUrl.replace(/\/api$/, '')     // e.g. https://192.168.1.15:7126
  const wssUrl = hostUrl.replace(/^https/, 'wss')      // e.g. wss://192.168.1.15:7126

  return {
    main: {
      plugins: [externalizeDepsPlugin()]
    },
    preload: {
      plugins: [externalizeDepsPlugin()]
    },
    renderer: {
      resolve: {
        alias: {
          '@renderer': resolve('src/renderer/src'),
          '@components': resolve('src/renderer/src/components'),
          '@pages': resolve('src/renderer/src/pages'),
          '@services': resolve('src/renderer/src/services'),
          '@store': resolve('src/renderer/src/store'),
          '@hooks': resolve('src/renderer/src/hooks'),
          '@types': resolve('src/renderer/src/types'),
          '@utils': resolve('src/renderer/src/utils')
        }
      },
      plugins: [
        react(),
        tailwindcss(),
        // --- THE TRICK ---
        {
          name: 'inject-csp',
          transformIndexHtml(html) {
            // This defines the safe sources: Localhost + Your Public IP
            const cspSources = `self http://localhost:5256 https://localhost:7126 wss://localhost:7126 ${hostUrl} ${wssUrl}`
            
            // Replace the placeholder in index.html with our calculated sources
            return html.replace('%CSP_CONNECT_SOURCES%', cspSources)
          }
        }
      ]
    }
  }
})