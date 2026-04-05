<template>
  <div class="departments-page animate-fade-in">
    <div class="page-header flex justify-between items-center mb-10">
      <div>
        <h1 class="page-title text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-primary text-white rounded-2xl flex items-center justify-center shadow-lg">
            <Building2 class="w-6 h-6" />
          </div>
          {{ i18n.t('departments.title') }}
        </h1>
        <p class="page-subtitle text-slate-400 font-black uppercase tracking-widest text-[10px] mt-2">{{ i18n.t('departments.subtitle').replace('{0}', departments.length) }}</p>
      </div>
      <button v-if="isAdmin" class="btn-premium bg-primary text-white px-8 py-3 shadow-lg" @click="openCreateModal">
        <Plus class="w-5 h-5" /> {{ i18n.t('departments.addBtn') }}
      </button>
    </div>

    <!-- Department Cards -->
    <div class="dept-grid grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
      <div v-for="dept in departments" :key="dept.departmentId" class="dept-card premium-card bg-white p-8 rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] hover:-translate-y-1 transition-all group">
        <div class="dept-header flex items-center gap-4 mb-6">
          <div class="dept-icon text-4xl group-hover:scale-110 transition-transform">{{ getDeptIcon(dept.departmentCode) }}</div>
          <div class="dept-info flex-1">
            <h3 class="dept-name font-black text-slate-800 uppercase tracking-widest">{{ dept.departmentName }}</h3>
            <span class="dept-code inline-block bg-indigo-50 text-indigo-600 px-3 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest mt-1 border border-indigo-100">{{ dept.departmentCode }}</span>
          </div>
          <div v-if="isAdmin" class="dept-actions flex gap-2">
            <button class="w-10 h-10 rounded-xl bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-400 hover:bg-indigo-50 hover:text-indigo-600 hover:border-indigo-100 transition-all" @click="openEditModal(dept)" title="Sửa">
              <Edit class="w-4 h-4" />
            </button>
            <button class="w-10 h-10 rounded-xl bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-400 hover:bg-rose-50 hover:text-rose-600 hover:border-rose-100 transition-all" @click="confirmDelete(dept)" title="Xóa">
              <Trash2 class="w-4 h-4" />
            </button>
          </div>
        </div>
        <p class="dept-desc text-slate-400 text-xs font-medium leading-relaxed mb-6 h-12 overflow-hidden line-clamp-2 italic">{{ dept.description || i18n.t('departments.noDesc') }}</p>
        <div class="dept-stats grid grid-cols-2 gap-4 border-t border-slate-50 pt-6">
          <div class="stat flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-indigo-50 text-indigo-600 flex items-center justify-center">
                <Users class="w-4 h-4" />
            </div>
            <div>
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">{{ i18n.t('departments.staff') }}</p>
                <p class="text-sm font-black text-slate-700">{{ dept.totalStaff }} TV</p>
            </div>
          </div>
          <div class="stat flex items-center gap-3">
             <div class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-600 flex items-center justify-center">
                <ShieldCheck class="w-4 h-4" />
            </div>
            <div>
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">{{ i18n.t('departments.users') }}</p>
                <p class="text-sm font-black text-slate-700">{{ dept.totalUsers }} TK</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="!loading && departments.length === 0" class="flex flex-col items-center justify-center py-32 text-center">
      <Building2 class="w-16 h-16 text-slate-200 mb-6" />
      <p class="text-slate-400 font-black uppercase tracking-widest text-sm">{{ i18n.t('departments.empty') }}</p>
      <button v-if="isAdmin" @click="openCreateModal" class="mt-6 text-orange-500 font-black uppercase tracking-widest text-xs hover:underline decoration-2">{{ i18n.t('departments.startLabel') }}</button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-20">
      <Loader2 class="w-10 h-10 animate-spin text-slate-200" />
    </div>

    <!-- Modal Thêm/Sửa -->
    <Teleport to="body">
        <div v-if="showModal" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-md p-4 overflow-y-auto" @click.self="closeModal">
          <div class="bg-white w-full max-w-xl rounded-[3rem] border-2 border-slate-900 shadow-2xl animate-fade-in-up relative overflow-hidden mt-auto mb-auto">
             <!-- Border Overlay -->
             <div class="absolute inset-0 rounded-[inherit] border-2 border-slate-900 pointer-events-none z-50"></div>
              
             <!-- Header Accent Line -->
             <div class="absolute top-0 left-0 right-0 h-4 bg-gradient-to-r from-indigo-400 to-indigo-600 z-0"></div>

              <div class="p-10 pb-6 relative z-10 pt-12 flex justify-between items-center">
                  <div class="flex items-center gap-4">
                      <div class="w-14 h-14 bg-indigo-50 text-indigo-600 rounded-3xl flex items-center justify-center shadow-inner border border-indigo-100">
                          <Plus v-if="!editMode" class="w-7 h-7" />
                          <Edit v-else class="w-7 h-7" />
                      </div>
                      <div>
                          <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest">{{ editMode ? i18n.t('departments.formTitleEdit') : i18n.t('departments.formTitleAdd') }}</h3>
                          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">{{ i18n.t('departments.formSubtitle') }}</p>
                      </div>
                  </div>
                  <button @click="closeModal" class="bg-slate-100 p-2 rounded-full hover:bg-slate-200 transition-all text-slate-500">
                      <X class="w-5 h-5" />
                  </button>
              </div>

            <form @submit.prevent="saveDepart" class="p-10 space-y-6">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="flex flex-col gap-2">
                  <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Tên Trạm Khám / Chuyên Khoa <span class="text-rose-500">*</span></label>
                  <input v-model="form.departmentName" type="text"
                    placeholder="VD: Trạm Siêu Âm, Khoa Mắt..." required class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" />
                </div>
                <div class="flex flex-col gap-2">
                  <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Mã Trạm <span class="text-rose-500">*</span></label>
                  <input v-model="form.departmentCode" type="text"
                    placeholder="VD: SIEU_AM, KHOA_MAT" maxlength="20" required class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" />
                </div>
              </div>
              <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Mô tả chức năng y tế</label>
                <textarea v-model="form.description" rows="3"
                  placeholder="VD: Thực hiện siêu âm ổ bụng, tim mạch và các cơ quan nội tạng..." class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full py-4 h-32"></textarea>
              </div>
              
              <div class="pt-6">
                <button type="submit" class="w-full btn-premium bg-orange-500 text-white shadow-orange-500/20 py-6" :disabled="saving">
                  <span v-if="saving" class="flex items-center gap-2"><Loader2 class="w-5 h-5 animate-spin" /> ĐANG LƯU...</span>
                  <span v-else>{{ editMode ? 'LƯU CẬP NHẬT TRẠM KHÁM' : 'TẠO TRẠM KHÁM MỚI' }}</span>
                </button>
              </div>
            </form>
          </div>
        </div>
    </Teleport>

    <!-- Confirm Delete -->
    <Teleport to="body">
        <div v-if="deleteTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-900/80 backdrop-blur-md" @click.self="deleteTarget = null">
          <div class="bg-white w-full max-w-sm rounded-[3rem] border-2 border-slate-900 shadow-2xl p-10 text-center animate-fade-in-up relative overflow-hidden">
             <!-- Border Overlay -->
             <div class="absolute inset-0 rounded-[inherit] border-2 border-slate-900 pointer-events-none z-50"></div>
             
             <div class="w-20 h-20 bg-rose-50 text-rose-600 rounded-[2rem] flex items-center justify-center mx-auto mb-6 border-2 border-rose-100">
                <AlertTriangle class="w-10 h-10" />
             </div>
              <h2 class="text-xl font-black text-slate-800 uppercase tracking-widest mb-4">Xác nhận xóa Trạm?</h2>
              <p class="text-sm text-slate-500 mb-8 font-medium">Bạn có chắc muốn xóa trạm khám <strong class="text-slate-800">{{ deleteTarget.departmentName }}</strong>? Hành động này không thể hoàn tác.</p>
              
              <div class="bg-amber-50 rounded-2xl p-4 flex gap-4 text-left border border-amber-100 mb-8">
                  <Info class="w-5 h-5 text-amber-500 shrink-0 mt-0.5" />
                  <p class="text-[10px] font-black text-amber-600 uppercase tracking-widest leading-relaxed">Lưu ý: Không thể xóa nếu trạm đang có nhân sự đang phân công.</p>
              </div>

              <div class="flex gap-4">
                <button class="flex-1 px-6 py-4 rounded-2xl font-black text-xs uppercase tracking-widest text-slate-400 hover:bg-slate-50 transition-all" @click="deleteTarget = null">Hủy bỏ</button>
                <button class="flex-1 px-6 py-4 rounded-2xl font-black text-xs uppercase tracking-widest bg-rose-600 text-white shadow-lg shadow-rose-600/20" @click="doDelete" :disabled="saving">
                  <Loader2 v-if="saving" class="w-4 h-4 animate-spin mx-auto" />
                  <span v-else>ĐỒNG Ý XÓA</span>
                </button>
              </div>
          </div>
        </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '../stores/auth'
import apiClient from '../services/apiClient'
import { parseApiError } from '../services/errorHelper'
import { useToast } from '../composables/useToast'
import { usePermission } from '../composables/usePermission'
import { useI18nStore } from '../stores/i18n'
import { 
    Building2, Plus, Edit, Trash2, Users, ShieldCheck, 
    AlertTriangle, Info, Loader2, X 
} from 'lucide-vue-next'

const auth = useAuthStore()
const { can } = usePermission()
const toast = useToast()
const i18n = useI18nStore()
const isAdmin = computed(() => can('HeThong.UserManage'))

const departments = ref([])
const loading = ref(false)
const saving = ref(false)
const showModal = ref(false)
const editMode = ref(false)
const deleteTarget = ref(null)

const defaultForm = { departmentId: null, departmentName: '', departmentCode: '', description: '' }
const form = ref({ ...defaultForm })

const deptIconMap = {
  // Trạm khám chuyên khoa
  'SIEU_AM': '🔊',     // Siêu âm
  'XET_NGHIEM': '🧪',  // Xét nghiệm
  'KHAM_NOI': '🩺',    // Khám nội tổng quát
  'MAT': '👁️',         // Khoa Mắt
  'TAI_MUI_HONG': '👂', // Tai Mũi Họng
  'RANG_HAM_MAT': '🦷', // Răng Hàm Mặt
  'X_QUANG': '🩻',     // X-Quang
  'TIM_MACH': '❤️',    // Tim mạch
  'DA_LIEU': '🧴',     // Da liễu
  'LAY_MAU': '💉',     // Lấy máu / Xét nghiệm máu
  'DIEN_TIM': '📈',    // Điện tim ECG
  'PHUC_HOI': '🏃',    // Phục hồi chức năng
  // Hành chính
  'HCNS': '🏢', 'DHDK': '📋', 'KVT': '📦',
  'KT': '💰', 'TKBC': '📊', 'NVDK': '🏥'
}
const getDeptIcon = (code) => deptIconMap[code?.toUpperCase()] || '🏥'

const fetchDepartments = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/api/Departments')
    departments.value = res.data
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  editMode.value = false
  form.value = { ...defaultForm }
  showModal.value = true
}

const openEditModal = (dept) => {
  editMode.value = true
  form.value = { ...dept }
  showModal.value = true
}

const closeModal = () => { showModal.value = false }

const saveDepart = async () => {
  saving.value = true
  try {
    if (editMode.value) {
      await apiClient.put(`/api/Departments/${form.value.departmentId}`, form.value)
    } else {
      await apiClient.post('/api/Departments', form.value)
    }
    await fetchDepartments()
    closeModal()
    toast.success(editMode.value ? 'Đã cập nhật trạm khám' : 'Đã tạo trạm khám mới')
  } catch (e) {
    toast.error(parseApiError(e))
  } finally {
    saving.value = false
  }
}

const confirmDelete = (dept) => { deleteTarget.value = dept }

const doDelete = async () => {
  saving.value = true
  try {
    await apiClient.delete(`/api/Departments/${deleteTarget.value.departmentId}`)
    await fetchDepartments()
    deleteTarget.value = null
    toast.success('Đã xóa trạm khám')
  } catch (e) {
    toast.error(parseApiError(e))
  } finally {
    saving.value = false
  }
}

onMounted(fetchDepartments)
</script>

<style scoped>
.departments-page { padding: 24px; max-width: 1200px; margin: 0 auto; }

.page-header {
  display: flex; justify-content: space-between; align-items: center;
  margin-bottom: 28px;
}
.page-title { font-size: 1.6rem; font-weight: 700; color: var(--text-primary, #1e293b); margin: 0; }
.page-title i { margin-right: 10px; color: var(--primary, #6366f1); }
.page-subtitle { color: var(--text-muted, #64748b); margin: 4px 0 0; font-size: 0.9rem; }

.dept-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}
.dept-card {
  background: var(--card-bg, #fff);
  border-radius: 14px;
  padding: 20px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.08);
  border: 1px solid var(--border, #e2e8f0);
  transition: transform .2s, box-shadow .2s;
}
.dept-card:hover { transform: translateY(-3px); box-shadow: 0 4px 16px rgba(0,0,0,0.12); }

.dept-header { display: flex; align-items: center; gap: 12px; margin-bottom: 12px; }
.dept-icon { font-size: 2rem; }
.dept-info { flex: 1; }
.dept-name { font-size: 1rem; font-weight: 600; margin: 0; }
.dept-code {
  display: inline-block; background: var(--primary-light, #eef2ff);
  color: var(--primary, #6366f1); padding: 2px 8px; border-radius: 6px;
  font-size: 0.75rem; font-weight: 600; margin-top: 4px;
}
.dept-actions { display: flex; gap: 6px; }
.btn-icon {
  width: 32px; height: 32px; border: none; border-radius: 8px;
  background: var(--hover, #f1f5f9); cursor: pointer; display: flex;
  align-items: center; justify-content: center; color: var(--text-muted, #64748b);
  transition: all .15s;
}
.btn-icon:hover { background: var(--primary, #6366f1); color: #fff; }
.btn-icon.btn-danger:hover { background: #ef4444; color: #fff; }

.dept-desc { color: var(--text-muted, #64748b); font-size: 0.88rem; margin: 0 0 14px; }
.dept-stats { display: flex; gap: 16px; }
.stat { display: flex; align-items: center; gap: 6px; font-size: 0.85rem; color: var(--text-secondary, #475569); }
.stat i { color: var(--primary, #6366f1); }

/* Modal */
.modal-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,0.5);
  display: flex; align-items: center; justify-content: center; z-index: 1000;
}
.modal-box {
  background: var(--card-bg, #fff); border-radius: 16px; width: 90%; max-width: 520px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.25); overflow: hidden;
}
.modal-sm { max-width: 400px; }
.modal-header {
  padding: 20px 24px; background: var(--primary, #6366f1); color: #fff;
  display: flex; justify-content: space-between; align-items: center;
}
.modal-header.danger { background: #ef4444; }
.modal-header h2 { margin: 0; font-size: 1.1rem; }
.btn-close { background: none; border: none; color: #fff; font-size: 1.5rem; cursor: pointer; }
.modal-form { padding: 24px; }
.modal-body { padding: 20px 24px; }
.modal-footer { padding: 16px 24px; display: flex; justify-content: flex-end; gap: 10px; border-top: 1px solid var(--border, #e2e8f0); }

.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.form-group { margin-bottom: 16px; }
.form-group label { display: block; font-size: 0.85rem; font-weight: 500; margin-bottom: 6px; color: var(--text-secondary, #475569); }
.required { color: #ef4444; }
.form-control {
  width: 100%; padding: 10px 14px; border: 1px solid var(--border, #e2e8f0);
  border-radius: 10px; font-size: 0.9rem; background: var(--input-bg, #f8fafc);
  color: var(--text-primary, #1e293b); transition: border-color .2s; box-sizing: border-box;
}
.form-control:focus { outline: none; border-color: var(--primary, #6366f1); }

.btn { padding: 10px 20px; border: none; border-radius: 10px; font-weight: 600; cursor: pointer; font-size: 0.9rem; transition: all .15s; }
.btn-primary { background: var(--primary, #6366f1); color: #fff; }
.btn-primary:hover:not(:disabled) { background: #4f46e5; }
.btn-secondary { background: var(--hover, #f1f5f9); color: var(--text-secondary, #475569); }
.btn-danger { background: #ef4444; color: #fff; }
.btn:disabled { opacity: 0.6; cursor: not-allowed; }

.text-warning { color: #f59e0b; font-size: 0.85rem; margin-top: 8px; }
.empty-state { text-align: center; padding: 60px; color: var(--text-muted, #64748b); }
.empty-state i { font-size: 3rem; margin-bottom: 16px; opacity: 0.4; }
.loading-overlay { display: flex; justify-content: center; padding: 60px; }
</style>
