<template>
  <div class="checkin-page">
    <!-- Nếu chưa có token QR -->
    <div v-if="!tokenData" class="scan-landing">
      <div class="brand-logo">🏥</div>
      <h1>Chấm công Đoàn khám</h1>
      <p class="subtitle">Quét mã QR do trưởng đoàn cung cấp để check-in/out tự động</p>

      <div v-if="urlToken" class="processing">
        <div class="spinner-lg"></div>
        <p>Đang xác thực mã QR...</p>
      </div>

      <div v-else class="manual-input">
        <p class="hint">Hoặc nhập mã token thủ công:</p>
        <input v-model="manualToken" type="text" placeholder="Dán mã QR token vào đây..."
          class="token-input" />
        <button class="btn-checkin" @click="processToken(manualToken)">
          <i class="fas fa-qrcode"></i> Xác nhận
        </button>
      </div>
    </div>

    <!-- Đã có token, hiển thị form check-in -->
    <div v-else class="checkin-form">
      <div class="group-info">
        <div class="group-badge">🩺</div>
        <h2>{{ tokenData.groupName }}</h2>
        <p class="exam-date">
          <i class="fas fa-calendar-alt"></i>
          {{ formatDate(tokenData.examDate) }}
        </p>
        <span class="expires-badge">
          <i class="fas fa-clock"></i>
          Hết hạn: {{ formatDateTime(tokenData.expiresAt) }}
        </span>
      </div>

      <!-- Trạng thái đã check-in -->
      <div v-if="result" class="result-card" :class="result.action === 'CheckIn' ? 'checkin-result' : 'checkout-result'">
        <div class="result-icon">
          {{ result.action === 'CheckIn' ? '✅' : '🎉' }}
        </div>
        <h3>{{ result.message }}</h3>
        <p class="result-time">{{ formatDateTime(result.time) }}</p>
        <p v-if="result.shiftType" class="shift-badge">
          Công quy đổi: <strong>{{ result.shiftType }} công</strong>
        </p>
        <button class="btn-secondary" @click="resetForm">Chấm công nhân viên khác</button>
      </div>

      <!-- Form check-in -->
      <div v-else class="form-card">
        <div class="form-group">
          <label>Mã nhân viên <span class="required">*</span></label>
          <input v-model.number="staffId" type="number"
            placeholder="Nhập mã số nhân viên (StaffId)..."
            class="form-input" @keyup.enter="doCheckIn" />
          <p class="form-hint">Liên hệ trưởng đoàn nếu không biết mã số.</p>
        </div>

        <div class="form-group">
          <label>Ghi chú (không bắt buộc)</label>
          <input v-model="note" type="text" placeholder="VD: Đến muộn 10 phút..."
            class="form-input" />
        </div>

        <button class="btn-checkin" @click="doCheckIn" :disabled="!staffId || loading">
          <span v-if="loading"><i class="fas fa-spinner fa-spin"></i> Đang ghi nhận...</span>
          <span v-else><i class="fas fa-fingerprint"></i> Check-in / Check-out</span>
        </button>

        <p v-if="error" class="error-msg">
          <i class="fas fa-exclamation-triangle"></i> {{ error }}
        </p>
      </div>
    </div>

    <!-- Footer -->
    <div class="checkin-footer">
      <p>© {{ new Date().getFullYear() }} QuanLyDoanKham — Hệ thống quản lý đoàn khám</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import apiClient from '../services/apiClient'

const route = useRoute()


const urlToken = ref('')
const manualToken = ref('')
const tokenData = ref(null)
const staffId = ref('')
const note = ref('')
const loading = ref(false)
const error = ref('')
const result = ref(null)

const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) : ''
const formatDateTime = (d) => d ? new Date(d).toLocaleString('vi-VN') : ''

const processToken = async (token) => {
  if (!token) return
  try {
    // Decode token để lấy groupId
    const raw = atob(token)
    const [gid] = raw.split(':')
    const groupId = parseInt(gid)

    if (!groupId) { error.value = 'Mã QR không hợp lệ.'; return }

    // Fetch thông tin đoàn
    const res = await apiClient.get(`/api/Attendance/qr/${groupId}`).catch(() => ({ data: { groupId, groupName: `Đoàn #${groupId}`, examDate: new Date(), expiresAt: new Date(Date.now() + 12*3600*1000), qrToken: token } }))

    tokenData.value = { ...res.data, qrToken: token }
  } catch (e) {
    // Fallback: dùng token trực tiếp mà không cần fetch
    tokenData.value = { groupId: null, groupName: 'Đoàn khám', examDate: new Date(), expiresAt: new Date(Date.now() + 12*3600*1000), qrToken: token }
  }
}

const doCheckIn = async () => {
  if (!staffId.value) return
  loading.value = true
  error.value = ''
  try {
    const res = await apiClient.post(`/api/Attendance/checkin`, {
      groupId: tokenData.value.groupId,
      staffId: staffId.value,
      qrToken: tokenData.value.qrToken,
      note: note.value
    })
    result.value = res.data
  } catch (e) {
    error.value = e.response?.data?.message || e.response?.data || 'Lỗi khi check-in. Vui lòng thử lại.'
  } finally {
    loading.value = false
  }
}

const resetForm = () => {
  staffId.value = ''
  note.value = ''
  result.value = null
  error.value = ''
}

onMounted(() => {
  urlToken.value = route.query.token || ''
  if (urlToken.value) processToken(urlToken.value)
})
</script>

<style scoped>
.checkin-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; padding: 20px; font-family: 'Inter', sans-serif;
}

.scan-landing, .checkin-form {
  background: rgba(255,255,255,0.97); border-radius: 24px;
  padding: 40px; max-width: 440px; width: 100%;
  box-shadow: 0 20px 60px rgba(0,0,0,0.3); text-align: center;
}

.brand-logo { font-size: 4rem; margin-bottom: 16px; }
h1 { font-size: 1.8rem; font-weight: 800; color: #1e293b; margin: 0 0 10px; }
.subtitle { color: #64748b; margin-bottom: 28px; line-height: 1.6; }

.processing { padding: 20px; }
.spinner-lg {
  width: 48px; height: 48px; border: 4px solid #e2e8f0;
  border-top-color: #6366f1; border-radius: 50%; animation: spin 0.8s linear infinite;
  margin: 0 auto 16px;
}
@keyframes spin { to { transform: rotate(360deg); } }

.hint { color: #94a3b8; font-size: 0.85rem; margin-bottom: 12px; }
.token-input {
  width: 100%; padding: 12px 16px; border: 2px solid #e2e8f0; border-radius: 12px;
  font-size: 0.9rem; margin-bottom: 12px; box-sizing: border-box;
}
.token-input:focus { outline: none; border-color: #6366f1; }

.btn-checkin {
  width: 100%; padding: 14px; border: none; border-radius: 14px;
  background: linear-gradient(135deg, #6366f1, #8b5cf6); color: #fff;
  font-size: 1rem; font-weight: 700; cursor: pointer; transition: all .2s;
  display: flex; align-items: center; justify-content: center; gap: 10px;
}
.btn-checkin:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 8px 24px rgba(99,102,241,0.4); }
.btn-checkin:disabled { opacity: 0.6; cursor: not-allowed; }

/* Group info */
.group-badge { font-size: 3.5rem; margin-bottom: 12px; }
h2 { font-size: 1.5rem; font-weight: 700; color: #1e293b; margin: 0 0 8px; }
.exam-date { color: #475569; margin: 0 0 12px; font-size: 0.9rem; }
.expires-badge {
  display: inline-flex; align-items: center; gap: 6px;
  background: #fef3c7; color: #92400e; padding: 4px 12px;
  border-radius: 20px; font-size: 0.78rem; font-weight: 600; margin-bottom: 24px;
}

/* Form card */
.form-card { margin-top: 20px; }
.form-group { text-align: left; margin-bottom: 16px; }
.form-group label { display: block; font-weight: 600; margin-bottom: 6px; font-size: 0.9rem; color: #374151; }
.form-input {
  width: 100%; padding: 12px 14px; border: 2px solid #e2e8f0; border-radius: 10px;
  font-size: 0.95rem; box-sizing: border-box; transition: border-color .2s;
}
.form-input:focus { outline: none; border-color: #6366f1; }
.form-hint { font-size: 0.78rem; color: #94a3b8; margin: 4px 0 0; }
.required { color: #ef4444; }

/* Result */
.result-card {
  border-radius: 16px; padding: 28px; margin-bottom: 20px;
}
.checkin-result { background: linear-gradient(135deg, #d1fae5, #a7f3d0); }
.checkout-result { background: linear-gradient(135deg, #dbeafe, #bfdbfe); }
.result-icon { font-size: 3rem; margin-bottom: 12px; }
.result-card h3 { font-size: 1.2rem; font-weight: 700; color: #1e293b; margin: 0 0 8px; }
.result-time { color: #475569; font-size: 0.85rem; margin: 0 0 10px; }
.shift-badge {
  display: inline-block; background: rgba(255,255,255,0.7); padding: 6px 14px;
  border-radius: 20px; font-size: 0.9rem; margin-bottom: 16px;
}

.btn-secondary {
  padding: 10px 20px; border: 2px solid #6366f1; border-radius: 10px;
  background: transparent; color: #6366f1; font-weight: 600; cursor: pointer;
  transition: all .15s;
}
.btn-secondary:hover { background: #6366f1; color: #fff; }

.error-msg {
  color: #ef4444; font-size: 0.88rem; background: #fef2f2;
  border: 1px solid #fecaca; border-radius: 8px; padding: 10px 14px; margin-top: 12px;
}

.checkin-footer { color: rgba(255,255,255,0.6); font-size: 0.78rem; margin-top: 24px; }
</style>
