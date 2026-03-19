import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useI18nStore = defineStore('i18n', () => {
    const locale = ref(localStorage.getItem('locale') || 'vi')

    const messages = {
        vi: {
            nav: {
                dashboard: 'Tổng quan',
                companies: 'Đối tác',
                contracts: 'Hợp đồng',
                staff: 'Nhân sự',
                supplies: 'Kho vật tư',
                accounts: 'Tài khoản',
                logout: 'Đăng xuất'
            },
            common: {
                save: 'Lưu thay đổi',
                cancel: 'Hủy bỏ',
                delete: 'Xóa',
                edit: 'Chỉnh sửa',
                add: 'Thêm mới',
                search: 'Tìm kiếm...',
                loading: 'Đang tải...',
                success: 'Thành công',
                error: 'Thất bại',
                searchPlaceholder: 'Tìm kiếm thông minh (Ctrl+K)...'
            },
            staff: {
                title: 'Quản lý Nhân sự',
                employeeCode: 'Mã NV',
                fullName: 'Họ và Tên',
                jobTitle: 'Chức danh',
                baseSalary: 'Lương cơ bản',
                status: 'Trạng thái',
                estimatedCompensation: 'Thù lao tạm tính',
                systemRole: 'Vai trò hệ thống'
            },
            roles: {
                Admin: 'Quản trị hệ thống',
                PersonnelManager: 'Quản lý Nhân sự',
                ContractManager: 'Quản lý Hợp đồng',
                MedicalGroupManager: 'Quản lý đoàn khám',
                WarehouseManager: 'Quản lý Kho',
                PayrollManager: 'Quản lý tính lương',
                MedicalStaff: 'Nhân sự đi đoàn',
                Customer: 'Tài khoản đối tác'
            }
        },
        en: {
            nav: {
                dashboard: 'Dashboard',
                companies: 'Companies',
                contracts: 'Contracts',
                staff: 'Staff',
                supplies: 'Supplies',
                accounts: 'Accounts',
                logout: 'Logout'
            },
            common: {
                save: 'Save Changes',
                cancel: 'Cancel',
                delete: 'Delete',
                edit: 'Edit',
                add: 'Add New',
                search: 'Search...',
                loading: 'Loading...',
                success: 'Success',
                error: 'Error',
                searchPlaceholder: 'Smart Search (Ctrl+K)...'
            },
            staff: {
                title: 'Staff Management',
                employeeCode: 'Emp ID',
                fullName: 'Full Name',
                jobTitle: 'Job Title',
                baseSalary: 'Base Salary',
                status: 'Status',
                estimatedCompensation: 'Est. Compensation',
                systemRole: 'System Role'
            },
            roles: {
                Admin: 'Admin',
                PersonnelManager: 'HR Manager',
                ContractManager: 'Sales Manager',
                MedicalGroupManager: 'Operations',
                WarehouseManager: 'Stock Manager',
                PayrollManager: 'Accounting',
                MedicalStaff: 'Medical Team',
                Customer: 'Partner'
            }
        }
    }

    const setLocale = (newLocale) => {
        locale.value = newLocale
        localStorage.setItem('locale', newLocale)
    }

    const t = (path) => {
        const keys = path.split('.')
        let result = messages[locale.value]
        for (const key of keys) {
            if (result[key]) result = result[key]
            else return path
        }
        return result
    }

    return { locale, setLocale, t }
})
