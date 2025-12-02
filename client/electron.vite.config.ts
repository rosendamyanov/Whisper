import { resolve } from 'path'
import { defineConfig, externalizeDepsPlugin } from 'electron-vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
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
      tailwindcss()
    ]
  }
})
