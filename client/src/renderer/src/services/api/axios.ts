import axios, { AxiosError, InternalAxiosRequestConfig } from "axios";

const BASE_URL = "https://localhost:7126/api";

// Extends the standard config to include our custom '_retry' flag
// This prevents the interceptor from creating an infinite loop
interface CustomAxiosRequestConfig extends InternalAxiosRequestConfig {
    _retry?: boolean;
}

export const axiosInstance = axios.create({
    baseURL: BASE_URL,
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json'
    },
    // CRITICAL: This tells the browser to include your HttpOnly cookies (Tokens)
    // in every request automatically.
    withCredentials: true 
});

const publicEndpoints = ['/user/login', '/user/register', '/auth/refresh'];

// Response Interceptor
axiosInstance.interceptors.response.use(
    (response) => {
        return response;
    },
    async (error: AxiosError) => {
        const originalRequest = error.config as CustomAxiosRequestConfig;
        const url = originalRequest.url;

        // NEW CHECK: If the request is a public/auth endpoint, DO NOT try to refresh.
        // We let the specific API call (users.ts) handle the 401 response.
        if (url && publicEndpoints.some(endpoint => url.endsWith(endpoint))) {
             return Promise.reject(error);
        }

        // Check if the error is 401 (Unauthorized) AND if we haven't already tried to refresh
        if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
            
            // ... (Rest of the silent refresh logic remains the same) ...

            originalRequest._retry = true;
            try {
                // 1. Attempt to refresh the token
                await axiosInstance.post('/auth/refresh'); 

                // 2. Retry the original request
                return axiosInstance(originalRequest);
                
            } catch (refreshError) {
                // 3. Force logout if refresh fails
                localStorage.removeItem('user-storage'); 
                window.location.href = '/login';
                
                return Promise.reject(refreshError);
            }
        }
        return Promise.reject(error);
    }
);

export default axiosInstance;