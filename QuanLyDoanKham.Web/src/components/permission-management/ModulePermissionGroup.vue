<template>
  <div class="module-group">
    <!-- Module Header -->
    <div class="module-group-header">
      <div class="module-group-title">
        <span class="module-group-icon">{{ moduleIcon }}</span>
        <span class="module-group-name">{{ moduleLabel }}</span>
      </div>
      <label class="module-group-toggle-all">
        <input
          type="checkbox"
          class="module-group-checkbox"
          :checked="isAllChecked"
          @change="handleToggleAll($event.target.checked)"
        />
        <div class="module-toggle-track" :class="isAllChecked ? 'is-active' : isPartialChecked ? 'is-partial' : ''">
          <div class="module-toggle-thumb"></div>
        </div>
        <span class="module-group-toggle-label">Tất cả</span>
      </label>
    </div>

    <!-- Permission Items -->
    <div class="module-group-permissions">
      <ToggleSwitch
        v-for="permission in permissions"
        :key="permission.permissionId"
        :model-value="isPermissionActive(permission.permissionId)"
        :label="permission.permissionName"
        :sublabel="permission.permissionKey"
        @update:model-value="handleTogglePermission(permission.permissionId)"
      />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import ToggleSwitch from './ToggleSwitch.vue'

const props = defineProps({
  moduleKey:           { type: String, required: true },
  moduleLabel:         { type: String, required: true },
  moduleIcon:          { type: String, default: '🔒' },
  permissions:         { type: Array,  required: true },
  activePermissionIds: { type: Array,  required: true },
})

const emit = defineEmits(['toggle-permission', 'toggle-module'])

const isPermissionActive = (permissionId) =>
  props.activePermissionIds.includes(permissionId)

const isAllChecked = computed(() =>
  props.permissions.length > 0 &&
  props.permissions.every(p => props.activePermissionIds.includes(p.permissionId))
)

const isPartialChecked = computed(() => {
  const checkedCount = props.permissions.filter(p =>
    props.activePermissionIds.includes(p.permissionId)
  ).length
  return checkedCount > 0 && checkedCount < props.permissions.length
})

const handleTogglePermission = (permissionId) => {
  emit('toggle-permission', permissionId)
}

const handleToggleAll = (isChecked) => {
  emit('toggle-module', props.moduleKey, isChecked)
}
</script>

<style scoped>
.module-group {
  background: rgba(248, 250, 252, 0.5);
  border-radius: 16px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
  transition: border-color 0.2s ease;
}
.module-group:hover {
  border-color: #cbd5e1;
}

/* Header */
.module-group-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 20px;
  border-bottom: 1px solid #f1f5f9;
  background: rgba(255, 255, 255, 0.6);
}
.module-group-title {
  display: flex;
  align-items: center;
  gap: 10px;
}
.module-group-icon {
  font-size: 1.15rem;
}
.module-group-name {
  font-weight: 700;
  font-size: 0.8rem;
  color: #334155;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

/* Toggle All */
.module-group-toggle-all {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}
.module-group-checkbox {
  position: absolute;
  width: 1px; height: 1px;
  overflow: hidden;
  clip: rect(0,0,0,0);
}
.module-toggle-track {
  position: relative;
  width: 36px; height: 20px;
  border-radius: 10px;
  background: #cbd5e1;
  transition: background 0.3s ease;
}
.module-toggle-track.is-active { background: #6366f1; }
.module-toggle-track.is-partial { background: #a5b4fc; }
.module-toggle-thumb {
  position: absolute;
  top: 2px; left: 2px;
  width: 16px; height: 16px;
  border-radius: 50%;
  background: #fff;
  transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
  box-shadow: 0 1px 3px rgba(0,0,0,0.12);
}
.module-toggle-track.is-active .module-toggle-thumb,
.module-toggle-track.is-partial .module-toggle-thumb {
  transform: translateX(16px);
}
.module-group-toggle-label {
  font-size: 10px;
  font-weight: 700;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.15em;
}
.module-group-toggle-all:hover .module-group-toggle-label {
  color: #6366f1;
}

/* Permissions List */
.module-group-permissions {
  padding: 8px 10px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}
</style>
