<template>
  <div class="permission-matrix">
    <!-- Loading Overlay -->
    <div v-if="isLoading" class="permission-matrix-loading">
      <Loader2 class="permission-matrix-spinner" />
    </div>

    <!-- Empty State -->
    <div v-if="!selectedRole && !isLoading" class="permission-matrix-empty">
      <ShieldAlert class="permission-matrix-empty-icon" />
      <p class="permission-matrix-empty-title">Chọn một vai trò bên trái</p>
      <p class="permission-matrix-empty-subtitle">để xem và chỉnh sửa phân quyền.</p>
    </div>

    <!-- Matrix Content -->
    <template v-if="selectedRole && !isLoading">
      <!-- Header Bar -->
      <div class="permission-matrix-header">
        <div>
          <h3 class="permission-matrix-role-name">
            Phân quyền cho:
            <span class="permission-matrix-role-highlight">
              {{ selectedRole.description || selectedRole.roleName }}
            </span>
          </h3>
          <p class="permission-matrix-counter">
            {{ activePermissionIds.length }} / {{ totalPermissionCount }} quyền đang bật
          </p>
        </div>
        <div class="permission-matrix-actions">
          <button class="permission-matrix-btn is-outline" @click="$emit('toggle-all', true)">
            <CheckCheck class="permission-matrix-btn-icon" /> Chọn hết
          </button>
          <button class="permission-matrix-btn is-outline is-danger" @click="$emit('toggle-all', false)">
            <X class="permission-matrix-btn-icon" /> Bỏ chọn
          </button>
          <button
            class="permission-matrix-btn is-primary"
            :disabled="isSaving"
            @click="$emit('save-permissions')"
          >
            <Loader2 v-if="isSaving" class="permission-matrix-btn-icon is-spinning" />
            <Save v-else class="permission-matrix-btn-icon" />
            {{ isSaving ? 'Đang lưu...' : 'Lưu Phân Quyền' }}
          </button>
        </div>
      </div>

      <!-- Module Groups Grid -->
      <div class="permission-matrix-grid">
        <ModulePermissionGroup
          v-for="group in permissionGroups"
          :key="group.module"
          :module-key="group.module"
          :module-label="getModuleLabel(group.module)"
          :module-icon="getModuleIcon(group.module)"
          :permissions="group.permissions"
          :active-permission-ids="activePermissionIds"
          @toggle-permission="(id) => $emit('toggle-permission', id)"
          @toggle-module="(key, val) => $emit('toggle-module', key, val)"
        />
      </div>
    </template>
  </div>
</template>

<script setup>
import { ShieldAlert, Loader2, CheckCheck, X, Save } from 'lucide-vue-next'
import ModulePermissionGroup from './ModulePermissionGroup.vue'
import { MODULE_LABELS, MODULE_ICONS } from '@/data/permission-mock-data'

const props = defineProps({
  selectedRole:        { type: Object,  default: null },
  permissionGroups:    { type: Array,   required: true },
  activePermissionIds: { type: Array,   required: true },
  isSaving:            { type: Boolean, default: false },
  isLoading:           { type: Boolean, default: false },
  totalPermissionCount: { type: Number, default: 0 },
})

defineEmits([
  'toggle-permission',
  'toggle-module',
  'toggle-all',
  'save-permissions',
])

const getModuleLabel = (moduleKey) => MODULE_LABELS[moduleKey] || moduleKey
const getModuleIcon = (moduleKey) => MODULE_ICONS[moduleKey] || '🔒'
</script>

<style scoped>
.permission-matrix {
  background: #fff;
  border-radius: 24px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.04);
  overflow: hidden;
  position: relative;
  min-height: 520px;
}

/* Loading */
.permission-matrix-loading {
  position: absolute;
  inset: 0;
  background: rgba(255,255,255,0.8);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
  border-radius: 24px;
}
.permission-matrix-spinner {
  width: 40px; height: 40px;
  color: #6366f1;
  animation: spin 1s linear infinite;
}

/* Empty State */
.permission-matrix-empty {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #cbd5e1;
}
.permission-matrix-empty-icon {
  width: 72px; height: 72px;
  margin-bottom: 16px;
}
.permission-matrix-empty-title {
  font-weight: 700;
  font-size: 1.1rem;
  color: #94a3b8;
}
.permission-matrix-empty-subtitle {
  font-size: 0.875rem;
  color: #cbd5e1;
}

/* Header */
.permission-matrix-header {
  padding: 20px 24px;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
  background: rgba(255,255,255,0.8);
  backdrop-filter: blur(8px);
  position: sticky;
  top: 0;
  z-index: 5;
}
.permission-matrix-role-name {
  font-size: 1.05rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}
.permission-matrix-role-highlight {
  color: #6366f1;
}
.permission-matrix-counter {
  font-size: 10px;
  font-weight: 700;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  margin-top: 4px;
}

/* Actions */
.permission-matrix-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}
.permission-matrix-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  cursor: pointer;
  transition: all 0.2s ease;
  border: 2px solid #e2e8f0;
  background: transparent;
  color: #64748b;
}
.permission-matrix-btn:hover {
  border-color: #6366f1;
  color: #6366f1;
}
.permission-matrix-btn.is-danger:hover {
  border-color: #f43f5e;
  color: #f43f5e;
}
.permission-matrix-btn.is-primary {
  background: linear-gradient(135deg, #6366f1, #4f46e5);
  color: #fff;
  border-color: transparent;
  padding: 8px 24px;
}
.permission-matrix-btn.is-primary:hover {
  box-shadow: 0 8px 24px rgba(99,102,241,0.3);
  transform: translateY(-1px);
}
.permission-matrix-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}
.permission-matrix-btn-icon {
  width: 16px; height: 16px;
}
.permission-matrix-btn-icon.is-spinning {
  animation: spin 1s linear infinite;
}

/* Grid */
.permission-matrix-grid {
  padding: 20px;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
  gap: 16px;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Responsive */
@media (max-width: 640px) {
  .permission-matrix-header {
    flex-direction: column;
    align-items: flex-start;
  }
  .permission-matrix-grid {
    grid-template-columns: 1fr;
  }
}
</style>
