<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-emerald-600 text-white rounded-2xl flex items-center justify-center shadow-lg">
            <Wallet class="w-6 h-6" />
          </div>
          Bảng lương & Thù lao
        </h2>
        <p class="text-slate-400 font-black uppercase tracking-widest text-[9px] mt-2">Nội bộ: Tổng hợp lương cố định & Thù lao đi đoàn</p>
      </div>
      
      <div class="flex items-center gap-3 bg-white p-2 rounded-2xl shadow-sm border border-slate-100">
        <select v-model="selectedMonth" class="bg-slate-50 px-4 py-2 rounded-xl font-black text-xs outline-none">
            <option v-for="m in 12" :key="m" :value="m">Tháng {{ m }}</option>
        </select>
        <input type="number" v-model="selectedYear" class="w-20 bg-slate-50 px-4 py-2 rounded-xl font-black text-xs outline-none" />
        <button @click="fetchPayroll" class="bg-slate-900 text-white p-2 rounded-xl"><RefreshCcw class="w-4 h-4" :class="loading ? 'animate-spin' : ''" /></button>
      </div>
    </div>

    <!-- Stats Summary -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
        <div class="premium-card p-8 bg-white border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] rounded-[2rem] transition-all hover:-translate-y-1">
            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Tổng quỹ lương tháng</p>
            <p class="text-3xl font-black text-slate-800 ">{{ formatPrice(totalPayroll) }}</p>
            <div class="mt-4 h-1 w-12 bg-emerald-500 rounded-full"></div>
        </div>
        <div class="premium-card p-8 bg-white border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] rounded-[2rem] transition-all hover:-translate-y-1">
            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Nhân sự hưởng lương</p>
            <p class="text-3xl font-black text-slate-800 ">{{ payrollList.length }} <span class="text-xs text-slate-400 uppercase">Thành viên</span></p>
            <div class="mt-4 h-1 w-12 bg-indigo-500 rounded-full"></div>
        </div>
        <button @click="exportExcel" class="p-8 bg-slate-900 text-white rounded-[2rem] shadow-[4px_4px_0px_#1e293b] border-2 border-slate-900 hover:bg-slate-800 transition-all flex flex-col items-center justify-center gap-2 group">
            <Download class="w-6 h-6 text-emerald-400 group-hover:scale-110 transition-transform" />
            <span class="font-black text-[11px] uppercase tracking-[0.2em]">XUẤT BẢNG LƯƠNG (EXCEL)</span>
        </button>
    </div>

    <!-- Payroll List -->
    <div class="premium-card bg-white rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] overflow-hidden">
        <div class="p-6 border-b border-slate-50 flex items-center gap-4 bg-slate-50/30">
            <div class="relative flex-1">
                <Search class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 w-4 h-4" />
                <input v-model="searchQuery" placeholder="Tìm tên nhân viên..." class="w-full pl-10 pr-4 py-2 rounded-xl bg-white border border-slate-200 text-xs font-black outline-none" />
            </div>
        </div>

        <div class="overflow-x-auto">
            <table class="w-full text-left">
                <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                    <tr>
                        <th class="p-4">Nhân viên</th>
                        <th class="p-4">Chức danh</th>
                        <th class="p-4 text-right">Lương Định mức</th>
                        <th class="p-4 text-center">Số ngày công</th>
                        <th class="p-4 text-right">Thù lao đoàn</th>
                        <th class="p-4 text-right">Thực lãnh</th>
                        <th class="p-4 text-center">Tác vụ</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="item in filteredList" :key="item.staffId" class="text-xs hover:bg-slate-50/50 transition-all">
                        <td class="p-4">
                            <div class="font-black text-slate-800">{{ item.fullName }}</div>
                            <div class="text-[9px] text-slate-400">{{ item.employeeCode }}</div>
                        </td>
                        <td class="p-4 font-black text-slate-500">{{ item.jobTitle }}</td>
                        <td class="p-4 text-right font-black">{{ formatPrice(item.baseSalary) }}</td>
                        <td class="p-4 text-center">
                            <span class="px-3 py-1 bg-indigo-50 text-indigo-600 rounded-lg font-black">{{ item.totalDays }}</span>
                        </td>
                        <td class="p-4 text-right font-black text-emerald-600">+ {{ formatPrice(item.groupEarnings) }}</td>
                        <td class="p-4 text-right">
                            <span class="px-4 py-2 bg-slate-900 text-white rounded-xl font-black">{{ formatPrice(item.totalSalary) }}</span>
                        </td>
                        <td class="p-4 text-center">
                            <button @click="showDetails(item)" class="text-indigo-600 hover:underline font-black uppercase tracking-widest text-[10px]">Cơ cấu</button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Detail Modal -->
    <Teleport to="body">
      <div v-if="detailItem" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-900/80 backdrop-blur-md">
          <div class="bg-white w-full max-w-2xl rounded-[2.5rem] border-2 border-slate-900 shadow-2xl overflow-hidden flex flex-col animate-fade-in-up relative mt-auto mb-auto">
              <!-- Border Overlay -->
              <div class="absolute inset-0 rounded-[inherit] border-2 border-slate-900 pointer-events-none z-50"></div>
              
              <!-- Header Accent Line -->
              <div class="absolute top-0 left-0 right-0 h-4 bg-gradient-to-r from-emerald-400 to-emerald-600 z-0"></div>

              <div class="p-10 pb-6 relative z-10 pt-12 flex justify-between items-center">
                  <div class="flex items-center gap-4">
                      <div class="w-14 h-14 bg-emerald-50 text-emerald-600 rounded-3xl flex items-center justify-center shadow-inner border border-emerald-100">
                          <Wallet class="w-7 h-7" />
                      </div>
                      <div>
                          <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest ">Cơ cấu lương</h3>
                          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">{{ detailItem.fullName }} — {{ detailItem.employeeCode }}</p>
                      </div>
                  </div>
                  <button @click="detailItem = null" class="bg-slate-100 p-2 rounded-full hover:bg-slate-200 transition-all text-slate-500">
                      <X class="w-5 h-5" />
                  </button>
              </div>
              
              <div class="p-8 space-y-6 overflow-y-auto max-h-[60vh]">
                  <div class="bg-slate-50 p-6 rounded-2xl grid grid-cols-2 gap-4">
                      <div>
                          <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Lương cố định</p>
                          <p class="text-lg font-black text-slate-700">{{ formatPrice(detailItem.baseSalary) }}</p>
                      </div>
                      <div>
                          <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Số ngày đi đoàn</p>
                          <p class="text-lg font-black text-indigo-600">{{ detailItem.totalDays }} Ngày</p>
                      </div>
                  </div>

                  <div class="space-y-4">
                      <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Chi tiết các đoàn đã tham gia</p>
                      <div class="space-y-2">
                          <div v-for="d in detailItem.details" :key="d.groupId" class="flex justify-between items-center p-4 bg-white border-2 border-slate-900 shadow-[2px_2px_0px_#0f172a] rounded-xl">
                              <div>
                                  <p class="font-black text-slate-700 text-sm">{{ d.groupName }}</p>
                                  <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">{{ formatDate(d.examDate) }} • Hệ số: {{ d.shiftType }}</p>
                              </div>
                              <p class="font-black text-emerald-600">{{ formatPrice(d.calculatedSalary) }}</p>
                          </div>
                          <p v-if="!detailItem.details.length" class="text-center py-4 text-slate-300 italic text-xs">Không tham gia đoàn nào trong tháng</p>
                      </div>
                  </div>
              </div>

              <div class="p-8 bg-slate-900 text-white flex justify-between items-center">
                  <div>
                      <p class="text-[9px] font-black text-slate-500 uppercase tracking-widest">Tổng cộng Thực lãnh</p>
                      <p class="text-3xl font-black text-emerald-400 ">{{ formatPrice(detailItem.totalSalary) }}</p>
                  </div>
                  <button @click="detailItem = null" class="px-8 py-3 bg-slate-800 text-white rounded-xl font-black text-xs uppercase tracking-widest">ĐÓNG</button>
              </div>
          </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import axios from 'axios'
import { Wallet, RefreshCcw, Download, Search, X, Layers, FileText } from 'lucide-vue-next'
import { useToast } from '../composables/useToast'

const toast = useToast()
const loading = ref(false)
const selectedMonth = ref(new Date().getMonth() + 1)
const selectedYear = ref(new Date().getFullYear())
const payrollList = ref([])
const searchQuery = ref('')
const detailItem = ref(null)

const totalPayroll = computed(() => payrollList.value.reduce((sum, item) => sum + item.totalSalary, 0))

const filteredList = computed(() => {
    let list = payrollList.value
    if (searchQuery.value) {
        const q = searchQuery.value.toLowerCase()
        list = list.filter(p => p.fullName.toLowerCase().includes(q) || p.employeeCode?.toLowerCase().includes(q))
    }
    return list
})

const fetchPayroll = async () => {
    loading.value = true
    try {
        const res = await axios.get(`http://localhost:5283/api/Payroll/monthly`, {
            params: { month: selectedMonth.value, year: selectedYear.value }
        })
        payrollList.value = res.data
    } catch (e) { toast.error("Lỗi dữ liệu bảng lương") }
    finally { loading.value = false }
}

const exportExcel = async () => {
    try {
        const res = await axios.get(`http://localhost:5283/api/Payroll/export-monthly`, {
            params: { month: selectedMonth.value, year: selectedYear.value },
            responseType: 'blob'
        })
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `BangLuong_${selectedMonth.value}_${selectedYear.value}.xlsx`)
        document.body.appendChild(link)
        link.click()
        toast.success("Đã xuất file bảng lương!")
    } catch (e) { toast.error("Lỗi xuất file") }
}

const showDetails = (item) => { detailItem.value = item }
const formatDate = (d) => new Date(d).toLocaleDateString('vi-VN')
const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)

onMounted(fetchPayroll)
watch([selectedMonth, selectedYear], fetchPayroll)
</script>
