<template>
  <div class="departments-page">
    <div class="page-header">
      <div class="header-info">
        <h1 class="page-title">
          <i class="fas fa-building"></i> Quản lý Phòng ban
        </h1>
        <p class="page-subtitle">{{ departments.length }} phòng ban đang hoạt động</p>
      </div>
      <button v-if="isAdmin" class="btn btn-primary" @click="openCreateModal">
        <i class="fas fa-plus"></i> Thêm phòng ban
      </button>
    </div>

    <!-- Department Cards -->
    <div class="dept-grid">
      <div v-for="dept in departments" :key="dept.departmentId" class="dept-card">
        <div class="dept-header">
          <div class="dept-icon">{{ getDeptIcon(dept.departmentCode) }}</div>
          <div class="dept-info">
            <h3 class="dept-name">{{ dept.departmentName }}</h3>
            <span class="dept-code">{{ dept.departmentCode }}</span>
          </div>
          <div v-if="isAdmin" class="dept-actions">
            <button class="btn-icon" @click="openEditModal(dept)" title="Sửa">
              <i class="fas fa-edit"></i>
            </button>
            <button class="btn-icon btn-danger" @click="confirmDelete(dept)" title="Xóa">
              <i class="fas fa-trash"></i>
            </button>
          </div>
        </div>
        <p class="dept-desc">{{ dept.description || 'Chưa có mô tả' }}</p>
        <div class="dept-stats">
          <div class="stat">
            <i class="fas fa-user-injured"></i>
            <span>{{ dept.totalStaff }} nhân viên</span>
          </div>
          <div class="stat">
            <i class="fas fa-user-shield"></i>
            <span>{{ dept.totalUsers }} tài khoản</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="!loading && departments.length === 0" class="empty-state">
      <i class="fas fa-building"></i>
      <p>Chưa có phòng ban nào. Bắt đầu bằng cách thêm phòng ban đầu tiên.</p>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading-overlay">
      <div class="spinner"></div>
    </div>

    <!-- Modal Thêm/Sửa -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-box">
        <div class="modal-header">
          <h2>{{ editMode ? 'Cập nhật Phòng ban' : 'Thêm Phòng ban mới' }}</h2>
          <button class="btn-close" @click="closeModal">×</button>
        </div>
        <form @submit.prevent="saveDepart" class="modal-form">
          <div class="form-row">
            <div class="form-group">
              <label>Tên phòng ban <span class="required">*</span></label>
              <input v-model="form.departmentName" type="text"
                placeholder="VD: Hành chính - Nhân sự" required class="form-control" />
            </div>
            <div class="form-group">
              <label>Mã phòng ban <span class="required">*</span></label>
              <input v-model="form.departmentCode" type="text"
                placeholder="VD: HCNS" maxlength="20" required class="form-control" />
            </div>
          </div>
          <div class="form-group">
            <label>Mô tả</label>
            <textarea v-model="form.description" rows="3"
              placeholder="Mô tả chức năng phòng ban..." class="form-control"></textarea>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeModal">Hủy</button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              <span v-if="saving"><i class="fas fa-spinner fa-spin"></i> Đang lưu...</span>
              <span v-else>{{ editMode ? 'Cập nhật' : 'Thêm mới' }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Confirm Delete -->
    <div v-if="deleteTarget" class="modal-overlay" @click.self="deleteTarget = null">
      <div class="modal-box modal-sm">
        <div class="modal-header danger">
          <h2><i class="fas fa-exclamation-triangle"></i> Xác nhận xóa</h2>
        </div>
        <div class="modal-body">
          <p>Bạn có chắc muốn xóa phòng ban <strong>{{ deleteTarget.departmentName }}</strong>?</p>
          <p class="text-warning"><i class="fas fa-info-circle"></i>
            Không thể xóa nếu còn nhân viên thuộc phòng ban này.
          </p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" @click="deleteTarget = null">Hủy</button>
          <button class="btn btn-danger" @click="doDelete" :disabled="saving">
            <span v-if="saving"><i class="fas fa-spinner fa-spin"></i></span>
            <span v-else>Xóa</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/apiClient'

const auth = useAuthStore()
const isAdmin = auth.isAdmin

const departments = ref([])
const loading = ref(false)
const saving = ref(false)
const showModal = ref(false)
const editMode = ref(false)
const deleteTarget = ref(null)

const defaultForm = { departmentId: null, departmentName: '', departmentCode: '', description: '' }
const form = ref({ ...defaultForm })

const deptIconMap = {
  'HCNS': '🏢', 'DHDK': '📋', 'KVT': '📦',
  'KT': '💰', 'TKBC': '📊', 'NVDK': '🩺'
}
const getDeptIcon = (code) => deptIconMap[code] || '🏛️'

const fetchDepartments = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/Departments')
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
      await apiClient.put(`/Departments/${form.value.departmentId}`, form.value)
    } else {
      await apiClient.post('/Departments', form.value)
    }
    await fetchDepartments()
    closeModal()
  } catch (e) {
    alert(e.response?.data?.message || 'Lỗi khi lưu phòng ban.')
  } finally {
    saving.value = false
  }
}

const confirmDelete = (dept) => { deleteTarget.value = dept }

const doDelete = async () => {
  saving.value = true
  try {
    await apiClient.delete(`/Departments/${deleteTarget.value.departmentId}`)
    await fetchDepartments()
    deleteTarget.value = null
  } catch (e) {
    alert(e.response?.data?.message || 'Không thể xóa phòng ban.')
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
