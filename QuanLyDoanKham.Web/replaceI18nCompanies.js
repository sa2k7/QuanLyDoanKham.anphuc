const fs = require('fs')

const file = 'src/views/Companies.vue'
let content = fs.readFileSync(file, 'utf8')

// Inject imports
if (!content.includes(`import { useI18nStore } from '../stores/i18n'`)) {
    content = content.replace(
        `import { useToast } from '../composables/useToast'`,
        `import { useToast } from '../composables/useToast'\nimport { useI18nStore } from '../stores/i18n'`
    )
    content = content.replace(
        `const toast = useToast()`,
        `const toast = useToast()\nconst i18n = useI18nStore()`
    )
}

// Header
content = content.replace('Công ty Đối tác', `{{ i18n.t('companies.title') }}`)
content = content.replace('Nội bộ: Quản lý thông tin pháp nhân khách hàng', `{{ i18n.t('companies.subtitle') }}`)
content = content.replace(`'HỦY BỎ' : 'THÊM CÔNG TY'`, `i18n.t('companies.cancelBtn') : i18n.t('companies.addBtn')`)

// Inline Form
content = content.replace('>Khai báo Công ty mới<', `>{{ i18n.t('companies.formTitle') }}<`)
content = content.replace('>Thêm mới đối tác pháp nhân vào hệ thống<', `>{{ i18n.t('companies.formSubtitle') }}<`)
// Table
content = content.replace('>Tên công ty<', `>{{ i18n.t('companies.table.info') }}<`)
content = content.replace('>Người đại diện<', `>Người đại diện<`) // Skip
content = content.replace('>Mã số thuế<', `>{{ i18n.t('companies.table.taxCode') }}<`)
content = content.replace('>SĐT Công ty<', `>{{ i18n.t('companies.table.contact') }}<`)
content = content.replace('>SĐT Đại diện<', `>SĐT Đại diện<`) // Skip
content = content.replace('>Địa chỉ<', `>{{ i18n.t('companies.table.address') }}<`)
content = content.replace('>Tác vụ<', `>{{ i18n.t('common.actions') }}<`)

// Modal
content = content.replace('>Cập nhật công ty<', `>{{ i18n.t('common.edit') }}<`)
content = content.replace('>Chỉnh sửa thông tin hồ sơ đối tác<', `>Chỉnh sửa thông tin hồ sơ đối tác<`) // Skip
content = content.replace('>HỦY<', `>{{ i18n.t('common.cancel') }}<`)
content = content.replace('>LƯU THÔNG TIN<', `>{{ i18n.t('common.save') }}<`)

fs.writeFileSync(file, content, 'utf8')
console.log('Applied i18n to Companies.vue')
