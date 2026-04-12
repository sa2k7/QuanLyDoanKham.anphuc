<template>
  <div class="checkin-master-page">
    <div class="glass-container animate-fade-in">
      <header class="master-header">
         <div class="premium-logo">
           <div class="logo-icon"><Hospital :size="20" /></div>
           <span>QuanLyDoanKham</span>
         </div>
         <h1 class="gradient-text">HỆ THỐNG QUÉT MÃ QR</h1>
         <p class="subtitle">Tích hợp Chấm công & Đón bệnh nhân (Check-in)</p>
      </header>

      <!-- Mode Selector -->
      <div class="mode-selector">
         <button :class="['mode-btn', { active: scanMode === 'patient' }]" @click="switchMode('patient')">
           <UserCheck :size="18" /> Bệnh nhân
         </button>
         <button :class="['mode-btn', { active: scanMode === 'staff' }]" @click="switchMode('staff')">
           <Fingerprint :size="18" /> Nhân sự
         </button>
      </div>

      <!-- Kiosk Mode Toggle (Admin/Receptionist Only) -->
      <div v-if="can('ChamCong.QR')" class="kiosk-toggle-row">
        <button @click="toggleKioskMode" :class="['kiosk-mode-btn', { active: isKioskMode }]">
          <Monitor :size="16" /> {{ isKioskMode ? 'Đóng chế độ Bảng tiếp đón' : 'Chế độ Bảng tiếp đón (Kiosk)' }}
        </button>
      </div>

      <!-- KIOSK DISPLAY AREA -->
      <div v-if="isKioskMode" class="kiosk-display-area animate-fade-in">
        <div class="qr-kiosk-card">
           <div class="kiosk-header">
              <QrCode :size="24" class="text-primary" />
              <h3>VUI LÒNG QUÉT MÃ</h3>
           </div>
           
           <div v-if="loadingQr" class="qr-loading">
              <Loader2 :size="40" class="animate-spin text-slate-300" />
              <p>Đang tạo mã bảo mật...</p>
           </div>
           <div v-else-if="activeQr" class="qr-image-box animate-zoom-in">
              <img :src="'data:image/png;base64,' + activeQr.pngBase64" alt="Active QR" class="main-qr" />
              <div class="qr-expiry-info">
                 <RefreshCw :size="12" class="animate-spin-slow" />
                 Làm mới sau mỗi 60s
              </div>
           </div>
            <div v-else class="qr-empty-state">
               <AlertCircle :size="40" class="text-rose-300 mb-4 mx-auto" />
               <p class="text-slate-600 font-bold max-w-[280px] mx-auto leading-relaxed">
                 {{ qrError || 'Không có đoàn khám nào đang diễn ra' }}
               </p>
               <div v-if="qrError?.includes('Draft')" class="mt-6 px-4 py-3 bg-amber-50 border border-amber-100 rounded-xl">
                  <p class="text-[11px] text-amber-700 font-medium">Hệ thống đã nhận diện được đoàn khám, nhưng bạn chưa bấm "Mở đoàn" (Open) ở mục Quản lý đoàn khám.</p>
               </div>
            </div>

           <div class="scan-instructions">
              <p class="text-sm font-bold text-slate-600 mb-2">HƯỚNG DẪN:</p>
              <ul class="text-xs text-slate-500 text-left list-disc pl-5 space-y-1">
                <li v-if="scanMode === 'staff'">Mở điện thoại cá nhân -> Truy cập hệ thống -> Quét mã này để chấm công.</li>
                <li v-else>Mở Camera điện thoại -> Quét mã này -> Nhập mã hồ sơ để báo danh.</li>
              </ul>
           </div>
        </div>

        <!-- RECENT ACTIVITY FEEDBACK -->
        <div class="recent-checkins-card mt-6">
           <h4 class="flex items-center gap-2 mb-4 text-sm font-black text-slate-700">
             <Activity :size="16" class="text-green-500" /> VỪA ĐẾN
           </h4>
           <div class="activity-list">
              <div v-for="item in recentCheckins" :key="item.calendarId" class="activity-item animate-slide-right">
                 <div class="activity-avatar"><User :size="14" /></div>
                 <div class="activity-info">
                    <span class="activity-name">{{ item.staffName }}</span>
                    <span class="activity-time">{{ formatTime(item.checkInTime) }}</span>
                 </div>
                 <div class="activity-status"><Check :size="14" /></div>
              </div>
              <p v-if="recentCheckins.length === 0" class="text-center text-xs text-slate-400 py-4">Chưa có ai check-in hôm nay</p>
           </div>
        </div>
      </div>

      <!-- Scanner Area (Standard Mode) -->
      <div v-if="!isKioskMode && !scannedToken" class="scanner-section">
         <div class="qr-wrapper">
           <div id="reader" class="qr-reader-premium"></div>
           <div class="scan-overlay">
              <div class="scan-line"></div>
           </div>
         </div>
         <div class="status-hint">
            <div class="pulse-dot"></div>
            <span>Đưa mã QR vào khung hình để quét tự động</span>
         </div>
         
         <div class="manual-fallback-premium">
           <label>Hoặc nhập MÃ HỒ SƠ thủ công</label>
           <div class="input-group">
             <input v-model="manualValue" 
                    type="text" 
                    placeholder="Nhập mã khám..." 
                    class="premium-input" 
                    @keyup.enter="handleManual" />
             <button @click="handleManual" class="btn-premium primary">
               <ArrowRight :size="18" />
             </button>
           </div>
         </div>

         <!-- WALK-IN CHECKIN -->
         <div v-if="scanMode === 'patient'" class="mt-4 p-4 border border-slate-200 rounded-2xl bg-white/50">
             <label class="text-xs font-bold text-slate-500 uppercase tracking-widest mb-2 block">Tra cứu thông tin bệnh nhân ngoại lệ (SĐT/CCCD/Tên)</label>
             <div class="flex gap-2">
                <input v-model="walkInSearchText" @keyup.enter="searchWalkIn" type="text" placeholder="Nhập từ khóa tìm kiếm..." class="premium-input flex-1 !py-3 !text-sm" />
                <button @click="searchWalkIn" class="btn-premium secondary px-6 !py-3" style="width: auto">
                    <Loader2 v-if="searchingWalkIn" :size="16" class="animate-spin" />
                    <span v-else>Tìm kiếm</span>
                </button>
             </div>
             
             <div v-if="walkInResults.length > 0" class="mt-4 max-h-48 overflow-y-auto w-full bg-white rounded-xl border border-slate-200">
                <div v-for="patient in walkInResults.slice(0, 5)" :key="patient.medicalRecordId" class="flex justify-between items-center p-3 border-b border-slate-100 hover:bg-slate-50">
                   <div>
                     <p class="font-bold text-sm text-slate-800">{{ patient.fullName }}</p>
                     <p class="text-xs text-slate-500">{{ patient.idCardNumber || 'Chưa cập nhật' }} - {{ patient.gender || '?' }} - Trạng thái: <strong>{{ patient.status }}</strong></p>
                   </div>
                   <button v-if="patient.status === 'CREATED' || patient.status === 'READY'" @click="manualCheckInPatient(patient.medicalRecordId)" class="bg-primary text-white text-xs font-bold px-3 py-1.5 rounded-lg hover:bg-sky-600 transition">
                     Báo Danh
                   </button>
                   <span v-else class="text-xs font-bold text-slate-400 bg-slate-100 px-3 py-1.5 rounded-lg">Đã Báo Danh</span>
                </div>
             </div>
             <p v-else-if="hasSearchedWalkIn" class="mt-3 text-xs text-slate-500 text-center">Không tìm thấy bệnh nhân nào.</p>
         </div>
      </div>

      <!-- Result View -->
      <div v-if="scannedToken" class="result-display">
          <!-- PATIENT MODE RESULT -->
          <div v-if="scanMode === 'patient'" class="patient-result">
              <div v-if="loading" class="loading-state">
                 <div class="spinner-premium"></div>
                 <p>Đang xác thực thông tin...</p>
              </div>
              
              <div v-else-if="resultStatus" class="result-card-premium" :class="resultStatus">
                  <div class="result-header">
                    <div class="icon-circle">
                      <Check v-if="resultStatus === 'success'" :size="32" />
                      <X v-else :size="32" />
                    </div>
                    <h2>{{ resultMessage }}</h2>
                  </div>

                  <div v-if="resultData" class="premium-info-box">
                      <div class="info-row">
                        <span class="label">Bệnh nhân</span>
                        <span class="value">{{ resultData.fullName }}</span>
                      </div>
                      <div class="queue-display">
                        <span class="q-label">SỐ THỨ TỰ</span>
                        <span class="q-number">{{ resultData.queueNo }}</span>
                      </div>
                      <div class="info-row">
                        <span class="label">Dịch vụ</span>
                        <span class="value">{{ resultData.serviceName || 'Khám tổng quát' }}</span>
                      </div>
                  </div>
                  
                  <div class="actions">
                    <button v-if="resultStatus === 'success'" class="btn-premium primary w-full mb-3" @click="printReceipt(resultData)">
                      <Printer :size="18" class="mr-2" /> IN PHIẾU KHÁM
                    </button>
                    <button class="btn-premium secondary w-full" @click="resetScanner">
                      <RefreshCw :size="18" class="mr-2" /> {{ resultStatus === 'success' ? 'QUÉT NGƯỜI TIẾP THEO' : 'THỬ LẠI' }}
                    </button>
                  </div>
              </div>
          </div>

          <!-- STAFF MODE RESULT -->
          <div v-if="scanMode === 'staff'" class="staff-result">
             <div v-if="loadingData" class="loading-state">
                <Loader2 :size="32" class="animate-spin text-primary" />
                <p>Đang kết nối đoàn khám...</p>
             </div>
             
             <div v-else-if="groupInfo" class="staff-flow">
                 <div class="group-header-card">
                    <div class="group-icon"><Users :size="20" /></div>
                    <div class="group-details">
                       <h3>{{ groupInfo.groupName }}</h3>
                       <p><Calendar :size="14" /> {{ formatDate(groupInfo.examDate) }}</p>
                    </div>
                 </div>

                 <!-- Checkin Success -->
                 <div v-if="resultStatus === 'success'" class="result-card-premium success">
                    <div class="result-header">
                      <div class="icon-circle">🎉</div>
                      <h3>{{ resultMessage }}</h3>
                    </div>
                    <p v-if="resultData" class="shift-info">Ca làm việc: {{ resultData.shiftType }}</p>
                    <div class="actions vertical mt-4">
                      <button class="btn-premium primary w-full" @click="resetStaffForm">Chấm công người khác</button>
                      <button class="btn-link mt-2" @click="resetScanner">Trở về Quét QR</button>
                    </div>
                 </div>

                 <!-- Staff Input Form -->
                 <div v-else class="staff-form-premium">
                    <div class="premium-form-group">
                       <label>Mã nhân viên (Staff ID)</label>
                       <div class="input-with-icon">
                         <User :size="18" />
                         <input v-model.number="staffId" type="number" placeholder="Nhập ID..." />
                       </div>
                    </div>
                    <div class="premium-form-group mt-3">
                       <label>Ghi chú (Tùy chọn)</label>
                       <div class="input-with-icon">
                         <MessageSquare :size="18" />
                         <input v-model="note" type="text" placeholder="Ghi chú nếu có..." />
                       </div>
                    </div>
                    
                    <button class="btn-premium primary w-full mt-6" @click="submitStaffCheckIn" :disabled="!staffId || loading">
                      <span v-if="!loading" class="flex items-center justify-center gap-2">
                        <Fingerprint :size="20" /> Xác nhận Chấm Công
                      </span>
                      <Loader2 v-else :size="20" class="animate-spin mx-auto" />
                    </button>
                    
                    <button class="btn-link mt-4" @click="resetScanner">Hủy / Quét mã khác</button>

                    <div v-if="errorMsg" class="error-toast animate-bounce-in">
                       <AlertCircle :size="16" /> {{ errorMsg }}
                    </div>
                 </div>
             </div>
             
             <div v-else class="result-card-premium error">
                <div class="result-header">
                  <div class="icon-circle"><X :size="32" /></div>
                  <h2>Mã QR không hợp lệ</h2>
                </div>
                <p class="error-detail">{{ errorMsg }}</p>
                <button class="btn-premium secondary w-full mt-4" @click="resetScanner">THỬ LẠI</button>
             </div>
          </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Html5QrcodeScanner } from 'html5-qrcode'
import { useAuthStore } from '../stores/auth'
import { 
  Hospital, UserCheck, Fingerprint, ArrowRight, RefreshCw, 
  Check, X, Loader2, Users, Calendar, User, MessageSquare, AlertCircle,
  Monitor, QrCode, Activity, Printer
} from 'lucide-vue-next'
import queueHub from '../services/queueHub'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const can = authStore.hasPermission

// UI State
const scanMode = ref('patient') 
const isKioskMode = ref(localStorage.getItem('kiosk_mode') === 'true')
const scannedToken = ref(null)
const manualValue = ref('')
const loading = ref(false)

// Kiosk State
const activeQr = ref(null)
const loadingQr = ref(false)
const qrError = ref('')
const recentCheckins = ref([])
let qrTimer = null
let feedTimer = null

// Result State
const resultStatus = ref('')
const resultMessage = ref('')
const resultData = ref(null)
const errorMsg = ref('')

// Walk-in State
const walkInSearchText = ref('')
const walkInResults = ref([])
const searchingWalkIn = ref(false)
const hasSearchedWalkIn = ref(false)

// Staff Specific State
const groupInfo = ref(null)
const loadingData = ref(false)
const staffId = ref('')
const note = ref('')

let scanner = null;

const toggleKioskMode = () => {
    isKioskMode.value = !isKioskMode.value
    localStorage.setItem('kiosk_mode', isKioskMode.value)
    if (isKioskMode.value) {
        if (scanner) scanner.clear()
        startKioskLoops()
    } else {
        stopKioskLoops()
        initScanner()
    }
}

const switchMode = (mode) => {
    scanMode.value = mode
    if (isKioskMode.value) {
        fetchActiveQr()
    } else {
        resetScanner()
    }
}

const formatTime = (d) => d ? new Date(d).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : ''
const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : ''

const checkUrlToken = () => {
    const urlToken = route.query.token
    if (urlToken) {
        scanMode.value = 'staff'
        processScannedData(urlToken)
    }
}

// Thử lấy thông tin nhân sự nếu đã đăng nhập
const autoDetectStaff = async () => {
    try {
        // Lấy thông tin user từ localStorage (được lưu bởi auth store)
        const userJson = localStorage.getItem('user')
        if (!userJson) return

        const userData = JSON.parse(userJson)
        const username = userData.username

        // Thử lấy profile mới nhất để có EmployeeCode
        const res = await apiClient.get('/api/Auth/profile')
        const employeeCode = res.data?.employeeCode || username
        
        if (employeeCode) {
            const staffRes = await apiClient.get(`/api/Staff/code/${employeeCode}`)
            if (staffRes.data && staffRes.data.staffId) {
                staffId.value = staffRes.data.staffId
            }
        }
    } catch (e) {
        console.warn("Auto detect staff failed", e)
    }
}

const initScanner = () => {
    if (scanner) {
        scanner.clear().catch(() => {})
    }
    
    nextTick(() => {
        const el = document.getElementById('reader')
        if (!el) return;
        
        scanner = new Html5QrcodeScanner("reader", { 
            fps: 15, 
            qrbox: { width: 250, height: 250 },
            aspectRatio: 1.0
        }, false)
        
        scanner.render(onScanSuccess, onScanError)
    })
}

const onScanSuccess = (decodedText) => {
    if (scannedToken.value) return 
    if (scanner) scanner.pause(true)
    
    try {
      const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2571/2571-preview.mp3');
      audio.volume = 0.2;
      audio.play();
    } catch(e) {}
    
    processScannedData(decodedText)
}

const onScanError = () => { }

const handleManual = () => {
    if (!manualValue.value.trim()) return
    processScannedData(manualValue.value.trim())
}

const processScannedData = async (token) => {
    scannedToken.value = token
    errorMsg.value = ''
    resultStatus.value = ''
    resultData.value = null
    resultMessage.value = ''
    
    if (scanMode.value === 'patient') {
        await processPatientCheckIn(token)
    } else {
        await fetchStaffGroupData(token)
    }
}

const processPatientCheckIn = async (token) => {
    loading.value = true
    try {
        const res = await apiClient.post('/api/CheckIn/scan', { token })
        resultStatus.value = 'success'
        resultMessage.value = 'Tiếp Đón Thành Công!'
        resultData.value = res.data
    } catch (err) {
        resultStatus.value = 'error'
        resultMessage.value = err.response?.data?.message || 'Token không hợp lệ hoặc đã quét.'
    } finally {
        loading.value = false
    }
}

const searchWalkIn = async () => {
    if (!walkInSearchText.value.trim()) return
    searchingWalkIn.value = true
    hasSearchedWalkIn.value = false
    try {
        const groupRes = await apiClient.get('/api/MedicalGroups')
        const activeGroups = groupRes.data.filter(g => g.status === 'Open' || g.status === 'InProgress')
        
        let allPatients = []
        for (const g of activeGroups) {
            const res = await apiClient.get(`/api/MedicalRecords/group/${g.groupId}`)
            allPatients = allPatients.concat(res.data)
        }
        
        const text = walkInSearchText.value.trim().toLowerCase()
        walkInResults.value = allPatients.filter(p => 
            (p.fullName && p.fullName.toLowerCase().includes(text)) || 
            (p.idCardNumber && p.idCardNumber.includes(text))
        )
        hasSearchedWalkIn.value = true
    } catch (e) {
        errorMsg.value = 'Lỗi tra cứu thông tin.' // toast is not defined here yet, actually we can just use errorMsg
    } finally {
        searchingWalkIn.value = false
    }
}

const manualCheckInPatient = async (recordId) => {
    try {
        const res = await apiClient.post(`/api/Oms/checkin/${recordId}`)
        resultStatus.value = 'success'
        resultMessage.value = 'Tiếp Đón Thành Công!'
        resultData.value = res.data.data
        scannedToken.value = "manual_" + recordId
        walkInResults.value = []
        walkInSearchText.value = ''
        
        try {
          const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2571/2571-preview.mp3');
          audio.volume = 0.2;
          audio.play();
        } catch(e) {}
    } catch (err) {
        resultStatus.value = 'error'
        resultMessage.value = err.response?.data?.message || 'Báo danh lỗi.'
        scannedToken.value = "error_manual"
        errorMsg.value = resultMessage.value
    }
}

const printReceipt = (data) => {
    if (!data) return;
    
    // Tạo 1 cửa sổ mới để in
    const printWindow = window.open('', '_blank');
    if (!printWindow) {
        toast?.error?.('Trình duyệt đã chặn popup. Vui lòng cho phép popup để in.');
        return;
    }
    
    const now = new Date();
    const timeStr = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) + ' ' + now.toLocaleDateString('vi-VN');
    
    const html = `
    <!DOCTYPE html>
    <html lang="vi">
    <head>
        <meta charset="UTF-8">
        <title>Phiếu Khám - ${data.fullName}</title>
        <style>
            @page { size: 80mm 200mm; margin: 0; }
            body { 
                font-family: 'Helvetica Neue', Arial, sans-serif; 
                width: 72mm; /* ~ 80mm roll width minus margins */
                margin: 0 auto;
                padding: 5mm;
                color: #000;
                background: #fff;
                text-align: center;
            }
            .header { border-bottom: 2px dashed #000; padding-bottom: 5mm; margin-bottom: 5mm; }
            .header h1 { font-size: 16pt; margin: 0 0 2mm 0; font-weight: bold; }
            .header p { font-size: 10pt; margin: 0; }
            .content { text-align: left; margin-bottom: 5mm; }
            .queue-box { 
                text-align: center; 
                border: 2px solid #000; 
                border-radius: 5mm;
                padding: 3mm;
                margin: 5mm 0;
            }
            .queue-box .q-label { font-size: 12pt; display: block; margin-bottom: 2mm; }
            .queue-box .q-number { font-size: 36pt; font-weight: bold; display: block; line-height: 1; }
            .patient-info { font-size: 12pt; margin-bottom: 2mm; display: flex; align-items: baseline;}
            .patient-info strong { width: 25mm; display: inline-block; font-size: 10pt; }
            .footer { border-top: 2px dashed #000; padding-top: 5mm; font-size: 10pt; text-align: center; }
            /* Basic CSS Barcode using repeating-linear-gradient - very simple approximation */
            .barcode-placeholder {
                height: 15mm;
                width: 100%;
                background: repeating-linear-gradient(
                  to right,
                  #000,
                  #000 2px,
                  #fff 3px,
                  #fff 4px,
                  #000 4px,
                  #000 7px,
                  #fff 7px,
                  #fff 9px
                );
                margin: 5mm 0;
            }
        </style>
    </head>
    <body>
        <div class="header">
            <h1>ĐA KHOA AN PHÚC</h1>
            <p>Khám sức khỏe Doanh nghiệp</p>
        </div>
        
        <div class="queue-box">
            <span class="q-label">SỐ THỨ TỰ</span>
            <span class="q-number">${data.queueNo.toString().padStart(3, '0')}</span>
        </div>
        
        <div class="content">
            <div class="patient-info"><strong>Họ tên:</strong> ${data.fullName.toUpperCase()}</div>
            <div class="patient-info"><strong>Quy trình:</strong> Khám tổng hợp</div>
        </div>
        
        <div class="barcode-placeholder"></div>
        <p style="font-size: 10pt; letter-spacing: 2px; margin-top: -3mm; padding-bottom: 5mm;">${data.medicalRecordId || '10239120'}</p>
        
        <div class="footer">
            Vui lòng ngồi chờ gọi số qua màn hình.<br/><br/>
            ${timeStr}
        </div>
        
        <script>
            window.onload = function() { window.print(); window.close(); }
        </scr` + `ipt>
    </body>
    </html>
    `;
    
    printWindow.document.open();
    printWindow.document.write(html);
    printWindow.document.close();
}

const fetchActiveQr = async () => {
    loadingQr.value = true
    qrError.value = ''
    try {
        const originParams = `?origin=${encodeURIComponent(window.location.origin)}`
        const endpoint = scanMode.value === 'staff' 
            ? `/api/Attendance/active-qr-today${originParams}`
            : `/api/Attendance/patient-qr${originParams}`
            
        const res = await apiClient.get(endpoint)
        activeQr.value = res.data
    } catch (e) {
        activeQr.value = null
        qrError.value = e.response?.data?.message || 'Không có đoàn khám nào đang diễn ra'
    } finally {
        loadingQr.value = false
    }
}

const fetchRecentCheckins = async () => {
    try {
        const res = await apiClient.get('/api/Attendance/recent-checkins')
        recentCheckins.value = res.data
    } catch (e) { }
}

const playNotificationSound = () => {
    try {
        const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/1077/1077-preview.mp3') // Soft "Ting" sound
        audio.volume = 0.5
        audio.play()
    } catch (e) { }
}

const startKioskLoops = async () => {
    fetchActiveQr()
    fetchRecentCheckins()
    qrTimer = setInterval(fetchActiveQr, 60000) // Keep QR interval

    // Phase 2: Replace polling feed with SignalR Real-Time connection
    await queueHub.start()
    queueHub.onUpdate((event, payload) => {
        if (event === 'NewPatientArrived') {
            // Rich payload — prepend directly to the recent list without API call
            recentCheckins.value.unshift({
                staffName: payload.fullName,
                checkInTime: payload.checkInAt,
                queueNo: payload.queueNo
            })
            // Keep list max 10 to avoid unbounded DOM growth
            if (recentCheckins.value.length > 10) recentCheckins.value.pop()
            playNotificationSound()
        } else if (event === 'QueueUpdated' && payload === 'ALL') {
            // Legacy fallback: refresh the full list if the richer event wasn't received
            fetchRecentCheckins()
            playNotificationSound()
        }
    })
}

const stopKioskLoops = () => {
    if (qrTimer) clearInterval(qrTimer)
}

const fetchStaffGroupData = async (token) => {
    loadingData.value = true
    try {
        let groupId = null;
        try {
            // New format: payloadBase64.signatureBase64
            const [payloadBase64] = token.split('.');
            const raw = atob(payloadBase64);
            const [gid] = raw.split(':');
            groupId = parseInt(gid);
        } catch(e) { 
            console.error("Token decode error:", e);
        }
        
        if (!groupId) throw new Error("Mã QR không đúng định dạng Chấm Công hoặc đã bị hỏng.")

        const res = await apiClient.get(`/api/Attendance/qr/${groupId}`)
        groupInfo.value = { ...res.data, qrToken: token }
        
        // Tự động điền staffId nếu đã có session
        if (!staffId.value) {
            await autoDetectStaff()
        }

        // Tự động bấm nút chấm công:
        if (staffId.value) {
            await submitStaffCheckIn()
        }
    } catch (err) {
        errorMsg.value = err.response?.data?.message || err.message || 'Không thể lấy thông tin đoàn khám.'
        groupInfo.value = null
    } finally {
        loadingData.value = false
    }
}

const submitStaffCheckIn = async () => {
    if (!staffId.value || !groupInfo.value) return
    loading.value = true
    errorMsg.value = ''
    try {
        const res = await apiClient.post(`/api/Attendance/checkin`, {
            groupId: groupInfo.value.groupId,
            staffId: staffId.value,
            qrToken: groupInfo.value.qrToken,
            note: note.value
        })
        resultStatus.value = 'success'
        resultMessage.value = res.data?.message || 'Chấm công thành công!'
        resultData.value = res.data
    } catch (e) {
        errorMsg.value = e.response?.data?.message || e.response?.data || 'Có lỗi xảy ra khi chấm công.'
        resultStatus.value = 'error'
    } finally {
        loading.value = false
    }
}

const resetStaffForm = () => {
    staffId.value = ''
    note.value = ''
    resultStatus.value = ''
    resultMessage.value = ''
    resultData.value = null
    errorMsg.value = ''
}

const resetScanner = () => {
    scannedToken.value = null
    manualValue.value = ''
    resultStatus.value = ''
    resultMessage.value = ''
    resultData.value = null
    errorMsg.value = ''
    groupInfo.value = null
    
    if (route.query.token) {
        router.replace({ ...route, query: {} })
    }
    
    initScanner()
}

onMounted(() => {
    const urlToken = route.query.token
    if (urlToken) {
        scanMode.value = 'staff'
        processScannedData(urlToken)
    } else {
        if (isKioskMode.value) {
            startKioskLoops()
        } else {
            initScanner()
        }
    }
})

onUnmounted(() => {
    stopKioskLoops()
    if (scanner) {
        scanner.clear().catch(() => {})
    }
})
</script>

<style scoped>
.checkin-master-page {
  min-height: 100vh;
  background: radial-gradient(circle at top left, #2e3b52, #0f172a);
  display: flex; align-items: center; justify-content: center;
  padding: 20px; font-family: 'Inter', sans-serif;
  color: #fff;
}

.glass-container {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 32px;
  width: 100%; max-width: 480px;
  min-height: 520px;
  padding: 32px 24px;
  box-shadow: 0 40px 100px -20px rgba(0, 0, 0, 0.6);
  color: #1e293b;
  position: relative;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.master-header { text-align: center; margin-bottom: 24px; }
.premium-logo { 
  display: inline-flex; align-items: center; gap: 8px;
  background: #f1f5f9; padding: 6px 14px; border-radius: 20px;
  font-size: 0.8rem; font-weight: 800; color: #6366f1; margin-bottom: 16px;
}
.logo-icon { width: 28px; height: 28px; background: #6366f1; color: #fff; border-radius: 8px; display: flex; align-items: center; justify-content: center; }

.gradient-text {
  font-size: 1.75rem; font-weight: 900; 
  background: linear-gradient(135deg, #1e293b 0%, #4f46e5 100%);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent;
  margin-bottom: 8px;
}

.subtitle { color: #64748b; font-size: 0.9rem; font-weight: 500; }

.mode-selector {
  display: flex; background: #f1f5f9; border-radius: 16px; padding: 5px; gap: 5px; margin-bottom: 24px;
}
.mode-btn {
  flex: 1; padding: 12px; border: none; border-radius: 12px;
  display: flex; align-items: center; justify-content: center; gap: 8px;
  font-weight: 700; font-size: 0.9rem; color: #64748b; background: transparent;
  cursor: pointer; transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
}
.mode-btn.active {
  background: #fff; color: #4f46e5;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
}

.qr-wrapper {
  position: relative; border-radius: 24px; overflow: hidden;
  border: 2px solid #e2e8f0; background: #000;
  margin-bottom: 20px; aspect-ratio: 1;
}
.qr-reader-premium { width: 100%; height: 100%; }

.scan-overlay {
  position: absolute; top: 0; left: 0; right: 0; bottom: 0;
  pointer-events: none; border: 40px solid rgba(0,0,0,0.3);
}
.scan-line {
  position: absolute; width: 100%; height: 2px;
  background: linear-gradient(to right, transparent, #4f46e5, transparent);
  animation: scanning 2s linear infinite;
}
@keyframes scanning {
  0% { top: 0; } 100% { top: 100%; }
}

.status-hint {
  display: flex; align-items: center; justify-content: center; gap: 10px;
  margin-bottom: 32px; color: #64748b; font-size: 0.9rem; font-weight: 500;
}
.pulse-dot { width: 8px; height: 8px; background: #10b981; border-radius: 50%; animation: pulse 1.5s infinite; }
@keyframes pulse { 0% { opacity: 1; transform: scale(1); } 50% { opacity: 0.4; transform: scale(1.4); } 100% { opacity: 1; transform: scale(1); } }

.manual-fallback-premium label { display: block; font-size: 0.8rem; font-weight: 700; color: #94a3b8; text-transform: uppercase; margin-bottom: 8px; }
.input-group { display: flex; gap: 8px; }
.premium-input {
  flex: 1; padding: 14px 18px; border: 2px solid #f1f5f9; border-radius: 16px;
  background: #f8fafc; font-size: 0.95rem; font-weight: 500; outline: none; transition: all 0.2s;
}
.premium-input:focus { border-color: #6366f1; background: #fff; box-shadow: 0 0 0 4px rgba(99, 102, 241, 0.1); }

.loading-state { text-align: center; padding: 40px 0; color: #64748b; }
.spinner-premium { 
  width: 48px; height: 48px; border: 4px solid #f1f5f9; border-top-color: #6366f1;
  border-radius: 50%; margin: 0 auto 16px; animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.result-card-premium { text-align: center; animation: slideUp 0.4s ease-out; }
@keyframes slideUp { from { opacity:0; transform: translateY(20px); } to { opacity:1; transform: translateY(0); } }

.icon-circle {
  width: 80px; height: 80px; border-radius: 50%; margin: 0 auto 20px;
  display: flex; align-items: center; justify-content: center;
  background: #f1f5f9; font-size: 2rem;
}
.success .icon-circle { background: #d1fae5; color: #10b981; }
.error .icon-circle { background: #fee2e2; color: #ef4444; }

.result-header h2 { font-size: 1.5rem; font-weight: 800; margin-bottom: 24px; color: #0f172a; }

.premium-info-box {
  background: #f8fafc; border-radius: 24px; padding: 24px; margin-bottom: 32px;
  border: 1px solid #e2e8f0;
}
.info-row { display: flex; justify-content: space-between; margin-bottom: 12px; font-size: 0.95rem; }
.info-row .label { color: #64748b; font-weight: 500; }
.info-row .value { color: #0f172a; font-weight: 700; }

.queue-display { margin: 24px 0; padding: 16px; background: #fff; border-radius: 16px; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px rgba(0,0,0,0.02); }
.q-label { display: block; font-size: 0.75rem; font-weight: 800; color: #94a3b8; letter-spacing: 1px; margin-bottom: 4px; }
.q-number { font-size: 4rem; font-weight: 900; color: #4f46e5; line-height: 1; }

.btn-premium {
  display: inline-flex; align-items: center; justify-content: center;
  padding: 14px 28px; border-radius: 16px; font-weight: 700; font-size: 0.95rem;
  cursor: pointer; transition: all 0.2s; border: none;
}
.btn-premium.primary { background: #4f46e5; color: #fff; }
.btn-premium.primary:hover { background: #4338ca; transform: translateY(-2px); box-shadow: 0 10px 20px -5px rgba(79, 70, 229, 0.4); }
.btn-premium.secondary { background: #f1f5f9; color: #475569; border: 1px solid #e2e8f0; }
.btn-premium.secondary:hover { background: #e2e8f0; }

.btn-link { background: transparent; border: none; color: #64748b; font-weight: 600; cursor: pointer; text-decoration: underline; }

.group-header-card {
  display: flex; align-items: center; gap: 12px;
  background: #f1f5f9; padding: 16px; border-radius: 16px; margin-bottom: 24px;
}
.group-icon { width: 40px; height: 40px; background: #fff; border-radius: 12px; display: flex; align-items: center; justify-content: center; color: #4f46e5; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }
.group-details h3 { font-size: 1rem; font-weight: 800; color: #1e293b; margin: 0; }
.group-details p { font-size: 0.8rem; color: #64748b; margin: 2px 0 0; display: flex; align-items: center; gap: 4px; }

.premium-form-group label { display: block; font-size: 0.85rem; font-weight: 700; color: #64748b; margin-bottom: 8px; text-align: left; }
.input-with-icon { position: relative; }
.input-with-icon svg { position: absolute; left: 14px; top: 50%; transform: translateY(-50%); color: #94a3b8; }
.input-with-icon input {
  width: 100%; padding: 14px 14px 14px 44px; border: 2px solid #f1f5f9; border-radius: 14px;
  background: #f8fafc; outline: none; transition: border-color .2s;
}
.input-with-icon input:focus { border-color: #6366f1; background: #fff; }

.shift-info { font-weight: 700; color: #059669; background: #d1fae5; padding: 6px 16px; border-radius: 20px; display: inline-block; margin-top: 8px; }

.error-toast { 
  margin-top: 20px; background: #fef2f2; border: 1px solid #fee2e2; color: #dc2626; 
  padding: 12px; border-radius: 12px; font-size: 0.85rem; font-weight: 600;
  display: flex; align-items: center; gap: 8px; justify-content: center;
}
@keyframes bounce-in { 0% { transform: scale(0.9); } 50% { transform: scale(1.05); } 100% { transform: scale(1); } }
.animate-bounce-in { animation: bounce-in 0.3s ease-out; }

:deep(#reader__dashboard_button_config_cancel),
:deep(#reader__dashboard_button_config_request_permission),
:deep(#reader__dashboard_button_stop),
:deep(#reader__dashboard_button_start) {
  background: #4f46e5 !important;
  color: white !important;
  border-radius: 12px !important;
  border: none !important;
  padding: 10px 20px !important;
  font-weight: 700 !important;
  cursor: pointer !important;
}

:deep(select) {
  padding: 8px !important;
  border-radius: 8px !important;
  border: 1px solid #e2e8f0 !important;
  outline: none !important;
}

.kiosk-toggle-row { margin-bottom: 20px; text-align: right; }
.kiosk-mode-btn { 
  background: #f8fafc; border: 1px solid #e2e8f0; color: #64748b;
  padding: 8px 16px; border-radius: 12px; font-size: 0.8rem; font-weight: 700;
  display: inline-flex; align-items: center; gap: 8px; cursor: pointer; transition: all 0.2s;
}
.kiosk-mode-btn:hover { background: #fff; border-color: #6366f1; color: #6366f1; }
.kiosk-mode-btn.active { background: #6366f1; color: #fff; border-color: #4f46e5; }

.kiosk-display-area { padding-bottom: 20px; }
.qr-kiosk-card {
  background: #fff; border-radius: 24px; padding: 24px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.05); text-align: center;
  border: 1px solid #f1f5f9;
}
.kiosk-header { display: flex; align-items: center; justify-content: center; gap: 10px; margin-bottom: 20px; }
.kiosk-header h3 { font-size: 1.1rem; font-weight: 900; color: #1e293b; margin: 0; letter-spacing: 0.5px; }

.qr-image-box { margin: 0 auto; max-width: 250px; }
.main-qr { width: 100%; height: auto; border: 4px solid #fff; border-radius: 16px; box-shadow: 0 10px 30px rgba(0,0,0,0.1); }
.qr-expiry-info { margin-top: 12px; font-size: 0.75rem; color: #94a3b8; font-weight: 600; display: flex; align-items: center; justify-content: center; gap: 4px; }
.qr-loading { padding: 40px 0; color: #64748b; }
.qr-empty-state { padding: 40px 0; color: #94a3b8; }

.scan-instructions { margin-top: 24px; padding: 16px; background: #f8fafc; border-radius: 16px; }

.recent-checkins-card { background: #fff; border-radius: 24px; padding: 24px; border: 1px solid #f1f5f9; box-shadow: 0 4px 20px rgba(0,0,0,0.05); }
.activity-item { 
  display: flex; align-items: center; gap: 12px; padding: 10px; 
  background: #f8fafc; border-radius: 12px; margin-bottom: 8px;
  border: 1px solid #f1f5f9;
}
.activity-avatar { width: 32px; height: 32px; background: #e0e7ff; color: #6366f1; border-radius: 10px; display: flex; align-items: center; justify-content: center; }
.activity-info { flex: 1; text-align: left; }
.activity-name { display: block; font-size: 0.85rem; font-weight: 800; color: #1e293b; }
.activity-time { font-size: 0.7rem; color: #64748b; font-weight: 500; }
.activity-status { color: #10b981; }

.animate-zoom-in { animation: zoomIn 0.3s ease-out; }
@keyframes zoomIn { from { transform: scale(0.95); opacity: 0; } to { transform: scale(1); opacity: 1; } }
.animate-slide-right { animation: slideRight 0.3s ease-out; }
@keyframes slideRight { from { transform: translateX(-10px); opacity: 0; } to { transform: translateX(0); opacity: 1; } }
.animate-spin-slow { animation: spin 3s linear infinite; }
</style>
