import { ApiResponse } from '@renderer/types/api/response/api/ApiResponse'
import { AuthResponse } from '@renderer/types/api/response/auth/AuthResponse'
import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios'

const BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7126/api'

interface CustomAxiosRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
}

interface QueueItem {
  resolve: (value?: unknown) => void
  reject: (reason?: unknown) => void
}

export const axiosInstance = axios.create({
  baseURL: BASE_URL,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true
})

const publicEndpoints = ['/user/login', '/user/register', '/auth/refresh']

let isRefreshing = false
let failedQueue: QueueItem[] = []

const processQueue = (error: unknown, token: string | null = null): void => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token)
    }
  })

  failedQueue = []
}

axiosInstance.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as CustomAxiosRequestConfig
    const url = originalRequest?.url

    if (url && publicEndpoints.some((endpoint) => url.endsWith(endpoint))) {
      return Promise.reject(error)
    }

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      if (isRefreshing) {
        console.log('⏳ Another request is refreshing. Queueing this one...')
        return new Promise(function (resolve, reject) {
          failedQueue.push({ resolve, reject })
        })
          .then(() => {
            return axiosInstance(originalRequest)
          })
          .catch((err) => {
            return Promise.reject(err)
          })
      }

      originalRequest._retry = true
      isRefreshing = true
      console.log('🔄 Starting Refresh Process...')

      try {
        const refreshResponse = await axios.post<ApiResponse<AuthResponse>>(
          `${BASE_URL}/auth/refresh`,
          {},
          {
            withCredentials: true,
            headers: { 'Content-Type': 'application/json' }
          }
        )

        if (refreshResponse.data && refreshResponse.data.isSuccess) {
          console.log('✅ Refresh Success! Resuming queue.')

          processQueue(null)
          isRefreshing = false

          console.log('🚀 Retrying original request')
          return axiosInstance(originalRequest)
        } else {
          throw new Error('Refresh failed logic')
        }
      } catch (refreshError) {
        console.error('💀 Refresh Failed:', refreshError)

        processQueue(refreshError, null)
        isRefreshing = false

        return Promise.reject(refreshError)
      }
    }
    return Promise.reject(error)
  }
)

export default axiosInstance
