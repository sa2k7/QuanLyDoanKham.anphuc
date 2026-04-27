import { createRouter, createWebHistory } from 'vue-router'
import Login from '../views/Login.vue'
import { setupRouterGuards } from './guards'

const routes = [
    // ── Public ──────────────────────────────────────────────────────
    {
        path: '/landing-page',
        name: 'LandingPage',
        component: () => import('../views/LandingPage.vue'),
        meta: { guest: true }
    },
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
        component: () => import('../views/CheckIn.vue')
    },
    {
        path: '/patient-checkin',
        name: 'PatientCheckIn',
        component: () => import('../views/PatientSelfCheckIn.vue')
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
        component: () => import('../views/UnifiedReports.vue'),
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

    // ── Bệnh nhân ────────────────────────────────────────────────────
    {
        path: '/patients',
        name: 'Patients',
        component: () => import('../views/Patients.vue'),
        meta: { requiresAuth: true, permission: 'DoanKham.View' }
    },

    // ── QC ───────────────────────────────────────────────────────────
    {
        path: '/qc',
        name: 'QcReview',
        component: () => import('../views/oms/QcReview.vue'), // Keep in oms folder for now or move later
        meta: { requiresAuth: true, permission: 'KetQua.QCApprove' }
    },


    // ── Quyết toán ────────────────────────────────────────────────
    {
        path: '/settlement-report',
        name: 'SettlementReport',
        component: () => import('../views/SettlementReport.vue'),
        meta: { requiresAuth: true, permission: 'BaoCao.View' }
    },
    {
        path: '/master-stats',
        name: 'MasterStatsDashboard',
        component: () => import('../views/MasterStatsDashboard.vue'),
        meta: {
            title: 'Thống Kê Tổng Hợp',
            permission: 'Reports.View' 
        }
    },

    // ── Nhân sự ──────────────────────────────────────────────────────
    {
        path: '/staff',
        name: 'Staff',
        component: () => import('../views/Staff.vue'),
        meta: { requiresAuth: true, permission: 'NhanSu.View' }
    },
    {
        path: '/payroll',
        name: 'Payroll',
        component: () => import('../views/Payroll.vue'),
        meta: { requiresAuth: true, permission: 'Luong.View' }
    },
    {
        path: '/attendance-summary',
        name: 'AttendanceSummary',
        component: () => import('../views/AttendanceSummary.vue'),
        meta: { requiresAuth: true, permission: 'ChamCong.ViewAll' }
    },
    {
        path: '/supplies',
        name: 'Supplies',
        component: () => import('../views/Supplies.vue'),
        meta: { requiresAuth: true, permission: 'Kho.View' }
    },



    // ── Báo cáo ──────────────────────────────────────────────────────
    {
        path: '/reports',
        name: 'Reports',
        component: () => import('../views/UnifiedReports.vue'),
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
    {
        path: '/audit-logs',
        name: 'AuditLogs',
        component: () => import('../views/AuditLogView.vue'),
        meta: {
            requiresAuth: true,
            title: 'Nhật Ký Thao Tác',
            permission: 'HeThong.AuditLog'
        }
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
