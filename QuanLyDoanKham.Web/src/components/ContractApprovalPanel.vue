<template>
  <!-- Panel workflows phê duyệt hợp đồng — nhúng vào Contracts.vue -->
  <div class="approval-panel bg-white/50 backdrop-blur-sm border border-slate-100 rounded-[2rem] p-4 shadow-sm mt-4 mb-2 animate-fade-in-up">
    <!-- Header trạng thái hiện tại (Compact Version) -->
    <div class="flex items-center gap-3 p-3 rounded-2xl mb-4 border transition-all duration-500" :class="statusClass">
      <div class="w-9 h-9 rounded-lg flex items-center justify-center bg-white shadow-sm border border-slate-100/50 shrink-0">
          <span class="text-xl">{{ statusIcon }}</span>
      </div>
      <div class="flex-1 min-w-0">
        <h4 class="text-sm font-black text-slate-800 tracking-tight leading-none uppercase">{{ statusLabel }}</h4>
        <p v-if="contract.currentApprovalStep > 0 && contract.status === 'PendingApproval'" class="text-[8px] font-black uppercase tracking-widest text-slate-400 mt-1 truncate">
          Bước {{ contract.currentApprovalStep }} / {{ totalSteps }}
        </p>
      </div>
    </div>

    <!-- Nút hành động tuỳ trạng thái -->
    <div class="flex flex-wrap gap-3 mb-6">
      <!-- Draft → Submit -->
      <button v-if="contract.status === 'Draft' && canCreate"
        class="flex-1 bg-gradient-to-r from-indigo-500 to-blue-600 text-white px-6 py-3 rounded-xl font-black transition-all flex items-center justify-center gap-2 shadow-lg shadow-indigo-500/20 hover:shadow-indigo-500/40 hover:-translate-y-1 active:scale-95 text-sm uppercase tracking-widest" 
        @click="$emit('submit', contract.healthContractId)">
        <Send class="w-4 h-4" /> <span>GỬI PHÊ DUYỆT</span>
      </button>

      <!-- PendingApproval → Approve/Reject (nếu có quyền) -->
      <template v-if="contract.status === 'PendingApproval' && canApprove">
        <button class="flex-1 bg-gradient-to-r from-emerald-500 to-teal-600 text-white px-8 py-4 rounded-2xl font-black transition-all flex items-center justify-center gap-3 shadow-lg shadow-emerald-500/20 hover:shadow-emerald-500/40 hover:-translate-y-1 active:scale-95" 
                @click="openApproveModal">
          <CheckCircle class="w-5 h-5" /> <span>PHÊ DUYỆT</span>
        </button>
        <button class="flex-1 bg-gradient-to-r from-rose-500 to-red-600 text-white px-8 py-4 rounded-2xl font-black transition-all flex items-center justify-center gap-3 shadow-lg shadow-rose-500/20 hover:shadow-rose-500/40 hover:-translate-y-1 active:scale-95" 
                @click="openRejectModal">
          <XCircle class="w-5 h-5" /> <span>TỪ CHỐI</span>
        </button>
      </template>

      <!-- Approved → đã xong -->
      <div v-if="contract.status === 'Approved'" class="flex-1 bg-emerald-50 text-emerald-600 p-4 rounded-2xl font-black text-xs uppercase tracking-widest flex items-center justify-center gap-3 border border-emerald-100">
        <Award class="w-5 h-5" /> Hợp đồng đã được phê duyệt hoàn toàn
      </div>

      <!-- Rejected → cho phép rút về Draft -->
      <template v-if="contract.status === 'Rejected' && canCreate">
        <div class="w-full flex items-center gap-3">
            <span class="bg-rose-50 text-rose-600 px-4 py-3 rounded-xl font-black text-xs uppercase tracking-widest border border-rose-100 flex items-center gap-2 grow truncate">
                <Ban class="w-4 h-4" /> Đã từ chối (Vui lòng kiểm tra lý do)
            </span>
            <button class="bg-amber-400 text-white px-8 py-4 rounded-2xl font-black transition-all flex items-center justify-center gap-3 shadow-lg shadow-amber-500/20 hover:shadow-amber-500/40 hover:-translate-y-1" 
                    @click="$emit('reset', contract.healthContractId)">
              <Undo class="w-5 h-5" /> <span>RÚT VỀ NHÁP</span>
            </button>
        </div>
      </template>
    </div>

    <!-- Lịch sử phê duyệt -->
    <div v-if="history.length > 0" class="mt-6 pt-6 border-t border-slate-100 items-start">
      <h4 class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-4 flex items-center gap-2">
          <History class="w-3 h-3" /> Lịch sử phê duyệt quy trình
      </h4>
      <div class="space-y-3">
        <div v-for="(item, idx) in history" :key="idx"
          class="flex gap-4 p-4 rounded-xl border transition-all hover:shadow-md" 
          :class="item.action === 'Approved' ? 'bg-emerald-50/30 border-emerald-100/50' : 'bg-rose-50/30 border-rose-100/50'">
          <div class="w-10 h-10 rounded-xl bg-white flex items-center justify-center border shadow-sm shrink-0">
            <CheckCircle v-if="item.action === 'Approved'" class="w-5 h-5 text-emerald-500" />
            <XCircle v-else class="w-5 h-5 text-rose-500" />
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex justify-between items-start mb-1 gap-2">
                <p class="text-xs font-black text-slate-800 uppercase tracking-tight">{{ item.stepName }}</p>
                <span class="text-[9px] font-black text-slate-300 uppercase shrink-0">{{ formatDate(item.actionDate) }}</span>
            </div>
            <p class="text-[10px] text-slate-400 font-bold mb-2">
              Bởi <span class="text-slate-600 uppercase tracking-widest">{{ item.approvedByName }}</span>
            </p>
            <div v-if="item.note" class="bg-white/50 p-2.5 rounded-lg border border-slate-50 italic text-[11px] text-slate-500 relative">
              <Quote class="w-3 h-3 absolute -top-1.5 -left-1 text-slate-200 fill-slate-200" />
               {{ item.note }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Phê duyệt -->
    <Teleport to="body">
        <div v-if="showApproveModal" class="fixed inset-0 z-[2000] bg-slate-900/40 backdrop-blur-sm flex items-center justify-center p-4 animate-fade-in" @click.self="showApproveModal = false">
          <div class="bg-white rounded-[2rem] w-full max-w-md overflow-hidden shadow-2xl animate-zoom-in border border-slate-100">
            <div class="bg-gradient-to-r from-emerald-500 to-teal-600 p-8 text-white">
              <h3 class="text-xl font-black flex items-center gap-3">
                  <CheckCircle class="w-6 h-6" /> XÁC NHẬN PHÊ DUYỆT
              </h3>
            </div>
            <div class="p-8">
              <p class="text-slate-500 font-medium mb-6">Bạn có chắc chắn muốn phê duyệt hợp đồng <strong class="text-slate-800">{{ contract.contractCode || '#' + contract.healthContractId }}</strong>?</p>
              <textarea v-model="actionNote" rows="3"
                placeholder="Ghi chú phê duyệt (không bắt buộc)..."
                class="w-full input-premium bg-slate-50 border-slate-200 focus:bg-white min-h-[100px]"></textarea>
            </div>
            <div class="p-8 pt-0 flex gap-3">
              <button class="flex-1 bg-slate-100 text-slate-500 px-6 py-4 rounded-xl font-black text-xs uppercase" @click="showApproveModal = false">Hủy</button>
              <button class="flex-[1.5] bg-emerald-500 text-white px-6 py-4 rounded-xl font-black text-xs uppercase shadow-lg shadow-emerald-500/20 hover:bg-emerald-600 flex items-center justify-center gap-2" 
                      @click="doAction('Approve')" :disabled="acting">
                <CheckCircle v-if="!acting" class="w-4 h-4" />
                <div v-else class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                XÁC NHẬN DUYỆT
              </button>
            </div>
          </div>
        </div>
    </Teleport>

    <!-- Modal Từ chối -->
    <Teleport to="body">
        <div v-if="showRejectModal" class="fixed inset-0 z-[2000] bg-slate-900/40 backdrop-blur-sm flex items-center justify-center p-4 animate-fade-in" @click.self="showRejectModal = false">
          <div class="bg-white rounded-[2rem] w-full max-w-md overflow-hidden shadow-2xl animate-zoom-in border border-slate-100">
            <div class="bg-gradient-to-r from-rose-500 to-red-600 p-8 text-white">
              <h3 class="text-xl font-black flex items-center gap-3">
                  <XCircle class="w-6 h-6" /> TỪ CHỐI HỢP ĐỒNG
              </h3>
            </div>
            <div class="p-8">
              <p class="text-slate-500 font-medium mb-6">Xác nhận từ chối hợp đồng <strong class="text-slate-800">{{ contract.contractCode || '#' + contract.healthContractId }}</strong>?</p>
              <textarea v-model="actionNote" rows="3"
                placeholder="Lý do từ chối (bắt buộc)..." required
                class="w-full input-premium bg-rose-50 border-rose-100 focus:bg-white min-h-[100px] text-rose-700"></textarea>
            </div>
            <div class="p-8 pt-0 flex gap-3">
              <button class="flex-1 bg-slate-100 text-slate-500 px-6 py-4 rounded-xl font-black text-xs uppercase" @click="showRejectModal = false">Hủy</button>
              <button class="flex-[1.5] bg-rose-500 text-white px-6 py-4 rounded-xl font-black text-xs uppercase shadow-lg shadow-rose-500/20 hover:bg-rose-600 flex items-center justify-center gap-2" 
                      @click="doAction('Reject')"
                :disabled="acting || !actionNote.trim()">
                <XCircle v-if="!acting" class="w-4 h-4" />
                <div v-else class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                XÁC NHẬN TỪ CHỐI
              </button>
            </div>
          </div>
        </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePermission } from '@/composables/usePermission'
import apiClient from '@/services/apiClient'
import { Send, CheckCircle, XCircle, Award, Undo, History, User, Quote, Ban } from 'lucide-vue-next'

const props = defineProps({
  contract: { type: Object, required: true },
  totalSteps: { type: Number, default: 2 }
})

const emit = defineEmits(['submit', 'reset', 'approved', 'rejected'])

const { can } = usePermission()
const canCreate = computed(() => can('HopDong.Create'))
const canApprove = computed(() => can('HopDong.Approve'))

const history = ref([])
const showApproveModal = ref(false)
const showRejectModal = ref(false)
const actionNote = ref('')
const acting = ref(false)

const statusMap = {
  Draft:           { label: 'Bản nháp',          icon: '📝', cls: 'bg-slate-50 border-slate-100' },
  PendingApproval: { label: 'Đang chờ phê duyệt', icon: '⏳', cls: 'bg-amber-50 border-amber-100 shadow-sm shadow-amber-500/5' },
  Approved:        { label: 'Đã phê duyệt',     icon: '✅', cls: 'bg-blue-50 border-blue-100 shadow-sm shadow-blue-500/5' },
  Rejected:        { label: 'Bị từ chối',       icon: '❌', cls: 'bg-rose-50 border-rose-100 shadow-sm shadow-rose-500/5' },
}

const statusLabel = computed(() => statusMap[props.contract.status]?.label || props.contract.status)
const statusIcon  = computed(() => statusMap[props.contract.status]?.icon || '📄')
const statusClass = computed(() => statusMap[props.contract.status]?.cls || '')

const formatDate = (d) => d ? new Date(d).toLocaleString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit', year: 'numeric' }) : ''

const openApproveModal = () => { actionNote.value = ''; showApproveModal.value = true }
const openRejectModal  = () => { actionNote.value = ''; showRejectModal.value  = true }

const doAction = async (action) => {
  acting.value = true
  try {
    const endpoint = action.toLowerCase() === 'approve' ? 'approve' : 'reject';
    await apiClient.post(`/api/Contracts/${props.contract.healthContractId}/${endpoint}`, {
      note: actionNote.value
    })
    showApproveModal.value = false
    showRejectModal.value  = false
    emit(action === 'Approve' ? 'approved' : 'rejected')
    await loadHistory()
  } catch (e) {
    console.error(e)
    alert(e.response?.data?.message || 'Lỗi khi thực hiện hành động.')
  } finally {
    acting.value = false
  }
}

const loadHistory = async () => {
  if (!props.contract?.healthContractId) return;
  try {
    // In HealthContractService, contract object contains ApprovalHistories
    if (props.contract.approvalHistories) {
        history.value = props.contract.approvalHistories;
    }
  } catch { history.value = [] }
}

onMounted(loadHistory)
</script>

<style scoped>
.animate-fade-in { animation: fadeIn 0.3s ease-out; }
.animate-zoom-in { animation: zoomIn 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275); }

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes zoomIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}
</style>
