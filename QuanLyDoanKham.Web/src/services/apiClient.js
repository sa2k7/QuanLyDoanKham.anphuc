/**
 * apiClient.js - Axios instance chuẩn hóa cho toàn bộ Frontend
 * Tự động đính kèm JWT token và xử lý lỗi 401 (refresh token / redirect login)
 */
import axios from 'axios'

const BASE_URL = import.meta.env.VITE_API_BASE_URL || ''

const apiClient = axios.create({
    baseURL: BASE_URL,
    timeout: 15000,
    headers: {
        'Content-Type': 'application/json',
    },
})

// ── Request Interceptor ──────────────────────────────────────────────────────
// Tự động thêm Authorization header từ localStorage
apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('auth_token')
        if (token) {
            config.headers.Authorization = `Bearer ${token}`
        }
        return config
    },
    (error) => Promise.reject(error)
)

// ── Response Interceptor ─────────────────────────────────────────────────────
// Bắt lỗi 401 → thử refresh token → nếu thất bại → redirect /login
let isRefreshing = false
let failedQueue = []

const processQueue = (error, token = null) => {
    failedQueue.forEach((prom) => {
        if (error) {
            prom.reject(error)
        } else {
            prom.resolve(token)
        }
    })
    failedQueue = []
}

apiClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        if (error.response) {
            console.error(`[API Error] ${error.config?.url}: Status ${error.response.status}`, error.response.data);
        } else {
            console.error(`[Network Error] Không thể kết nối tới server`);
        }

        const originalRequest = error.config

        if (error.response?.status === 401 && !originalRequest._retry) {
            if (isRefreshing) {
                // Xếp hàng các request đang chờ khi đang refresh
                return new Promise((resolve, reject) => {
                    failedQueue.push({ resolve, reject })
                })
                    .then((token) => {
                        originalRequest.headers.Authorization = `Bearer ${token}`
                        return apiClient(originalRequest)
                    })
                    .catch((err) => Promise.reject(err))
            }

            originalRequest._retry = true
            isRefreshing = true

            const refreshToken = localStorage.getItem('auth_refresh_token')
            if (!refreshToken) {
                isRefreshing = false
                redirectToLogin()
                return Promise.reject(error)
            }

            try {
                const res = await axios.post(`${BASE_URL}/api/Auth/refresh-token`, {
                    refreshToken,
                })
                const newToken = res.data.token
                localStorage.setItem('auth_token', newToken)
                apiClient.defaults.headers.common.Authorization = `Bearer ${newToken}`
                processQueue(null, newToken)
                originalRequest.headers.Authorization = `Bearer ${newToken}`
                return apiClient(originalRequest)
            } catch (refreshError) {
                processQueue(refreshError, null)
                redirectToLogin()
                return Promise.reject(refreshError)
            } finally {
                isRefreshing = false
            }
        }

        return Promise.reject(error)
    }
)

function redirectToLogin() {
    localStorage.removeItem('auth_token')
    localStorage.removeItem('auth_refresh_token')
    // Tránh redirect loop nếu đang ở trang login
    if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login'
    }
}

export default apiClient

// ── Convenience re-exports ───────────────────────────────────────────────────
// Dùng như thay thế cho axios trực tiếp:
// import { api } from '@/services/apiClient'
export const api = apiClient
