import { createRouter, createWebHistory } from 'vue-router'
import Login from '../views/Login.vue'
import { setupRouterGuards } from './guards'

const routes = [
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
    {
        path: '/',
        name: 'Dashboard',
        component: () => import('../views/Dashboard.vue'),
        meta: { requiresAuth: true }
    },
    {
        path: '/companies',
        name: 'Companies',
        component: () => import('../views/Companies.vue'),
        meta: { requiresAuth: true, roles: ['Admin', 'ContractManager'] }
    },
    {
        path: '/contracts',
        name: 'Contracts',
        component: () => import('../views/Contracts.vue'),
        meta: { requiresAuth: true, roles: ['Admin', 'ContractManager', 'Customer'] }
    },
    {
        path: '/groups',
        name: 'Groups',
        component: () => import('../views/Groups.vue'),
        meta: { requiresAuth: true, roles: ['Admin', 'MedicalGroupManager', 'MedicalStaff', 'Customer'] }
    },
    {
        path: '/staff',
        name: 'Staff',
        component: () => import('../views/Staff.vue'),
        meta: { requiresAuth: true, roles: ['Admin', 'PersonnelManager', 'MedicalGroupManager'] }
    },
    {
        path: '/supplies',
        name: 'Supplies',
        component: () => import('../views/Supplies.vue'),
        meta: { requiresAuth: true, roles: ['Admin', 'WarehouseManager'] }
    },
    {
        path: '/users',
        name: 'Users',
        component: () => import('../views/Users.vue'),
        meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
        path: '/payroll',
        name: 'Payroll',
        component: () => import('../views/Payroll.vue'),
        meta: { requiresAuth: true, roles: ['Admin', 'PayrollManager'] }
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

// Apply navigation guards
setupRouterGuards(router)

export default router
