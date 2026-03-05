import { useAuthStore } from '../stores/auth'

export function setupRouterGuards(router) {
    router.beforeEach(async (to, from, next) => {
        const authStore = useAuthStore()

        // Đảm bảo trạng thái xác thực được kiểm tra với Backend ít nhất một lần khi khởi động
        if (!authStore.isInitialized && authStore.token) {
            await authStore.checkAuth()
        }

        // If already logged in and trying to access login page, redirect to dashboard
        if (to.path === '/login' && authStore.isAuthenticated) {
            return next('/')
        }

        // Check if route requires authentication
        if (to.meta.requiresAuth && !authStore.isAuthenticated) {
            return next('/login')
        }

        // Check if route requires specific role
        if (to.meta.roles && to.meta.roles.length > 0) {
            const userRole = authStore.role
            if (!to.meta.roles.includes(userRole)) {
                // Redirect to forbidden page if user doesn't have required role
                return next('/forbidden')
            }
        }

        next()
    })
}
