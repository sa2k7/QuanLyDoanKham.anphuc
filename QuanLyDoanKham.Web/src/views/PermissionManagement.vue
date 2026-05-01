<template>
  <div class="permission-management-page">
    <div class="permission-management-container">
      <!-- Page Header -->
      <div class="permission-management-page-header">
        <div>
          <h2 class="permission-management-page-title">
            <div class="permission-management-title-icon">
              <ShieldCheck class="w-6 h-6" />
            </div>
            Phân Quyền Hệ Thống
          </h2>
          <p class="permission-management-page-subtitle">
            Quản trị vai trò &amp; Quyền hạn truy cập (RBAC Matrix)
          </p>
        </div>
      </div>

      <!-- Two-Column Layout -->
      <div class="permission-management-layout">
        <!-- Left: Role Sidebar -->
        <div class="permission-management-sidebar">
          <RoleSidebar
            :role-list="roleList"
            :active-role-id="activeRoleId"
            @role-selected="handleRoleSelected"
          />
        </div>

        <!-- Right: Permission Matrix -->
        <div class="permission-management-main">
          <PermissionMatrix
            :selected-role="selectedRole"
            :permission-groups="permissionGroups"
            :active-permission-ids="activePermissionIds"
            :is-saving="isSaving"
            :is-loading="isLoading"
            :total-permission-count="totalPermissionCount"
            @toggle-permission="togglePermission"
            @toggle-module="toggleModule"
            @toggle-all="toggleAll"
            @save-permissions="handleSavePermissions"
          />
        </div>
      </div>
    </div>

    <!-- Toast Notification (dùng component có sẵn của dự án) -->
    <Teleport to="body">
      <Toast
        v-if="toastConfig.isVisible"
        :message="toastConfig.message"
        :type="toastConfig.type"
        @close="toastConfig.isVisible = false"
      />
    </Teleport>
  </div>
</template>

<script setup>
import { reactive, onMounted } from 'vue'
import { ShieldCheck } from 'lucide-vue-next'
import RoleSidebar from '@/components/permission-management/RoleSidebar.vue'
import PermissionMatrix from '@/components/permission-management/PermissionMatrix.vue'
import Toast from '@/components/Toast.vue'
import { usePermissionManagement } from '@/composables/usePermissionManagement'

// ── Composable (chứa toàn bộ logic) ──────────────────────────────────────────
const {
  roleList,
  permissionGroups,
  selectedRole,
  activePermissionIds,
  isLoading,
  isSaving,
  totalPermissionCount,
  activeRoleId,
  selectRole,
  togglePermission,
  toggleModule,
  toggleAll,
  savePermissions,
  initPermissionModule,
} = usePermissionManagement()

// ── Toast State ──────────────────────────────────────────────────────────────
const toastConfig = reactive({
  isVisible: false,
  message: '',
  type: 'success',
})

const showToastNotification = (type, message) => {
  toastConfig.isVisible = false
  // Dùng nextTick để đảm bảo Toast được re-mount (reset timer)
  setTimeout(() => {
    toastConfig.type = type
    toastConfig.message = message
    toastConfig.isVisible = true
  }, 50)
}

// ── Event Handlers ───────────────────────────────────────────────────────────
const handleRoleSelected = (role) => {
  selectRole(role)
}

const handleSavePermissions = async () => {
  const result = await savePermissions()
  showToastNotification(
    result.success ? 'success' : 'error',
    result.message
  )
}

// ── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(() => {
  initPermissionModule()
})
</script>

<style scoped>
.permission-management-page {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  padding: 24px;
  padding-bottom: 48px;
  overflow-y: auto;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.permission-management-container {
  max-width: 1280px;
  margin: 0 auto;
  width: 100%;
}

/* Page Header */
.permission-management-page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 32px;
}
.permission-management-page-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1e293b;
  display: flex;
  align-items: center;
  gap: 12px;
  margin: 0;
}
.permission-management-title-icon {
  width: 48px;
  height: 48px;
  background: #6366f1;
  color: #fff;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(99, 102, 241, 0.3);
}
.permission-management-page-subtitle {
  font-size: 10px;
  font-weight: 600;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.2em;
  margin-top: 8px;
}

/* Two-Column Layout */
.permission-management-layout {
  display: flex;
  gap: 28px;
}
.permission-management-sidebar {
  width: 280px;
  flex-shrink: 0;
}
.permission-management-main {
  flex: 1;
  min-width: 0;
}

/* Responsive */
@media (max-width: 1024px) {
  .permission-management-layout {
    flex-direction: column;
  }
  .permission-management-sidebar {
    width: 100%;
  }
}
</style>
