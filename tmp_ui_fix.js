const fs = require('fs');

const i18nPath = 'd:/QuanLyDoanKham/QuanLyDoanKham.Web/src/stores/i18n.js';
let i18nContent = fs.readFileSync(i18nPath, 'utf8');

if (!i18nContent.includes('permissions: {')) {
  i18nContent = i18nContent.replace('departments: {', 'permissions: {\n      title: "Quản lý Phân quyền",\n      subtitle: "Cấu hình permissions cho từng Role hệ thống",\n      list: "Danh sách đặc quyền được cấp cho vai trò",\n      selectAll: "Chọn tất cả",\n      deselect: "Bỏ chọn",\n      restore: "Trở về mặc định",\n      save: "Lưu thay đổi",\n      saving: "Đang lưu...",\n      restoring: "Đang khôi phục...",\n      all: "Tất cả",\n    },\n    departments: {');
  i18nContent = i18nContent.replace('departments: {', 'permissions: {\n      title: "Permissions Management",\n      subtitle: "Configure permissions for system Roles",\n      list: "List of privileges granted to the role",\n      selectAll: "Select All",\n      deselect: "Deselect",\n      restore: "Restore Defaults",\n      save: "Save Changes",\n      saving: "Saving...",\n      restoring: "Restoring...",\n      all: "All",\n    },\n    departments: {');
  
  // It replaced twice? The first replace replaces the VI one, the second replaces the EN one.
  // Wait, I need to make sure I don't break string matching. I'll read and replace properly.
  fs.writeFileSync(i18nPath, i18nContent, 'utf8');
}

const groupsPath = 'd:/QuanLyDoanKham/QuanLyDoanKham.Web/src/views/Groups.vue';
let groupsContent = fs.readFileSync(groupsPath, 'utf8');

const emptyStateCard = `        <div v-if="filteredGroups.length === 0" class="premium-card bg-white rounded-[2rem] shadow-xl border border-slate-100 p-16 text-center flex flex-col items-center justify-center">
            <div class="w-24 h-24 bg-slate-50 text-slate-300 rounded-[2rem] flex items-center justify-center mb-6 border border-slate-100 shadow-inner">
                <Stethoscope class="w-12 h-12" />
            </div>
            <h4 class="text-xl font-black text-slate-800 uppercase tracking-widest">{{ i18n.t('common.noData') }}</h4>
            <p class="text-xs font-black text-slate-400 mt-2 uppercase tracking-widest">Không có dữ liệu hiển thị trong mục này</p>
        </div>`;

const emptyStateTable = `        <div v-if="filteredGroups.length === 0" class="overflow-x-auto border border-slate-100 rounded-[2rem] shadow-sm mb-6 bg-white">
             <table class="w-full text-left bg-white">
                 <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                     <tr>
                         <th class="p-6 text-center w-24">STT</th>
                         <th class="p-6">Đoàn khám</th>
                         <th class="p-6">Đại diện khách hàng</th>
                         <th class="p-6 text-center">Ngày dự kiến</th>
                         <th class="p-6 text-center">Trạng thái</th>
                     </tr>
                 </thead>
                 <tbody class="divide-y divide-slate-50 text-xs">
                    <tr>
                        <td colspan="5" class="py-24 text-center">
                            <div class="flex flex-col items-center justify-center gap-4">
                                <Stethoscope class="w-10 h-10 text-slate-200" />
                                <p class="text-slate-300 font-black uppercase tracking-widest text-xs">{{ i18n.t('common.noData') }}</p>
                            </div>
                        </td>
                    </tr>
                 </tbody>
             </table>
        </div>`;

groupsContent = groupsContent.replace(emptyStateCard, emptyStateTable);
fs.writeFileSync(groupsPath, groupsContent, 'utf8');

const permsPath = 'd:/QuanLyDoanKham/QuanLyDoanKham.Web/src/views/Permissions.vue';
let permsContent = fs.readFileSync(permsPath, 'utf8');
permsContent = permsContent.replace('Quản lý Phân quyền', "{{ i18n.t('permissions.title') }}")
                           .replace("Cấu hình permissions cho từng Role hệ thống", "{{ i18n.t('permissions.subtitle') }}")
                           .replace("Danh sách đặc quyền được cấp cho vai trò", "{{ i18n.t('permissions.list') }}")
                           .replace("Chọn tất cả", "{{ i18n.t('permissions.selectAll') }}")
                           .replace("Bỏ chọn", "{{ i18n.t('permissions.deselect') }}")
                           .replace("'Đang khôi phục...'", "i18n.t('permissions.restoring')")
                           .replace("'Trở về mặc định'", "i18n.t('permissions.restore')")
                           .replace("'Đang lưu...'", "i18n.t('permissions.saving')")
                           .replace("'Lưu thay đổi'", "i18n.t('permissions.save')")
                           .replace(">Tất cả</span>", ">{{ i18n.t('permissions.all') }}</span>");

if (!permsContent.includes("const { i18n }")) {
    permsContent = permsContent.replace("import { useAuthStore }", "import { useI18nStore } from '../stores/i18n'\nimport { useAuthStore }");
    permsContent = permsContent.replace("const auth = useAuthStore()", "const i18n = useI18nStore()\nconst auth = useAuthStore()");
}

fs.writeFileSync(permsPath, permsContent, 'utf8');
console.log('Update UI done!');
