<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in pb-12 p-6 scrollbar-premium overflow-y-auto font-sans">
    <div class="max-w-7xl mx-auto w-full">
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 mb-8">
        <div>
          <h2 class="text-3xl font-bold text-slate-800 flex items-center gap-3">
            <div class="w-12 h-12 bg-primary text-white rounded-2xl flex items-center justify-center shadow-lg">
              <Calculator class="w-6 h-6" />
            </div>
            Đối Soát & Quyết Toán
          </h2>
          <p class="text-slate-400 font-semibold uppercase tracking-widest text-[10px] mt-2">Phân tích chênh lệch giá trị dự kiến và thực tế (P&L Audit)</p>
        </div>
        <div class="flex gap-4">
          <button @click="loadContracts" class="w-14 h-14 bg-white/80 backdrop-blur-md border border-white/40 rounded-[1.25rem] text-slate-600 hover:text-primary hover:scale-105 transition-all shadow-sm flex items-center justify-center group">
            <RefreshCw class="w-6 h-6" :class="{ 'animate-spin': loading }" />
          </button>
        </div>
      </div>

      <!-- Search & Filter -->
      <div class="premium-card bg-white rounded-[2rem] shadow-sm mb-8 overflow-hidden">
        <div class="p-6 border-b border-slate-100 flex items-center gap-4 bg-white/50">
            <div class="relative group flex-1">
                <Search class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 w-4 h-4" />
                <input v-model="searchQuery" placeholder="Tìm tên hợp đồng, công ty..."
                       class="w-full pl-10 pr-4 py-2.5 rounded-xl bg-white border border-slate-200 focus:border-primary/30 focus:ring-2 focus:ring-primary/10 outline-none font-semibold text-sm text-slate-600 shadow-sm transition-all" />
            </div>
            <div class="px-4 py-2 bg-white rounded-xl border border-slate-100 text-[10px] font-semibold text-slate-400 uppercase tracking-widest whitespace-nowrap">
                Kết quả: <span class="text-primary font-bold">{{ filteredContracts.length }}</span>
            </div>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-left">
            <thead class="bg-slate-50 text-[10px] font-semibold uppercase tracking-widest text-slate-500 border-b border-slate-100">
              <tr>
                <th class="p-4 text-center w-16">STT</th>
                <th class="p-4">Thông tin hợp đồng</th>
                <th class="p-4">Doanh nghiệp</th>
                <th class="p-4 text-right">Giá trị HĐ</th>
                <th class="p-4 text-center">Dự kiến</th>
                <th class="p-4 text-center">Trạng thái</th>
                <th class="p-4 text-center">Tác vụ</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-50">
              <tr v-for="(c, index) in filteredContracts" :key="c.healthContractId"
                @click="viewSettlement(c)"
                class="text-sm hover:bg-sky-50/50 transition-all duration-200 group cursor-pointer border-b border-slate-50 last:border-0">
                <td class="p-4 text-center font-semibold text-slate-400 tabular-nums">
                  {{ String(index + 1).padStart(3, '0') }}
                </td>
                <td class="p-4">
                  <div class="flex items-center gap-3">
                    <div class="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center shadow-sm group-hover:scale-110 transition-transform duration-300">
                      <FileText class="w-5 h-5" />
                    </div>
                    <div>
                      <div class="font-bold text-slate-800 uppercase italic leading-tight group-hover:text-primary transition-colors">{{ c.contractName }}</div>
                      <div class="text-[9px] text-slate-400 font-bold uppercase tracking-wider mt-1">{{ c.contractCode || 'HĐ-' + c.healthContractId }}</div>
                    </div>
                  </div>
                </td>
                <td class="p-4">
                  <div class="flex items-center gap-2 font-semibold text-slate-600">
                    <Building2 class="w-4 h-4 text-slate-300" />
                    {{ c.companyName }}
                  </div>
                </td>
                <td class="p-4 text-right font-black text-slate-700 tabular-nums">
                  {{ formatCurrency(c.totalValue) }}
                </td>
                <td class="p-4 text-center">
                  <span class="px-3 py-1 bg-slate-100 rounded-lg font-bold text-slate-600 text-xs shadow-sm">
                    {{ c.expectedQuantity }} người
                  </span>
                </td>
                <td class="p-4 text-center">
                  <span :class="getStatusBadgeClass(c.status)" class="px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-widest shadow-sm">
                    {{ c.status === 'Completed' || c.status === 'Active' ? 'HOÀN TẤT' : 'ĐANG XỬ LÝ' }}
                  </span>
                </td>
                <td class="p-4 text-center">
                  <button @click.stop="viewSettlement(c)" 
                    class="btn-action-premium variant-indigo scale-90"
                    title="Phân tích quyết toán">
                    <Activity class="w-5 h-5" />
                  </button>
                </td>
              </tr>
              <tr v-if="filteredContracts.length === 0">
                <td colspan="7" class="py-20 text-center">
                  <div class="flex flex-col items-center justify-center gap-4">
                    <Calculator class="w-12 h-12 text-slate-200" />
                    <p class="text-slate-400 font-semibold uppercase tracking-widest text-xs">Không tìm thấy hợp đồng phù hợp</p>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Settlement Detail Modal -->
      <Teleport to="body">
        <div v-if="selectedSettlement" class="modal-overlay flex items-center justify-center p-4" @click.self="selectedSettlement = null">
          <div class="modal-box w-full max-w-2xl overflow-hidden animate-scale-up !rounded-[2.5rem] flex flex-col max-h-[90vh]">
            <div class="bg-gradient-to-br from-slate-800 to-slate-900 p-12 text-white relative overflow-hidden shrink-0">
              <div class="absolute -right-10 -top-10 w-64 h-64 bg-primary/20 rounded-full blur-[80px]"></div>
              <button @click="selectedSettlement = null" class="absolute top-8 right-8 w-10 h-10 rounded-full bg-white/10 hover:bg-white/20 flex items-center justify-center transition-all group z-10">
                <X class="w-5 h-5 text-white/70 group-hover:text-white group-hover:rotate-90 transition-all" />
              </button>
              
              <div class="relative z-10">
                <div class="flex items-center gap-4 mb-4">
                  <div class="w-14 h-14 bg-primary rounded-2xl flex items-center justify-center shadow-lg shadow-primary/20">
                    <Calculator class="w-7 h-7" />
                  </div>
                  <div>
                    <h2 class="text-4xl font-black tracking-tight italic uppercase">QUYẾT TOÁN</h2>
                    <p class="text-[10px] font-bold opacity-60 uppercase tracking-[0.2em] mt-1">Audit Report Analysis</p>
                  </div>
                </div>
                <div class="h-px w-24 bg-gradient-to-r from-primary to-transparent mb-4"></div>
                <p class="text-lg font-black text-white/90 uppercase tracking-widest leading-tight max-w-md">{{ selectedSettlement.contractName }}</p>
              </div>
            </div>
            
            <div class="p-10 overflow-y-auto custom-scrollbar flex-1">
             <!-- Revenue & Gross Profit -->
             <div class="grid grid-cols-2 gap-6 mb-8">
                <!-- Net Revenue -->
                <div class="bg-slate-900 p-8 rounded-[2rem] shadow-xl relative overflow-hidden group premium-card border-none">
                  <div class="absolute -right-4 -bottom-4 opacity-10 text-primary transform group-hover:scale-110 transition-transform">
                    <Calculator class="w-32 h-32" />
                  </div>
                  <p class="text-[10px] font-black text-slate-300 uppercase tracking-widest mb-2">Doanh Thu Thuần (Net Revenue)</p>
                  <h4 class="text-3xl font-black text-white tabular-nums">{{ formatCurrency(selectedSettlement.netRevenue) }}</h4>
                  <div class="flex items-center gap-2 mt-2">
                    <span class="text-[10px] text-primary font-bold uppercase tracking-wide">{{ selectedSettlement.completedRecords }} ca COMPLETED</span>
                  </div>
                </div>
                
                <!-- Gross Profit -->
                <div :class="[selectedSettlement.profitMargin < 20 ? 'bg-rose-50 border-rose-100' : 'bg-white/80 border-white/50 backdrop-blur-md shadow-glass']" class="p-8 rounded-[2rem] border relative overflow-hidden group">
                  <div class="absolute -right-4 -bottom-4 opacity-5 transform group-hover:scale-110 transition-transform">
                    <Activity class="w-32 h-32 text-emerald-500" />
                  </div>
                  <p :class="selectedSettlement.profitMargin < 20 ? 'text-rose-400' : 'text-emerald-500'" class="text-[10px] font-black uppercase tracking-widest mb-2">
                    Lợi Nhuận Gộp
                  </p>
                  <h4 :class="selectedSettlement.profitMargin < 20 ? 'text-rose-700' : 'text-emerald-700'" class="text-3xl font-black tabular-nums">
                    {{ formatCurrency(selectedSettlement.grossProfit) }}
                  </h4>
                  <div class="flex items-center gap-2 mt-2">
                    <span :class="selectedSettlement.profitMargin < 20 ? 'bg-rose-200 text-rose-700' : 'bg-emerald-100 text-emerald-700'" class="px-3 py-1 rounded-xl text-xs font-black shadow-sm">
                       {{ selectedSettlement.profitMargin?.toFixed(1) || 0 }}%
                    </span>
                    <span :class="selectedSettlement.profitMargin < 20 ? 'text-rose-600' : 'text-emerald-600'" class="text-[10px] font-black uppercase tracking-widest opacity-60">
                       Margin %
                    </span>
                  </div>
                </div>
             </div>

             <!-- Breakdown Costs -->
             <div class="mb-8">
                <h3 class="text-sm font-black text-slate-800 uppercase tracking-widest mb-4">Cấu trúc Chi Phí (Cost Breakdown)</h3>
                
                <div class="space-y-4 bg-white/50 p-6 rounded-3xl border border-slate-100 shadow-sm">
                     <div class="flex justify-between items-center text-sm">
                       <div class="flex items-center gap-3">
                          <div class="w-10 h-10 rounded-xl bg-blue-50 text-blue-500 flex items-center justify-center shadow-sm"><Users class="w-5 h-5" /></div>
                          <span class="font-bold text-slate-600 italic">Lương nhân sự (Labor)</span>
                       </div>
                       <span class="font-black text-slate-800">{{ formatCurrency(selectedSettlement.laborCost) }}</span>
                    </div>

                    <div class="flex justify-between items-center text-sm">
                       <div class="flex items-center gap-3">
                          <div class="w-10 h-10 rounded-xl bg-amber-50 text-amber-500 flex items-center justify-center shadow-sm"><Package class="w-5 h-5" /></div>
                          <span class="font-bold text-slate-600 italic">Độ lệch vật tư (Material Out-In)</span>
                       </div>
                       <span class="font-black text-slate-800">{{ formatCurrency(selectedSettlement.materialCost) }}</span>
                    </div>

                    <div class="flex justify-between items-center text-sm">
                       <div class="flex items-center gap-3">
                          <div class="w-10 h-10 rounded-xl bg-purple-50 text-purple-500 flex items-center justify-center shadow-sm"><Clock class="w-5 h-5" /></div>
                          <span class="font-bold text-slate-600 italic">Chi phí chung (Overhead)</span>
                       </div>
                       <span class="font-black text-slate-800">{{ formatCurrency(selectedSettlement.overheadCost) }}</span>
                    </div>

                    <div class="my-4 border-t border-slate-100"></div>

                    <div class="flex justify-between items-center px-2">
                       <span class="font-black text-slate-800 text-sm uppercase tracking-widest">Tổng Chi Phí (Total Cost)</span>
                       <span class="font-black text-rose-500 text-xl">{{ formatCurrency(selectedSettlement.totalCost) }}</span>
                    </div>

                    <!-- Dịch vụ ngoài gói -->
                    <div class="my-4 border-t border-slate-100"></div>
                    <div class="p-4 bg-orange-50 rounded-2xl border border-orange-100 mt-4">
                       <div class="flex justify-between items-center mb-2">
                           <span class="font-black text-orange-800 text-sm uppercase tracking-widest">Phát sinh ngoài gói</span>
                           <button v-if="!isEditingExtra && authStore.hasPermission('QuyetToan.Edit')" @click="startEditingExtra" class="text-xs font-bold text-orange-600 hover:text-orange-700 bg-orange-100 px-3 py-1 rounded-full uppercase">Sửa đổi</button>
                       </div>
                       
                       <div v-if="!isEditingExtra">
                           <div class="flex justify-between items-center mb-3">
                               <span class="text-sm text-orange-700 font-medium italic">Doanh thu ngoài gói</span>
                               <span class="font-black text-orange-600 text-lg">{{ formatCurrency(selectedSettlement.extraServiceRevenue) }}</span>
                           </div>
                           <div class="flex justify-between items-center mb-3">
                               <span class="text-sm text-orange-700 font-medium italic">VAT Rate: {{ selectedSettlement.vatRate }}% ( {{ formatCurrency(selectedSettlement.vatAmount) }} )</span>
                           </div>
                           <ul class="text-xs text-orange-800 space-y-1" v-if="parsedExtraItems.length > 0">
                               <li v-for="(item, idx) in parsedExtraItems" :key="idx" class="flex justify-between border-b border-orange-200/50 pb-1">
                                  <span>{{ item.name }}</span>
                                  <span class="font-bold">{{ formatCurrency(item.amount) }}</span>
                               </li>
                           </ul>
                           <p v-else class="text-xs text-orange-600 italic">Không có chi tiết</p>
                       </div>
                       
                       <div v-else class="flex flex-col gap-2 mt-2">
                           <div class="flex gap-2 items-center mb-2">
                               <label class="text-xs font-bold text-orange-800">Thuế GTGT (VAT %):</label>
                               <input type="number" v-model.number="tempVatRate" class="w-20 bg-white border border-orange-200 rounded-xl px-2 py-1 text-slate-800 font-bold focus:outline-none focus:border-orange-400" />
                           </div>
                           
                           <div v-for="(item, idx) in tempExtraItems" :key="idx" class="flex gap-2 items-center">
                               <input type="text" v-model="item.name" class="flex-1 bg-white border border-orange-200 rounded-xl px-3 py-2 text-slate-800 text-sm focus:outline-none focus:border-orange-400" placeholder="Tên dịch vụ..." />
                               <input type="number" v-model.number="item.amount" class="w-32 bg-white border border-orange-200 rounded-xl px-3 py-2 text-slate-800 font-bold text-sm focus:outline-none focus:border-orange-400" placeholder="Số tiền..." />
                               <button @click="removeExtraItem(idx)" class="text-rose-500 hover:text-rose-700 bg-white border border-rose-200 p-2 rounded-xl transition">
                                  <Trash2 class="w-4 h-4" />
                               </button>
                           </div>
                           
                           <div class="flex gap-2 items-center mt-2">
                               <button @click="addExtraItem" class="bg-orange-100 text-orange-700 font-bold px-3 py-2 rounded-xl hover:bg-orange-200 transition text-sm flex-1 border border-orange-200 border-dashed">
                                   + Thêm Mục Mới
                               </button>
                               <button @click="saveExtraService" :disabled="savingExtra" class="bg-orange-600 text-white font-bold px-4 py-2 rounded-xl hover:bg-orange-700 transition">
                                   {{ savingExtra ? 'Đang lưu...' : 'Lưu' }}
                               </button>
                               <button @click="isEditingExtra = false" class="bg-white border border-slate-200 text-slate-500 font-bold px-4 py-2 rounded-xl hover:bg-slate-50 transition">Hủy</button>
                           </div>
                       </div>
                    </div>
                </div>
             </div>

             <!-- Alerts -->
             <div v-if="selectedSettlement.profitMargin < 20" class="p-6 mb-8 bg-rose-50 border border-rose-100 rounded-2xl flex gap-5 items-start">
                <div class="w-12 h-12 shrink-0 bg-rose-200/50 text-rose-600 rounded-2xl flex items-center justify-center shadow-inner"><AlertTriangle class="w-6 h-6 animate-bounce-slow" /></div>
                <div>
                   <p class="font-black text-rose-800 text-sm uppercase tracking-wider mb-1">Cảnh báo Biên lợi nhuận (Margin &lt; 20%)</p>
                   <p class="text-xs text-rose-600 leading-relaxed font-bold">Biên lợi nhuận gộp của đoàn này đang ở dưới mức an toàn (20%). Rà soát độ lệch vật tư (Material Out-In) và hiệu suất nhân sự.</p>
                </div>
             </div>

              <div class="flex gap-4">
                  <button class="flex-1 btn-premium secondary">
                   <Download class="w-4 h-4 text-emerald-500" /> Xuất Excel
                  </button>
                  <button v-if="authStore.hasPermission('HopDong.Approve')" @click="settleContract(selectedSettlement.contractId)" class="flex-1 btn-premium primary !bg-emerald-600 !shadow-emerald-100">
                   <CheckCircle class="w-4 h-4" /> Chốt Quyết Toán
                  </button>
                  <button @click="selectedSettlement = null" class="btn-premium secondary">
                   ĐÓNG
                  </button>
              </div>
            </div>
        </div>
      </Teleport>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { RefreshCw, FileText, Building2, X, Calculator, Users, Clock, Download, CheckCircle, AlertTriangle, Activity, Package, ArrowRight, Trash2, Search } from 'lucide-vue-next'
import apiClient from '../services/apiClient'
import { useToast } from '@/composables/useToast'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const toast = useToast()
const authStore = useAuthStore()
const loading = ref(false)
const contracts = ref([])
const searchQuery = ref('')
const selectedSettlement = ref(null)

const filteredContracts = computed(() => {
  if (!searchQuery.value) return contracts.value
  const q = searchQuery.value.toLowerCase()
  return contracts.value.filter(c => 
    c.contractName?.toLowerCase().includes(q) || 
    c.companyName?.toLowerCase().includes(q) ||
    c.contractCode?.toLowerCase().includes(q)
  )
})

// Extra Service editing state
const isEditingExtra = ref(false)
const tempExtraItems = ref([])
const tempVatRate = ref(0)
const savingExtra = ref(false)

const parsedExtraItems = computed(() => {
    if (!selectedSettlement.value?.extraServiceDetails) return []
    try {
        return JSON.parse(selectedSettlement.value.extraServiceDetails)
    } catch(e) {
        return []
    }
})

const startEditingExtra = () => {
    try {
        const details = selectedSettlement.value.extraServiceDetails;
        tempExtraItems.value = details ? JSON.parse(details) : [];
    } catch(e) {
        tempExtraItems.value = []
    }
    tempVatRate.value = selectedSettlement.value.vatRate || 0
    isEditingExtra.value = true
}

const addExtraItem = () => {
    tempExtraItems.value.push({ name: '', amount: 0 })
}

const removeExtraItem = (index) => {
    tempExtraItems.value.splice(index, 1)
}

const saveExtraService = async () => {
    savingExtra.value = true
    try {
        const totalAmount = tempExtraItems.value.reduce((sum, item) => sum + (Number(item.amount) || 0), 0)
        
        const payload = {
            extraServiceRevenue: totalAmount,
            extraServiceDetails: JSON.stringify(tempExtraItems.value),
            vatRate: tempVatRate.value || 0
        }
        
        await apiClient.put(`/api/Contracts/${selectedSettlement.value.contractId}/extra-service`, payload, {
            headers: { 'Content-Type': 'application/json' }
        })
        toast.success('Đã lưu dữ liệu phát sinh!')
        isEditingExtra.value = false
        // Tải lại để tính toán phần lợi nhuận (Revenue)
        await viewSettlement(contracts.value.find(c => c.healthContractId === selectedSettlement.value.contractId))
    } catch (e) {
        toast.error('Có lỗi xảy ra khi lưu chi phí phát sinh.')
    } finally {
        savingExtra.value = false
    }
}

const loadContracts = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/api/Contracts')
    contracts.value = res.data
  } catch (e) {
    toast.error('Lỗi tải danh sách hợp đồng')
  } finally {
    loading.value = false
  }
}

const viewSettlement = async (contract) => {
  loading.value = true
  try {
    const res = await apiClient.get(`/api/Contracts/${contract.healthContractId}/pnl-report`)
    selectedSettlement.value = res.data
    
    // Add extra details JSON from the original contract to keep editing capability
    selectedSettlement.value.extraServiceDetails = contract.extraServiceDetails
  } catch (e) {
    if (e.response && e.response.data && e.response.data.message) {
      toast.error(`ServiceResult: ${e.response.data.message}`)
    } else {
      toast.error('Không thể tính toán P&L cho hợp đồng này')
    }
  } finally {
    loading.value = false
  }
}

const settleContract = async (contractId) => {
  if (!confirm('Bạn có chắc chắn muốn chốt quyết toán chi phí cho hợp đồng này không? Dữ liệu sau khi chốt sẽ không thể sửa chữa.')) return
  loading.value = true
  try {
    await apiClient.post(`/api/Contracts/${contractId}/costs/settle`, "Quyết toán hoàn tất", { headers: { 'Content-Type': 'application/json' } })
    toast.success('Đã chốt quyết toán thành công!')
    selectedSettlement.value = null
    await loadContracts()
  } catch (e) {
    if (e.response?.status === 403) {
      toast.error('Bạn không có quyền chốt hợp đồng (HopDong.Approve).')
    } else {
      toast.error('Lỗi khi chốt quyết toán.')
    }
  } finally {
    loading.value = false
  }
}

const formatCurrency = (val) => {
  if (val === undefined || val === null || isNaN(val)) return '0 ₫'
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
}

const getStatusBadgeClass = (status) => {
  if (status === 'Completed') return 'success'
  if (status === 'Settled' || status === 'Finalized') return 'info'
  return 'warning'
}

onMounted(async () => {
  await loadContracts()
  if (route.query.contractId) {
    const contract = contracts.value.find(c => c.healthContractId == route.query.contractId)
    if (contract) {
      viewSettlement(contract)
    }
  }
})
</script>

<style scoped>
.animate-in {
  animation: animate-in 0.3s ease-out;
}
@keyframes animate-in {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}
</style>
