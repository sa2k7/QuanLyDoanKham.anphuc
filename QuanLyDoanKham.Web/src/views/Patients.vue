<template>
  <div class="space-y-8 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-8 mb-10">
      <div>
        <h2 class="text-4xl font-black tracking-tighter text-slate-800 flex items-center gap-4">
          <div class="w-14 h-14 bg-primary text-white rounded-[1.5rem] flex items-center justify-center shadow-2xl shadow-primary/20">
            <UsersIcon class="w-8 h-8" />
          </div>
          Hồ sơ Bệnh nhân
        </h2>
        <p class="text-slate-400 font-bold uppercase tracking-widest text-[10px] mt-4 ml-1">Lưu trữ & Truy xuất kết quả khám sức khỏe định kỳ</p>
      </div>
      <div v-if="selectedContractId" class="flex gap-4">
          <button @click="exportToExcel" 
                  :disabled="isLoading || patients.length === 0"
                  class="btn-premium bg-emerald-50 text-emerald-600 hover:bg-emerald-600 hover:text-white border-2 border-emerald-100 shadow-xl shadow-emerald-50 px-8 py-4 disabled:opacity-50">
            <Download class="w-6 h-6" />
            <span>XUẤT EXCEL</span>
          </button>
          <button v-if="authStore.role === 'Admin'" 
                  @click="openModal()" 
                  class="btn-premium bg-primary text-white hover:bg-slate-900 shadow-2xl shadow-primary/10 px-10 py-4">
            <UserPlus class="w-6 h-6" />
            <span>THÊM BỆNH NHÂN</span>
          </button>
      </div>
    </div>

    <!-- Contract Selection Card -->
    <div class="premium-card p-10 bg-white border-2 border-slate-50 mb-10 relative overflow-hidden group">
      <div class="absolute right-0 top-0 w-64 h-64 bg-slate-50 rounded-full -mr-32 -mt-32 transition-transform group-hover:scale-110"></div>
      <div class="flex flex-col md:flex-row items-center gap-10 relative z-10">
          <div class="flex-1 space-y-4 w-full">
              <label class="text-[10px] font-black uppercase text-slate-400 tracking-[0.2em] ml-1">Vui lòng chọn hợp đồng chỉ định</label>
              <div class="relative group/sel">
                <select 
                  v-model="selectedContractId" 
                  @change="fetchPatients"
                  class="w-full px-8 py-5 rounded-2xl bg-white border-2 border-slate-100 focus:border-primary/20 outline-none font-black text-slate-700 text-lg shadow-sm appearance-none cursor-pointer transition-all"
                >
                  <option :value="null">-------------------- LỰA CHỌN HỢP ĐỒNG --------------------</option>
                  <option v-for="contract in activeContracts" :key="contract.contractId ?? contract.healthContractId" :value="contract.contractId ?? contract.healthContractId">
                    {{ contract.companyName }} • #{{ contract.healthContractId ?? contract.contractId }}
                  </option>
                </select>
                <div class="absolute right-8 top-1/2 -translate-y-1/2 pointer-events-none text-slate-300 group-hover/sel:text-primary transition-colors">
                    <ChevronDown class="w-6 h-6" />
                </div>
              </div>
          </div>
          <div v-if="selectedContractId" class="hidden md:flex flex-col items-center bg-slate-900 text-white p-6 rounded-3xl shadow-xl shadow-slate-200">
              <p class="text-[9px] font-black text-slate-500 uppercase tracking-widest mb-2">QUY MÔ NHÂN SỰ</p>
              <p class="text-4xl font-black text-white leading-none tracking-tighter">{{ patients.length }}</p>
          </div>
      </div>
    </div>

    <!-- Patients List -->
    <div v-if="selectedContractId" class="premium-card bg-white border-2 border-slate-50 overflow-hidden mb-20 animate-slide-up">
        <div class="p-10 border-b border-slate-50 flex flex-col md:flex-row justify-between items-center gap-8 bg-slate-50/30">
            <h3 class="font-black text-slate-800 uppercase text-xs tracking-[0.2em] flex items-center gap-3">
                <div class="w-2 h-2 bg-primary rounded-full animate-pulse"></div>
                Danh sách nhân sự khám thực tế
            </h3>
            <div class="relative w-full md:w-96 group">
                <Search class="absolute left-6 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-300 group-focus-within:text-primary transition-colors" />
                <input placeholder="Tìm tên bệnh nhân, CMND..." 
                       class="w-full pl-16 pr-8 py-4 rounded-2xl bg-white border-2 border-slate-100 focus:border-primary/20 outline-none text-sm font-black text-slate-600 shadow-sm transition-all" />
            </div>
        </div>

      <!-- Patients Table -->
      <div class="overflow-x-auto">
        <table class="professional-table">
          <thead>
            <tr>
              <th class="w-20">#</th>
              <th>Họ tên & Thông tin cơ bản</th>
              <th>Ngày sinh</th>
              <th>Giới tính</th>
              <th>Định danh (CMND)</th>
              <th>Phòng ban / Tổ đội</th>
              <th class="text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="(patient, idx) in patients" :key="patient.patientId" class="hover:bg-slate-50/50 transition-all group">
              <td>
                  <span class="text-slate-300 font-black text-xs">{{ (idx + 1).toString().padStart(2, '0') }}</span>
              </td>
              <td>
                  <div class="flex items-center gap-4">
                      <div class="w-10 h-10 rounded-xl bg-slate-100 flex items-center justify-center font-black text-slate-400 group-hover:bg-primary group-hover:text-white transition-all text-xs">
                          {{ patient.fullName.charAt(0) }}
                      </div>
                      <span class="font-black text-slate-800 text-sm">{{ patient.fullName }}</span>
                  </div>
              </td>
              <td class="text-sm font-bold text-slate-600">{{ formatDate(patient.dateOfBirth) }}</td>
              <td>
                  <span class="px-3 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest"
                        :class="patient.gender === 'Nam' ? 'bg-indigo-50 text-indigo-600' : 'bg-rose-50 text-rose-600'">
                      {{ patient.gender }}
                  </span>
              </td>
              <td class="text-sm font-black text-slate-500 tracking-wider">{{ patient.idCardNumber }}</td>
              <td class="text-sm font-bold text-slate-700 italic">{{ patient.department }}</td>
              <td class="text-right">
                  <div class="flex justify-end gap-2">
                    <button 
                      @click="viewExamResults(patient)"
                      class="w-10 h-10 flex items-center justify-center rounded-xl bg-slate-50 text-slate-300 hover:bg-white hover:text-primary transition-all border border-transparent hover:border-slate-100 shadow-sm"
                      title="Xem kết quả"
                    >
                      <FileText class="w-5 h-5" />
                    </button>
                    <button 
                      v-if="authStore.role === 'Admin'"
                      @click="confirmDeletePatient(patient)"
                      class="w-10 h-10 flex items-center justify-center rounded-xl bg-slate-50 text-slate-300 hover:bg-white hover:text-rose-600 transition-all border border-transparent hover:border-slate-100 shadow-sm"
                      title="Xóa bệnh nhân"
                    >
                      <Trash2 class="w-5 h-5" />
                    </button>
                  </div>
              </td>
            </tr>
            <tr v-if="patients.length === 0">
              <td colspan="7" class="py-32 text-center text-slate-200 font-black uppercase tracking-[0.2em] text-xs">Chưa có dữ liệu nhân sự cho hợp đồng này</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add Patient Modal -->
    <div v-if="showModal" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div class="p-6 border-b border-slate-200">
          <h3 class="text-2xl font-black text-slate-900">Thêm Bệnh nhân</h3>
        </div>
        
        <div class="p-6 space-y-4">
          <div>
            <label class="block text-sm font-bold text-slate-700 mb-2">Họ tên *</label>
            <input v-model="currentPatient.fullName" type="text" class="w-full px-4 py-3 rounded-xl border-2 border-slate-200 focus:border-primary focus:outline-none" required />
          </div>
          
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-bold text-slate-700 mb-2">Ngày sinh *</label>
              <input v-model="currentPatient.dateOfBirth" type="date" class="w-full px-4 py-3 rounded-xl border-2 border-slate-200 focus:border-primary focus:outline-none" required />
            </div>
            <div>
              <label class="block text-sm font-bold text-slate-700 mb-2">Giới tính *</label>
              <select v-model="currentPatient.gender" class="w-full px-4 py-3 rounded-xl border-2 border-slate-200 focus:border-primary focus:outline-none">
                <option value="Nam">Nam</option>
                <option value="Nữ">Nữ</option>
              </select>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-bold text-slate-700 mb-2">CMND/CCCD</label>
              <input v-model="currentPatient.idCardNumber" type="text" class="w-full px-4 py-3 rounded-xl border-2 border-slate-200 focus:border-primary focus:outline-none" />
            </div>
            <div>
              <label class="block text-sm font-bold text-slate-700 mb-2">Số điện thoại</label>
              <input v-model="currentPatient.phoneNumber" type="text" class="w-full px-4 py-3 rounded-xl border-2 border-slate-200 focus:border-primary focus:outline-none" />
            </div>
          </div>

          <div>
            <label class="block text-sm font-bold text-slate-700 mb-2">Phòng ban</label>
            <input v-model="currentPatient.department" type="text" class="w-full px-4 py-3 rounded-xl border-2 border-slate-200 focus:border-primary focus:outline-none" />
          </div>
        </div>

        <div class="p-6 border-t border-slate-200 flex justify-end space-x-3">
          <button @click="showModal = false" class="px-6 py-3 rounded-xl font-bold text-slate-700 hover:bg-slate-100">Hủy</button>
          <button @click="savePatient" :disabled="isLoading" class="px-6 py-3 rounded-xl font-bold bg-primary hover:bg-primary-dark text-white disabled:opacity-50">
            {{ isLoading ? 'Đang lưu...' : 'Lưu' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Exam Results Modal -->
    <div v-if="showExamModal" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-2xl shadow-2xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div class="p-6 border-b border-slate-200">
          <h3 class="text-2xl font-black text-slate-900">Kết quả khám: {{ selectedPatient?.fullName }}</h3>
        </div>
        
        <div class="p-6">
          <button 
            v-if="authStore.role === 'Admin' || authStore.role === 'MedicalStaff'"
            @click="showAddExamForm = !showAddExamForm"
            class="mb-4 flex items-center space-x-2 bg-primary hover:bg-primary-dark text-white px-4 py-2 rounded-xl font-bold"
          >
            <Plus class="w-5 h-5" />
            <span>Thêm kết quả khám</span>
          </button>

          <!-- Add Exam Form -->
          <div v-if="showAddExamForm" class="bg-slate-50 rounded-xl p-4 mb-6 space-y-3">
            <div>
              <label class="block text-sm font-bold text-slate-700 mb-2">Loại khám</label>
              <input v-model="newExam.examType" type="text" class="w-full px-4 py-2 rounded-xl border-2 border-slate-200 focus:border-primary focus:outline-none" placeholder="Ví dụ: Nội khoa" />
            </div>
            <div>
              <label class="block text-sm font-bold text-slate-700 mb-2">Kết quả</label>
              <textarea v-model="newExam.result" class="w-full px-4 py-2 rounded-xl border-2 border-slate-200 focus:border-primary focus:outline-none" rows="3"></textarea>
            </div>
            <div>
              <label class="block text-sm font-bold text-slate-700 mb-2">Chẩn đoán</label>
              <input v-model="newExam.diagnosis" type="text" class="w-full px-4 py-2 rounded-xl border-2 border-slate-200 focus:border-primary focus:outline-none" />
            </div>
            <button @click="saveExamResult" :disabled="isLoading" class="bg-primary hover:bg-primary-dark text-white px-6 py-2 rounded-xl font-bold disabled:opacity-50">
              {{ isLoading ? 'Đang lưu...' : 'Lưu kết quả' }}
            </button>
          </div>

          <!-- Exam Results List -->
          <div class="space-y-3">
            <div v-for="exam in examResults" :key="exam.examResultId" class="border-2 border-slate-200 rounded-xl p-4">
              <div class="flex justify-between items-start mb-2">
                <h4 class="font-black text-slate-900">{{ exam.examType }}</h4>
                <span class="text-sm text-slate-500">{{ formatDate(exam.examDate) }}</span>
              </div>
              <p class="text-slate-700 mb-2"><strong>Kết quả:</strong> {{ exam.result }}</p>
              <p class="text-slate-700"><strong>Chẩn đoán:</strong> {{ exam.diagnosis }}</p>
              <p v-if="exam.doctorName" class="text-sm text-slate-500 mt-2">Bác sĩ: {{ exam.doctorName }}</p>
            </div>
            <div v-if="examResults.length === 0" class="text-center py-8 text-slate-300 font-bold italic">Chưa có kết quả khám nào</div>
          </div>
        </div>

        <div class="p-6 border-t border-slate-200 flex justify-end">
          <button @click="closeExamModal" class="px-6 py-3 rounded-xl font-bold text-slate-700 hover:bg-slate-100">Đóng</button>
        </div>
      </div>
    </div>

    <!-- Confirm Delete Dialog -->
    <ConfirmDialog
      v-model="showDeleteConfirm"
      title="Xóa bệnh nhân?"
      :message="`Bạn có chắc muốn xóa &quot;${patientToDelete?.fullName}&quot;? Hành động này không thể hoàn tác.`"
      confirmText="Xóa ngay"
      variant="danger"
      @confirm="deletePatient"
    />
  </div>
</template>

<script setup>
// BUG FIX: Thêm đầy đủ các import bị thiếu (ref, onMounted, axios)
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { UserPlus, FileText, Trash2, Plus, Users as UsersIcon, Search, ChevronDown, Download } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'

const authStore = useAuthStore()
const toast = useToast()

const contracts = ref([])
const selectedContractId = ref(null)
const patients = ref([])
const showModal = ref(false)
const showExamModal = ref(false)
const showAddExamForm = ref(false)
const selectedPatient = ref(null)
const examResults = ref([])
const isLoading = ref(false)

// For delete confirmation
const showDeleteConfirm = ref(false)
const patientToDelete = ref(null)

const currentPatient = ref({
  fullName: '',
  dateOfBirth: '',
  gender: 'Nam',
  idCardNumber: '',
  phoneNumber: '',
  department: ''
})

const activeContracts = computed(() => {
  return contracts.value.filter(c => !c.isFinished)
})

const newExam = ref({
  examType: '',
  result: '',
  diagnosis: ''
})

onMounted(async () => {
  await fetchContracts()
})

const fetchContracts = async () => {
  try {
    const res = await axios.get('http://localhost:5283/api/HealthContracts')
    contracts.value = res.data
  } catch (e) {
    console.error(e)
    toast.error('Không thể tải danh sách hợp đồng')
  }
}

const fetchPatients = async () => {
  if (!selectedContractId.value) return
  try {
    const res = await axios.get(`http://localhost:5283/api/Patients/by-contract/${selectedContractId.value}`)
    patients.value = res.data
  } catch (e) {
    console.error(e)
    toast.error('Không thể tải danh sách bệnh nhân')
  }
}

const openModal = () => {
  currentPatient.value = {
    fullName: '',
    dateOfBirth: '',
    gender: 'Nam',
    idCardNumber: '',
    phoneNumber: '',
    department: ''
  }
  showModal.value = true
}

const savePatient = async () => {
  if (!currentPatient.value.fullName) return toast.warning('Vui lòng nhập họ tên bệnh nhân!')
  try {
    isLoading.value = true
    await axios.post('http://localhost:5283/api/Patients', {
      ...currentPatient.value,
      healthContractId: selectedContractId.value
    })
    toast.success('Đã thêm bệnh nhân thành công!')
    showModal.value = false
    fetchPatients()
  } catch (e) {
    console.error(e)
    toast.error('Lỗi khi thêm bệnh nhân')
  } finally {
    isLoading.value = false
  }
}

const viewExamResults = async (patient) => {
  selectedPatient.value = patient
  showExamModal.value = true
  showAddExamForm.value = false
  try {
    const res = await axios.get(`http://localhost:5283/api/Patients/${patient.patientId}/exam-results`)
    examResults.value = res.data
  } catch (e) {
    console.error(e)
    toast.error('Không thể tải kết quả khám')
  }
}

const saveExamResult = async () => {
  try {
    isLoading.value = true
    await axios.post(`http://localhost:5283/api/Patients/${selectedPatient.value.patientId}/exam-result`, newExam.value)
    toast.success('Đã lưu kết quả khám!')
    showAddExamForm.value = false
    newExam.value = { examType: '', result: '', diagnosis: '' }
    viewExamResults(selectedPatient.value)
  } catch (e) {
    console.error(e)
    toast.error('Lỗi khi lưu kết quả khám')
  } finally {
    isLoading.value = false
  }
}

const closeExamModal = () => {
  showExamModal.value = false
  selectedPatient.value = null
  examResults.value = []
}

const confirmDeletePatient = (patient) => {
  patientToDelete.value = patient
  showDeleteConfirm.value = true
}

const deletePatient = async () => {
  try {
    isLoading.value = true
    await axios.delete(`http://localhost:5283/api/Patients/${patientToDelete.value.patientId}`)
    toast.success(`Đã xóa bệnh nhân "${patientToDelete.value.fullName}"`)
    patientToDelete.value = null
    fetchPatients()
  } catch (e) {
    console.error(e)
    toast.error('Lỗi khi xóa bệnh nhân')
  } finally {
    isLoading.value = false
  }
}

const exportToExcel = async () => {
  if (!selectedContractId.value) return toast.warning('Vui lòng chọn hợp đồng để xuất danh sách!')
  try {
    isLoading.value = true
    const response = await axios.get(`http://localhost:5283/api/Patients/export/${selectedContractId.value}`, {
      responseType: 'blob'
    })
    
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    
    // Fallback filename
    let fileName = 'DanhSachBenhNhan.xlsx'
    const contentDisposition = response.headers['content-disposition']
    if (contentDisposition) {
      const fileNameMatch = contentDisposition.match(/filename="?([^"]+)"?/)
      if (fileNameMatch) fileName = fileNameMatch[1]
    }
    
    link.setAttribute('download', fileName)
    document.body.appendChild(link)
    link.click()
    
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    toast.success('Đã xuất file Excel thành công!')
  } catch (e) {
    console.error(e)
    toast.error('Lỗi khi xuất file Excel')
  } finally {
    isLoading.value = false
  }
}

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleDateString('vi-VN')
}
</script>
