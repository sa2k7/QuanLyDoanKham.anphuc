<template>
  <div class="role-sidebar">
    <div class="role-sidebar-header">
      <h3 class="role-sidebar-title">Danh Sách Vai Trò</h3>
    </div>
    <div class="role-sidebar-list">
      <button
        v-for="role in roleList"
        :key="role.roleId"
        class="role-sidebar-item"
        :class="{ 'is-active': activeRoleId === role.roleId }"
        @click="$emit('role-selected', role)"
      >
        <span class="role-sidebar-item-name">
          {{ role.description || role.roleName }}
        </span>
        <ChevronRight class="role-sidebar-item-arrow" />
      </button>
    </div>
  </div>
</template>

<script setup>
import { ChevronRight } from 'lucide-vue-next'

defineProps({
  roleList:     { type: Array,            required: true },
  activeRoleId: { type: [Number, null],   default: null },
})

defineEmits(['role-selected'])
</script>

<style scoped>
.role-sidebar {
  background: #fff;
  border-radius: 24px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.04);
  overflow: hidden;
}

.role-sidebar-header {
  padding: 16px 20px;
  border-bottom: 1px solid #f1f5f9;
  background: rgba(248, 250, 252, 0.5);
}
.role-sidebar-title {
  font-size: 10px;
  font-weight: 800;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.2em;
  margin: 0;
}

.role-sidebar-list {
  padding: 8px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.role-sidebar-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-radius: 12px;
  border: 1px solid transparent;
  background: transparent;
  cursor: pointer;
  transition: all 0.25s ease;
  width: 100%;
  text-align: left;
}
.role-sidebar-item:hover {
  background: #f8fafc;
}
.role-sidebar-item.is-active {
  background: rgba(99, 102, 241, 0.08);
  border-color: rgba(99, 102, 241, 0.15);
  box-shadow: 0 1px 4px rgba(99, 102, 241, 0.08);
}

.role-sidebar-item-name {
  font-size: 0.875rem;
  font-weight: 600;
  color: #64748b;
  transition: color 0.2s ease;
}
.role-sidebar-item.is-active .role-sidebar-item-name {
  color: #6366f1;
}
.role-sidebar-item:hover .role-sidebar-item-name {
  color: #334155;
}

.role-sidebar-item-arrow {
  width: 16px;
  height: 16px;
  color: #cbd5e1;
  opacity: 0;
  transform: translateX(-4px);
  transition: all 0.25s ease;
}
.role-sidebar-item:hover .role-sidebar-item-arrow {
  opacity: 0.6;
  transform: translateX(0);
}
.role-sidebar-item.is-active .role-sidebar-item-arrow {
  opacity: 1;
  transform: translateX(0);
  color: #6366f1;
}
</style>
