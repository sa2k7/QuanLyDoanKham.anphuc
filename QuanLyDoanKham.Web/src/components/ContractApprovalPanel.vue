<template>
  <!-- Panel workflows phê duyệt hợp đồng — nhúng vào Contracts.vue -->
  <div class="approval-panel">
    <!-- Header trạng thái hiện tại -->
    <div class="status-banner" :class="statusClass">
      <span class="status-icon">{{ statusIcon }}</span>
      <div>
        <strong>{{ statusLabel }}</strong>
        <p v-if="contract.approvalStep > 0" class="step-text">
          Đang ở bước {{ contract.currentApprovalStep }} / {{ totalSteps }}
        </p>
      </div>
    </div>

    <!-- Nút hành động tuỳ trạng thái -->
    <div class="action-buttons">
      <!-- Draft → Submit -->
      <button v-if="contract.status === 'Draft' && canCreate"
        class="btn btn-submit" @click="$emit('submit', contract.healthContractId)">
        <i class="fas fa-paper-plane"></i> Gửi duyệt
      </button>

      <!-- PendingApproval → Approve/Reject (nếu có quyền) -->
      <template v-if="contract.status === 'PendingApproval' && canApprove">
        <button class="btn btn-approve" @click="openApproveModal">
          <i class="fas fa-check-circle"></i> Phê duyệt
        </button>
        <button class="btn btn-reject" @click="openRejectModal">
          <i class="fas fa-times-circle"></i> Từ chối
        </button>
      </template>

      <!-- Approved → đã xong -->
      <div v-if="contract.status === 'Approved'" class="approved-stamp">
        <i class="fas fa-award"></i> Hợp đồng đã được phê duyệt
      </div>

      <!-- Rejected → cho phép rút về Draft -->
      <template v-if="contract.status === 'Rejected' && canCreate">
        <span class="rejected-label"><i class="fas fa-ban"></i> Đã từ chối</span>
        <button class="btn btn-reset" @click="$emit('reset', contract.healthContractId)">
          <i class="fas fa-undo"></i> Rút về Nháp
        </button>
      </template>
    </div>

    <!-- Lịch sử phê duyệt -->
    <div v-if="history.length > 0" class="history-section">
      <h4 class="history-title"><i class="fas fa-history"></i> Lịch sử phê duyệt</h4>
      <div class="history-timeline">
        <div v-for="(item, idx) in history" :key="idx"
          class="history-item" :class="item.action === 'Approve' ? 'approved' : 'rejected'">
          <div class="timeline-dot">
            {{ item.action === 'Approve' ? '✅' : '❌' }}
          </div>
          <div class="timeline-content">
            <p class="step-name">{{ item.stepName }}</p>
            <p class="action-by">
              <i class="fas fa-user"></i> Bởi <strong>{{ item.approvedByUser }}</strong>
              · {{ formatDate(item.actionDate) }}
            </p>
            <p v-if="item.note" class="action-note">
              <i class="fas fa-quote-left"></i> {{ item.note }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Phê duyệt -->
    <div v-if="showApproveModal" class="modal-overlay" @click.self="showApproveModal = false">
      <div class="modal-box">
        <div class="modal-header approve">
          <h3><i class="fas fa-check-circle"></i> Xác nhận Phê duyệt</h3>
        </div>
        <div class="modal-body">
          <p>Phê duyệt hợp đồng <strong>{{ contract.contractCode || '#' + contract.healthContractId }}</strong>?</p>
          <textarea v-model="actionNote" rows="3"
            placeholder="Ghi chú phê duyệt (không bắt buộc)..."
            class="note-input"></textarea>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" @click="showApproveModal = false">Hủy</button>
          <button class="btn btn-approve" @click="doAction('Approve')" :disabled="acting">
            <i v-if="acting" class="fas fa-spinner fa-spin"></i>
            <i v-else class="fas fa-check"></i>
            Xác nhận Phê duyệt
          </button>
        </div>
      </div>
    </div>

    <!-- Modal Từ chối -->
    <div v-if="showRejectModal" class="modal-overlay" @click.self="showRejectModal = false">
      <div class="modal-box">
        <div class="modal-header reject">
          <h3><i class="fas fa-times-circle"></i> Từ chối Hợp đồng</h3>
        </div>
        <div class="modal-body">
          <p>Từ chối hợp đồng <strong>{{ contract.contractCode || '#' + contract.healthContractId }}</strong>?</p>
          <textarea v-model="actionNote" rows="3"
            placeholder="Lý do từ chối (bắt buộc)..." required
            class="note-input"></textarea>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" @click="showRejectModal = false">Hủy</button>
          <button class="btn btn-reject" @click="doAction('Reject')"
            :disabled="acting || !actionNote.trim()">
            <i v-if="acting" class="fas fa-spinner fa-spin"></i>
            <i v-else class="fas fa-ban"></i>
            Xác nhận Từ chối
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePermission } from '@/composables/usePermission'
import apiClient from '@/services/apiClient'

const props = defineProps({
  contract: { type: Object, required: true },
  totalSteps: { type: Number, default: 2 }
})

const emit = defineEmits(['submit', 'reset', 'approved', 'rejected'])

const { canCreateContract, canApproveContract } = usePermission()
const canCreate = canCreateContract
const canApprove = canApproveContract

const history = ref([])
const showApproveModal = ref(false)
const showRejectModal = ref(false)
const actionNote = ref('')
const acting = ref(false)

const statusMap = {
  Draft:          { label: 'Nháp',             icon: '📝', cls: 'draft' },
  PendingApproval:{ label: 'Chờ phê duyệt',   icon: '⏳', cls: 'pending' },
  Approved:       { label: 'Đã phê duyệt',    icon: '✅', cls: 'approved' },
  Rejected:       { label: 'Đã từ chối',      icon: '❌', cls: 'rejected' },
}

const statusLabel = computed(() => statusMap[props.contract.status]?.label || props.contract.status)
const statusIcon  = computed(() => statusMap[props.contract.status]?.icon || '📄')
const statusClass = computed(() => statusMap[props.contract.status]?.cls || '')

const formatDate = (d) => d ? new Date(d).toLocaleString('vi-VN') : ''

const openApproveModal = () => { actionNote.value = ''; showApproveModal.value = true }
const openRejectModal  = () => { actionNote.value = ''; showRejectModal.value  = true }

const doAction = async (action) => {
  acting.value = true
  try {
    await apiClient.post(`/HealthContracts/${props.contract.healthContractId}/approve`, {
      action,
      note: actionNote.value
    })
    showApproveModal.value = false
    showRejectModal.value  = false
    emit(action === 'Approve' ? 'approved' : 'rejected')
    await loadHistory()
  } catch (e) {
    alert(e.response?.data?.message || 'Lỗi khi thực hiện hành động.')
  } finally {
    acting.value = false
  }
}

const loadHistory = async () => {
  try {
    const res = await apiClient.get(`/HealthContracts/${props.contract.healthContractId}/history`)
    history.value = res.data || []
  } catch { history.value = [] }
}

onMounted(loadHistory)
</script>

<style scoped>
.approval-panel {
  border: 1px solid var(--border, #e2e8f0);
  border-radius: 14px; padding: 20px; background: var(--card-bg, #fff);
  margin-top: 16px;
}

/* Status banner */
.status-banner {
  display: flex; align-items: center; gap: 14px; padding: 14px 18px;
  border-radius: 10px; margin-bottom: 16px; font-size: 0.95rem;
}
.status-banner.draft     { background: #f8fafc; border: 1px solid #e2e8f0; }
.status-banner.pending   { background: #fffbeb; border: 1px solid #fde68a; }
.status-banner.approved  { background: #dcfce7; border: 1px solid #86efac; }
.status-banner.rejected  { background: #fef2f2; border: 1px solid #fca5a5; }
.status-icon { font-size: 1.6rem; }
.step-text { font-size: 0.8rem; color: var(--text-muted, #64748b); margin: 2px 0 0; }

/* Action buttons */
.action-buttons { display: flex; gap: 10px; flex-wrap: wrap; margin-bottom: 20px; }
.btn {
  padding: 10px 20px; border: none; border-radius: 10px; font-weight: 600;
  cursor: pointer; font-size: 0.9rem; display: flex; align-items: center;
  gap: 8px; transition: all .15s;
}
.btn-submit  { background: #6366f1; color: #fff; }
.btn-submit:hover  { background: #4f46e5; }
.btn-approve { background: #10b981; color: #fff; }
.btn-approve:hover { background: #059669; }
.btn-reject  { background: #ef4444; color: #fff; }
.btn-reject:hover  { background: #dc2626; }
.btn-reset   { background: #f59e0b; color: #fff; }
.btn-reset:hover   { background: #d97706; }
.btn-secondary { background: var(--hover, #f1f5f9); color: var(--text-secondary, #475569); }
.btn:disabled { opacity: 0.6; cursor: not-allowed; }

.approved-stamp {
  display: flex; align-items: center; gap: 8px; color: #059669;
  font-weight: 700; font-size: 0.95rem;
}
.rejected-label {
  display: flex; align-items: center; gap: 6px; color: #ef4444;
  font-weight: 600; font-size: 0.9rem;
}

/* History */
.history-section { border-top: 1px solid var(--border, #e2e8f0); padding-top: 16px; }
.history-title { font-size: 0.9rem; font-weight: 600; color: var(--text-secondary, #475569); margin: 0 0 14px; }

.history-timeline { display: flex; flex-direction: column; gap: 12px; }
.history-item { display: flex; gap: 14px; }
.timeline-dot { font-size: 1.2rem; flex-shrink: 0; }
.timeline-content { flex: 1; padding-bottom: 12px; border-bottom: 1px solid var(--border, #f1f5f9); }
.timeline-content:last-child { border-bottom: none; }
.step-name { font-weight: 600; color: var(--text-primary, #1e293b); margin: 0 0 4px; font-size: 0.9rem; }
.action-by { color: var(--text-muted, #64748b); font-size: 0.82rem; margin: 0 0 4px; }
.action-note { color: var(--text-secondary, #475569); font-size: 0.83rem; font-style: italic; margin: 0; }

/* Modal */
.modal-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,0.5);
  display: flex; align-items: center; justify-content: center; z-index: 2000;
}
.modal-box {
  background: var(--card-bg, #fff); border-radius: 16px; width: 90%; max-width: 440px;
  overflow: hidden; box-shadow: 0 20px 60px rgba(0,0,0,0.25);
}
.modal-header {
  padding: 18px 22px; color: #fff;
  display: flex; align-items: center; gap: 10px;
}
.modal-header h3 { margin: 0; font-size: 1rem; }
.modal-header.approve { background: #10b981; }
.modal-header.reject  { background: #ef4444; }
.modal-body  { padding: 20px 22px; }
.modal-body p { color: var(--text-secondary, #475569); margin: 0 0 12px; }
.modal-footer { padding: 14px 22px; border-top: 1px solid var(--border, #e2e8f0); display: flex; justify-content: flex-end; gap: 10px; }
.note-input {
  width: 100%; padding: 10px 12px; border: 1px solid var(--border, #e2e8f0);
  border-radius: 8px; font-size: 0.9rem; resize: vertical; box-sizing: border-box;
}
.note-input:focus { outline: none; border-color: #6366f1; }
</style>
