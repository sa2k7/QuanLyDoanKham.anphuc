import { useAuthStore } from '@/stores/auth'

export function setupRouterGuards(router) {
    router.beforeEach((to, from, next) => {
        const auth = useAuthStore()

        // 1. Route cho guest (login page) — đã đăng nhập thì về Dashboard
        if (to.meta.guest) {
            if (auth.isLoggedIn) return next({ name: 'Dashboard' })
            return next()
        }

        // 2. Route yêu cầu đăng nhập
        if (to.meta.requiresAuth) {
            if (!auth.isLoggedIn) {
                return next({ name: 'Login', query: { redirect: to.fullPath } })
            }

            // 3. Admin bypass tất cả
            if (auth.isAdmin) return next()

            // 4. Kiểm tra permission key (ưu tiên hơn roles)
            if (to.meta.permission) {
                if (!auth.hasPermission(to.meta.permission)) {
                    return next({ name: 'Forbidden' })
                }
                return next()
            }

            // 5. Kiểm tra roles (backward compat)
            if (to.meta.roles && to.meta.roles.length > 0) {
                const hasAccess = to.meta.roles.some(role => auth.hasRole(role))
                if (!hasAccess) {
                    return next({ name: 'Forbidden' })
                }
            }

            return next()
        }

        // 6. Routes không yêu cầu auth (CheckIn QR, Forbidden, NotFound)
        next()
    })
}
