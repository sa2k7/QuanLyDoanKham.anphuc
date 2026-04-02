import { createRouter, createWebHistory } from 'vue-router'
import Login from '../views/Login.vue'
import { setupRouterGuards } from './guards'

const routes = [
    // ── Public ──────────────────────────────────────────────────────
    {
        path: '/login',
        name: 'Login',
        component: Login,
        meta: { guest: true }
    },
    {
        path: '/forbidden',
        name: 'Forbidden',
        component: () => import('../views/Forbidden.vue')
    },

    // ── Chấm công QR (public, không cần đăng nhập) ─────────────────
    {
        path: '/checkin',
        name: 'CheckIn',
        component: () => import('../views/CheckIn.vue'),
        meta: { guest: true }
    },

    // ── Dashboard ────────────────────────────────────────────────────
    {
        path: '/',
        name: 'Dashboard',
        component: () => import('../views/Dashboard.vue'),
        meta: { requiresAuth: true }
    },
    {
        path: '/analytics',
        name: 'Analytics',
        component: () => import('../views/AnalyticsDashboard.vue'),
        meta: { requiresAuth: true, permission: 'BaoCao.View' }
    },

    // ── Đối tác ──────────────────────────────────────────────────────
    {
        path: '/companies',
        name: 'Companies',
        component: () => import('../views/Companies.vue'),
        meta: { requiresAuth: true, roles: ['Admin', 'ContractManager', 'PersonnelManager'] }
    },

    // ── Hợp đồng ────────────────────────────────────────────────────
    {
        path: '/contracts',
        name: 'Contracts',
        component: () => import('../views/Contracts.vue'),
        meta: { requiresAuth: true, permission: 'HopDong.View' }
    },

    // ── Đoàn khám ────────────────────────────────────────────────────
    {
        path: '/groups',
        name: 'Groups',
        component: () => import('../views/Groups.vue'),
        meta: { requiresAuth: true, permission: 'DoanKham.View' }
    },

    // ── Nhân sự ──────────────────────────────────────────────────────
    {
        path: '/staff',
        name: 'Staff',
        component: () => import('../views/Staff.vue'),
        meta: { requiresAuth: true, permission: 'NhanSu.View' }
    },

    // ── Phòng ban ────────────────────────────────────────────────────
    {
        path: '/departments',
        name: 'Departments',
        component: () => import('../views/Departments.vue'),
        meta: { requiresAuth: true, roles: ['Admin', 'PersonnelManager'] }
    },

    // ── Kho vật tư ──────────────────────────────────────────────────
    {
        path: '/supplies',
        name: 'Supplies',
        component: () => import('../views/Supplies.vue'),
        meta: { requiresAuth: true, permission: 'Kho.View' }
    },

    // ── Lương ────────────────────────────────────────────────────────
    {
        path: '/payroll',
        name: 'Payroll',
        component: () => import('../views/Payroll.vue'),
        meta: { requiresAuth: true, permission: 'Luong.View' }
    },

    // ── Báo cáo ──────────────────────────────────────────────────────
    {
        path: '/reports',
        name: 'Reports',
        component: () => import('../views/Reports.vue'),
        meta: { requiresAuth: true, permission: 'BaoCao.View' }
    },

    // ── Quản trị hệ thống ────────────────────────────────────────────
    {
        path: '/users',
        name: 'Users',
        component: () => import('../views/Users.vue'),
        meta: { requiresAuth: true, permission: 'HeThong.UserManage' }
    },
    {
        path: '/permissions',
        name: 'Permissions',
        component: () => import('../views/Permissions.vue'),
        meta: { requiresAuth: true, permission: 'HeThong.RoleManage' }
    },

    // ── Lịch cá nhân ────────────────────────────────────────────────
    {
        path: '/my-schedule',
        name: 'MySchedule',
        component: () => import('../views/MySchedule.vue'),
        meta: { requiresAuth: true, permission: 'LichKham.ViewOwn' }
    },

    // ── 404 ─────────────────────────────────────────────────────────
    {
        path: '/:pathMatch(.*)*',
        name: 'NotFound',
        component: () => import('../views/NotFound.vue')
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

setupRouterGuards(router)

export default router
