<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in-up pb-12 pr-4 scrollbar-premium overflow-y-auto font-sans">
    <div class="max-w-7xl mx-auto w-full p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-8 glass-header p-6 rounded-[2rem] shadow-sm">
        <div>
          <h1 class="text-3xl font-black text-slate-900 tracking-tight italic uppercase">Nhật Ký Thao Tác (Audit Log)</h1>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Theo dõi mọi hành động Thêm/Xóa/Sửa trên hệ thống.</p>
        </div>
        <div class="flex gap-3">
          <input type="date" v-model="startDate" class="px-4 py-2 border border-slate-200 rounded-xl text-sm font-bold text-slate-700 focus:outline-none focus:border-primary" />
          <span class="px-2 py-2 text-slate-400">→</span>
          <input type="date" v-model="endDate" class="px-4 py-2 border border-slate-200 rounded-xl text-sm font-bold text-slate-700 focus:outline-none focus:border-primary" />
          <button @click="loadLogs" class="w-12 h-12 bg-white rounded-2xl shadow-sm border border-slate-100 text-slate-400 hover:text-primary transition-all flex items-center justify-center">
            <RefreshCw class="w-5 h-5" :class="{ 'animate-spin': loading }" />
          </button>
        </div>
      </div>

      <!-- Filters -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <div class="premium-card p-4 flex items-center gap-4">
          <div class="w-10 h-10 bg-slate-100 rounded-xl flex items-center justify-center text-slate-400">
            <User class="w-5 h-5" />
          </div>
          <div class="flex-1">
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Người dùng</p>
            <input v-model="filterUsername" @input="loadLogs" placeholder="Tên đăng nhập..." class="w-full text-sm font-bold text-slate-700 bg-transparent focus:outline-none" />
          </div>
        </div>
        <div class="premium-card p-4 flex items-center gap-4">
          <div class="w-10 h-10 bg-slate-100 rounded-xl flex items-center justify-center text-slate-400">
            <Package class="w-5 h-5" />
          </div>
          <div class="flex-1">
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Module</p>
            <input v-model="filterModule" @input="loadLogs" placeholder="Hợp đồng, Đoàn khám..." class="w-full text-sm font-bold text-slate-700 bg-transparent focus:outline-none" />
          </div>
        </div>
        <div class="premium-card p-4 flex items-center gap-4">
          <div class="w-10 h-10 bg-slate-100 rounded-xl flex items-center justify-center text-slate-400">
            <Activity class="w-5 h-5" />
          </div>
          <div class="flex-1">
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Hành động</p>
            <select v-model="filterAction" @change="loadLogs" class="w-full text-sm font-bold text-slate-700 bg-transparent focus:outline-none appearance-none">
              <option value="">Tất cả</option>
              <option value="CREATE">CREATE</option>
              <option value="UPDATE">UPDATE</option>
              <option value="DELETE">DELETE</option>
              <option value="LOGIN">LOGIN</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Logs Table -->
      <div class="premium-card overflow-hidden flex flex-col">
        <div class="overflow-x-auto">
          <table class="w-full text-left">
            <thead class="bg-slate-50/50">
              <tr>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100">Thời Gian</th>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100">Người Dùng</th>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100">Hành Động</th>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100">Module</th>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100">Mô Tả</th>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100 text-right">IP Address</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-if="logs.length === 0">
                <td colspan="6" class="py-16 text-center">
                  <ShieldAlert class="w-12 h-12 text-slate-200 mx-auto mb-3" />
                  <p class="text-xs font-black text-slate-400 uppercase tracking-widest">Không có nhật ký thao tác nào</p>
                </td>
              </tr>
              <tr v-for="log in logs" :key="log.auditLogId" class="hover:bg-slate-50/50 transition-colors group">
                <td class="px-6 py-4">
                  <p class="font-bold text-slate-800 text-xs">{{ formatDateTime(log.createdAt) }}</p>
                </td>
                <td class="px-6 py-4">
                  <div class="flex items-center gap-2">
                    <div class="w-6 h-6 bg-primary/10 text-primary rounded-full flex items-center justify-center text-[10px] font-black">
                      {{ log.username.substring(0, 1).toUpperCase() }}
                    </div>
                    <span class="font-black text-slate-700 text-xs tracking-tight">{{ log.username }}</span>
                  </div>
                </td>
                <td class="px-6 py-4">
                  <span :class="getActionClass(log.action)" class="px-3 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest">
                    {{ log.action }}
                  </span>
                </td>
                <td class="px-6 py-4">
                  <span class="text-[10px] font-black text-slate-500 uppercase tracking-widest">{{ log.module }}</span>
                </td>
                <td class="px-6 py-4">
                  <p class="text-xs text-slate-600 line-clamp-1 group-hover:line-clamp-none transition-all">{{ log.description }}</p>
                </td>
                <td class="px-6 py-4 text-right">
                  <span class="font-mono text-[10px] text-slate-400">{{ log.ipAddress || 'N/A' }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { RefreshCw, User, Package, Activity, ShieldAlert } from 'lucide-vue-next'
import apiClient from '../services/apiClient'
import { useToast } from '../composables/useToast'

const toast = useToast()
const loading = ref(false)
const logs = ref([])

// Filters
const today = new Date()
const startDate = ref(new Date(today.getFullYear(), today.getMonth(), 1).toISOString().split('T')[0])
const endDate = ref(today.toISOString().split('T')[0])
const filterUsername = ref('')
const filterModule = ref('')
const filterAction = ref('')

const loadLogs = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/api/AuditLog', {
      params: {
        start: startDate.value,
        end: endDate.value,
        username: filterUsername.value,
        module: filterModule.value,
        action: filterAction.value
      }
    })
    logs.value = res.data
  } catch (err) {
    console.error('Lỗi tải nhật ký', err)
    toast.show('Lỗi tải nhật ký thao tác', 'error')
  } finally {
    loading.value = false
  }
}

const formatDateTime = (dateStr) => {
  const d = new Date(dateStr)
  return d.toLocaleString('vi-VN')
}

const getActionClass = (action) => {
  const classes = {
    'CREATE': 'bg-emerald-100 text-emerald-700',
    'UPDATE': 'bg-blue-100 text-blue-700',
    'DELETE': 'bg-rose-100 text-rose-700',
    'LOGIN': 'bg-amber-100 text-amber-700'
  }
  return classes[action] || 'bg-slate-100 text-slate-700'
}

onMounted(() => {
  loadLogs()
})
</script>
