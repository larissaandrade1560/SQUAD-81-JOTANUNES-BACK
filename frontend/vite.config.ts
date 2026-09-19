import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const apiUrl = process.env.VITE_API_URL?.trim()
  if (mode === 'production' && !apiUrl) {
    throw new Error('VITE_API_URL must be set for production builds (Render API base URL).')
  }

  return {
    plugins: [react()],
  }
})
