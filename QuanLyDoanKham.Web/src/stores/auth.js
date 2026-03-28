import { defineStore } from 'pinia'
import axios from 'axios'
import router from '../router'

export const useAuthStore = defineStore('auth', {
    state: () => ({
        user: JSON.parse(localStorage.getItem('user') || sessionStorage.getItem('user')) || null,
        profile: null,
        token: localStorage.getItem('token') || sessionStorage.getItem('token') || null,
        refreshToken: localStorage.getItem('refreshToken') || sessionStorage.getItem('refreshToken') || null,
        loading: false,
        error: null,
        isInitialized: false
    }),
    getters: {
        isAuthenticated: (state) => !!state.token,
        role: (state) => state.user?.role || null,
        companyId: (state) => state.user?.companyId || null,
        currentUser: (state) => state.user?.username || null
    },
    actions: {
        async checkAuth() {
            if (this.token && !this.isInitialized) {
                try {
                    // Xác thực token với Backend
                    axios.defaults.headers.common['Authorization'] = `Bearer ${this.token}`
                    const res = await axios.get('/api/auth/profile')
                    this.profile = res.data
                    this.user = {
                        username: res.data.username,
                        role: res.data.roleName,
                        companyId: res.data.companyId
                    }
                    
                    // Cập nhật lại user info vào đúng bộ nhớ đang dùng
                    if (localStorage.getItem('token')) {
                        localStorage.setItem('user', JSON.stringify(this.user))
                    } else {
                        sessionStorage.setItem('user', JSON.stringify(this.user))
                    }

                    this.isInitialized = true
                    return true
                } catch (e) {
                    console.warn("Token verify failed or expired", e)
                    this.logout()
                    return false
                }
            }
            this.isInitialized = true
            return !!this.token
        },
        async fetchProfile() {
            try {
                const res = await axios.get('/api/auth/profile')
                this.profile = res.data
                return res.data
            } catch (e) {
                console.error("Fetch profile failed", e)
            }
        },
        async login(username, password, rememberMe = false) {
            this.loading = true
            this.error = null
            try {
                const response = await axios.post('/api/auth/login', {
                    username,
                    password
                })
                this.setData(response.data, rememberMe)
                return true
            } catch (err) {
                this.error = err.response?.data || "Đăng nhập thất bại"
                return false
            } finally {
                this.loading = false
            }
        },
        setData(data, rememberMe = false) {
            this.token = data.token
            this.refreshToken = data.refreshToken
            this.user = {
                username: data.username,
                role: data.role,
                companyId: data.companyId
            }

            const storage = rememberMe ? localStorage : sessionStorage
            
            // Xóa sạch ở cả 2 trước khi set mới để tránh xung đột
            this.clearStorage()

            storage.setItem('token', this.token)
            storage.setItem('refreshToken', this.refreshToken)
            storage.setItem('user', JSON.stringify(this.user))
            
            axios.defaults.headers.common['Authorization'] = `Bearer ${this.token}`
        },
        clearStorage() {
            localStorage.removeItem('token')
            localStorage.removeItem('refreshToken')
            localStorage.removeItem('user')
            sessionStorage.removeItem('token')
            sessionStorage.removeItem('refreshToken')
            sessionStorage.removeItem('user')
        },
        async refresh() {
            try {
                const res = await axios.post('/api/auth/refresh-token', {
                    refreshToken: this.refreshToken
                })
                // Kiểm tra xem đang dùng storage nào để lưu tiếp vào đó
                const isPersistent = !!localStorage.getItem('refreshToken')
                this.setData(res.data, isPersistent)
                return res.data.token
            } catch (e) {
                this.logout()
                return null
            }
        },
        logout() {
            this.user = null
            this.token = null
            this.refreshToken = null
            this.profile = null
            this.clearStorage()
            delete axios.defaults.headers.common['Authorization']
            router.push('/login')
        }
    }
})

// Setup Axios Interceptor
axios.interceptors.response.use(
    response => response,
    async error => {
        const originalRequest = error.config

        // Tránh loop vô tận khi refresh token cũng fail
        if (error.response?.status === 401 && !originalRequest._retry && !originalRequest.url.includes('refresh-token')) {
            originalRequest._retry = true
            const authStore = useAuthStore()
            if (authStore.refreshToken) {
                const newToken = await authStore.refresh()
                if (newToken) {
                    originalRequest.headers['Authorization'] = `Bearer ${newToken}`
                    return axios(originalRequest)
                }
            }
        }

        if (error.response?.status === 403) {
            const currentPath = router.currentRoute.value.path
            if (currentPath !== '/forbidden') {
                router.push('/forbidden')
            }
        }

        return Promise.reject(error)
    }
)

// Initial setup - Chạy khi file được nạp lần đầu
const token = localStorage.getItem('token') || sessionStorage.getItem('token')
if (token) {
    axios.defaults.headers.common['Authorization'] = `Bearer ${token}`
}

