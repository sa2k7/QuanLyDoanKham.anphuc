import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useI18nStore = defineStore('i18n', () => {
    const locale = ref(localStorage.getItem('locale') || 'vi')

    const messages = {
        vi: {
            nav: {
                dashboard: 'Tổng quan',
                companies: 'Quản lý Công ty',
                contracts: 'Hợp đồng & Ký kết',
                groups: 'Quản lý Đoàn khám',
                patients: 'Hồ sơ Bệnh nhân',
                staff: 'Nhân sự & Chấm công',
                payroll: 'Bảng lương Tháng',
                reports: 'Thống kê & Báo cáo',
                accounts: 'Tài khoản Hệ thống',
                permissions: 'Quyền hạn & Bảo mật',
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
                searchPlaceholder: 'Tìm kiếm thông minh (Ctrl+K)...',
                noData: 'Chưa có thông tin',
                stt: 'STT',
                actions: 'Tác vụ'
            },
            companies: {
                title: 'Danh sách Công ty',
                subtitle: 'Quản lý thông tin các công ty và đối tác doanh nghiệp',
                addBtn: 'THÊM CÔNG TY',
                cancelBtn: 'HỦY BỎ',
                formTitle: 'Khai báo Công ty mới',
                formSubtitle: 'Thêm mới thông tin công ty vào hệ thống',
                inputs: {
                    name: 'Tên pháp nhân doanh nghiệp',
                    taxCode: 'Mã số thuế',
                    companyPhone: 'SĐT liên hệ doanh nghiệp',
                    representativePhone: 'SĐT người đại diện',
                    address: 'Địa chỉ trụ sở'
                },
                table: {
                    info: 'THÔNG TIN DOANH NGHIỆP',
                    taxCode: 'MÃ THUẾ',
                    contact: 'THÔNG TIN LIÊN HỆ',
                    address: 'ĐỊA CHỈ'
                }
            },
            staff: {
                title: 'Đội ngũ Nhân sự',
                subtitle: 'Nhân sự: Quản lý hồ sơ y bác sĩ & điều dưỡng',
                addBtn: 'THÊM NHÂN SỰ',
                cancelBtn: 'HỦY BỎ',
                formTitle: 'Khai báo Nhân sự',
                formSubtitle: 'Tạo mới hồ sơ nhân sự vào hệ thống',
                inputs: {
                    name: 'Họ và tên nhân sự',
                    salary: 'Mức lương cơ bản',
                    role: 'Vai trò hệ thống'
                },
                table: {
                    info: 'NHÂN SỰ',
                    title: 'CHỨC DANH / VAI TRÒ',
                    salary: 'LƯƠNG CƠ BẢN',
                    comp: 'THÙ LAO TẠM TÍNH'
                }
            },
            groups: {
                title: 'Tổ chức Đoàn khám',
                subtitle: 'Vận hành: Tổ chức & Phân bổ nhân sự đi tuyến',
                addBtn: 'TẠO ĐOÀN MỚI',
                cancelBtn: 'HỦY BỎ',
                tabOpen: 'Đang thực hiện',
                tabFinished: 'Đã hoàn tất',
                btnFinish: 'HOÀN TẤT ĐOÀN',
                tabStaff: 'Đội ngũ tham gia',
                tabPositions: 'Cơ cấu Vị trí trực',
                btnAssign: '+ ĐIỀU ĐỘNG & GÁN VỊ TRÍ',
                btnAICopilot: 'AI COPILOT GỢI Ý',
                table: {
                    staff: 'NHÂN SỰ',
                    position: 'VỊ TRÍ LÀM VIỆC TẠI ĐOÀN',
                    shift: 'CA LÀM',
                    posName: 'Tên Vị trí',
                    posReq: 'Số lượng Cần',
                    posAssigned: 'Đã có mặt'
                },
                btnAddPosition: '+ THÊM VỊ TRÍ TRỰC'
            },
            contracts: {
                title: 'Hợp đồng Dịch vụ',
                subtitle: 'Kinh doanh: Khai báo Doanh thu & Hợp đồng',
                addBtn: 'THÊM HỢP ĐỒNG',
                cancelBtn: 'HỦY BỎ',
                formTitle: 'Khai báo Hợp đồng',
                formSubtitle: 'Thiết lập thông tin thương mại',
                table: {
                    client: 'ĐẠI DIỆN HỢP ĐỒNG',
                    value: 'TỔNG GIÁ TRỊ',
                    discount: 'CHIẾT KHẤU',
                    expected: 'QUY MÔ DỰ KIẾN',
                    actual: 'QUY MÔ THỰC TẾ',
                    date: 'NGÀY KẾT THÚC'
                }
            },
            permissions: {
                title: 'Quản trị Phân quyền',
                subtitle: 'Cấu hình đặc quyền truy cập cho từng vai trò nhân sự',
                list: 'Danh sách đặc quyền theo nhóm chức năng',
                selectAll: 'Quyết định Tất cả',
                deselect: 'Hủy chọn Toàn bộ',
                restore: 'Khôi phục Mặc định',
                save: 'Lưu Cấu hình',
                saving: 'Đang ghi nhận...',
                restoring: 'Đang thiết lập lại...',
                all: 'Toàn quyền nhóm'
            },
    departments: {
                title: 'Quản lý Trạm Khám & Chuyên Khoa',
                subtitle: '{0} trạm khám / đơn vị chuyên môn đang hoạt động',
                addBtn: 'Thêm Trạm Khám mới',
                empty: 'Chưa có Trạm Khám nào được tạo',
                startLabel: 'Bắt đầu bằng cách tạo trạm khám đầu tiên',
                noDesc: 'Chưa có mô tả chức năng của trạm khám / chuyên khoa này...',
                staff: 'Nhân sự',
                users: 'Tài khoản',
                formTitleEdit: 'Cập nhật Trạm Khám',
                formTitleAdd: 'Tạo Trạm Khám mới',
                formSubtitle: 'Đơn vị chuyên môn trong hệ thống đoàn khám sức khoẻ'
            },
            users: {
                title: 'Quản trị Tài khoản Hệ thống',
                subtitle: '{0} tài khoản đang có quyền truy cập',
                addBtn: 'Cấp Tài Khoản Mới',
                filterAll: 'Tất cả ({0})',
                filterAdmin: 'Quản trị viên ({0})',
                filterDoctor: 'Chuyên gia Y tế ({0})',
                filterDirector: 'Ban Giám đốc ({0})',
                filterStaff: 'Nhân viên ({0})',
                profileTitle: 'Thông tin Tài khoản',
                profileSubtitle: 'Thông tin cá nhân & Nhật ký hoạt động',
                table: {
                    account: 'Tài khoản',
                    role: 'Vai trò'
                },
                unnamed: 'Chưa đặt tên',
                empty: 'Không có dữ liệu tài khoản',
                formTitleEdit: 'Hiệu chỉnh Tài khoản',
                formTitleAdd: 'Cấp Tài khoản Mới',
                formSubtitle: 'Phân quyền và bảo mật truy cập hệ thống'
            },
            supplies: {
                title: 'Kho Vật tư Y tế',
                subtitle: 'Quản lý Xuất/Nhập và Tồn kho',
                addBtn: 'NHẬP VẬT TƯ MỚI',
                cancelBtn: 'HỦY BỎ'
            },
            roles: {
                Admin: 'Quản trị hệ thống',
                PersonnelManager: 'Quản lý Nhân sự',
                ContractManager: 'Quản lý Hợp đồng',
                MedicalGroupManager: 'Quản lý đoàn khám',
                WarehouseManager: 'Quản lý Kho',
                PayrollManager: 'Quản lý tính lương',
                MedicalStaff: 'Nhân sự đi đoàn',
                Customer: 'Tài khoản Công ty'
            },
            schedule: {
                title: 'Lịch khám của tôi',
                subtitle: 'Tháng {0} / {1} • Theo dõi tiến độ công việc',
                summary: {
                    groups: 'Đoàn tham gia',
                    days: 'Công thực tế',
                    salary: 'Lương ước tính'
                },
                item: {
                    month: 'T{0}',
                    shift1: '1.0 Công',
                    shift05: '0.5 Công'
                },
                empty: 'Không có lịch khám phát sinh trong tháng {0}/{1}'
            }
        },
        en: {
            nav: {
                dashboard: 'Dashboard',
                companies: 'Companies',
                contracts: 'Contracts',
                groups: 'Medical Groups',
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
                searchPlaceholder: 'Smart Search (Ctrl+K)...',
                noData: 'No Data Available',
                stt: 'No.',
                actions: 'Actions'
            },
            companies: {
                title: 'Partner Companies',
                subtitle: 'Internal: Corporate Clients Management',
                addBtn: 'ADD COMPANY',
                cancelBtn: 'CANCEL',
                formTitle: 'Declare New Company',
                formSubtitle: 'Add new corporate partner to the system',
                inputs: {
                    name: 'Company Legal Name',
                    taxCode: 'Tax Code',
                    companyPhone: 'Company Phone',
                    representativePhone: 'Representative Phone',
                    address: 'Headquarters Address'
                },
                table: {
                    info: 'COMPANY INFO',
                    taxCode: 'TAX CODE',
                    contact: 'CONTACT INFO',
                    address: 'ADDRESS'
                }
            },
            staff: {
                title: 'Medical Staff',
                subtitle: 'HR: Doctors & Nurses Profile Management',
                addBtn: 'ADD STAFF',
                cancelBtn: 'CANCEL',
                formTitle: 'Declare Staff',
                formSubtitle: 'Create new personnel profile',
                inputs: {
                    name: 'Staff Full Name',
                    salary: 'Base Salary',
                    role: 'System Role'
                },
                table: {
                    info: 'STAFF INFO',
                    title: 'JOB TITLE / ROLE',
                    salary: 'BASE SALARY',
                    comp: 'EST. COMPENSATION'
                }
            },
            groups: {
                title: 'Medical Group Operations',
                subtitle: 'Ops: Organization & Personnel Allocation',
                addBtn: 'CREATE GROUP',
                cancelBtn: 'CANCEL',
                tabOpen: 'In Progress',
                tabFinished: 'Completed',
                btnFinish: 'COMPLETE GROUP',
                tabStaff: 'Participating Staff',
                tabPositions: 'Position Structure',
                btnAssign: '+ ALLOCATE & ASSIGN',
                btnAICopilot: 'AI SMART SUGGESTION',
                table: {
                    staff: 'STAFF MEMBERS',
                    position: 'ASSIGNED POSITION',
                    shift: 'WORK SHIFT',
                    posName: 'Position Name',
                    posReq: 'Required Count',
                    posAssigned: 'Present'
                },
                btnAddPosition: '+ ADD POSITION'
            },
            contracts: {
                title: 'Service Contracts',
                subtitle: 'Sales: Revenue & Contract Declaration',
                addBtn: 'ADD CONTRACT',
                cancelBtn: 'CANCEL',
                formTitle: 'Declare Contract',
                formSubtitle: 'Establish commercial information',
                table: {
                    client: 'CONTRACT CLIENT',
                    date: 'EXAM DATE (EST.)',
                    value: 'TOTAL VALUE',
                    discount: 'DISCOUNT'
                }
            },
            departments: {
                title: 'Clinic Departments & Specialties',
                subtitle: '{0} departments / medical units active',
                addBtn: 'Add New Department',
                empty: 'No Departments Created Yet',
                startLabel: 'Start by creating your first department',
                noDesc: 'No functional description available for this department...',
                staff: 'Staff',
                users: 'Accounts',
                formTitleEdit: 'Update Department',
                formTitleAdd: 'Create New Department',
                formSubtitle: 'Specialized medical unit in the health check group system'
            },
            users: {
                title: 'Quản lý Tài khoản Hệ thống',
                subtitle: '{0} tài khoản đang hoạt động',
                addBtn: 'Cấp tài khoản mới',
                filterAll: 'Tất cả ({0})',
                filterAdmin: 'Ban Quản trị ({0})',
                filterDoctor: 'Chuyên gia Y tế ({0})',
                filterDirector: 'Ban Giám đốc ({0})',
                filterStaff: 'Nhân viên vận hành ({0})',
                profileTitle: 'Hồ sơ tài khoản',
                profileSubtitle: 'Thông tin cá nhân & Nhật ký hoạt động',
                table: {
                    account: 'Tài khoản',
                    role: 'Vai trò'
                },
                unnamed: 'Chưa đặt tên',
                empty: 'Không tìm thấy dữ liệu tài khoản',
                formTitleEdit: 'Cập nhật tài khoản',
                formTitleAdd: 'Cấp tài khoản mới',
                formSubtitle: 'Quản lý phân quyền & truy cập hệ thống'
            },
            supplies: {
                title: 'Medical Supplies Inventory',
                subtitle: 'Manage Import/Export and Stock Levels',
                addBtn: 'IMPORT NEW SUPPLIES',
                cancelBtn: 'CANCEL'
            },
            roles: {
                Admin: 'Quản trị hệ thống',
                PersonnelManager: 'Quản lý Nhân sự',
                ContractManager: 'Quản lý Hợp đồng',
                MedicalGroupManager: 'Quản lý Đoàn khám',
                WarehouseManager: 'Quản lý Kho vật tư',
                PayrollManager: 'Quản lý Tiền lương',
                MedicalStaff: 'Nhân viên Y tế',
                Customer: 'Đại diện Công ty'
            },
            schedule: {
                title: 'My Schedule',
                subtitle: 'Month {0} / {1} • Track Work Progress',
                summary: {
                    groups: 'Joined Groups',
                    days: 'Actual Days',
                    salary: 'Est. Salary'
                },
                item: {
                    month: 'M{0}',
                    shift1: '1.0 Shift',
                    shift05: '0.5 Shift'
                },
                empty: 'No schedule found in month {0}/{1}'
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
            if (result && result[key]) result = result[key]
            else return path
        }
        return result
    }

    return { locale, setLocale, t }
})
