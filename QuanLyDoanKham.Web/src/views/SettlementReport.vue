<template>
  <div class="h-full flex flex-col bg-slate-50 relative animate-fade-in p-3 scrollbar-premium overflow-y-auto font-sans">
    <div class="max-w-full mx-auto w-full">
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-3 mb-4">
        <div>
          <h2 class="text-xl font-bold text-slate-800 flex items-center gap-2">
            <div class="w-9 h-9 bg-primary text-white rounded-xl flex items-center justify-center shadow-md">
              <Calculator class="w-5 h-5" />
            </div>
            Đối Soát & Quyết Toán
          </h2>
          <p class="text-slate-400 font-semibold uppercase tracking-widest text-[8px] mt-1">Phân tích chênh lệch giá trị dự kiến và thực tế (P&L Audit)</p>
        </div>
        <div class="flex gap-2">
          <button @click="loadContracts" class="h-9 w-9 bg-white border border-slate-200 rounded-xl text-slate-600 hover:text-primary transition-all shadow-sm flex items-center justify-center group">
            <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />
          </button>
        </div>
      </div>

      <div class="premium-card bg-white rounded-xl shadow-sm mb-4 overflow-hidden border border-slate-100">
        <div class="p-3 border-b border-slate-50 flex items-center gap-3 bg-white/50">
            <div class="relative group flex-1">
                <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-300 w-3.5 h-3.5" />
                <input v-model="searchQuery" placeholder="Tìm tên hợp đồng, công ty..."
                       class="w-full pl-9 pr-4 py-2 rounded-lg bg-white border border-slate-200 focus:border-primary/30 focus:ring-2 focus:ring-primary/10 outline-none font-semibold text-[11px] text-slate-600 shadow-sm transition-all" />
            </div>
            <div class="px-2.5 py-1 bg-white rounded-lg border border-slate-100 text-[8px] font-semibold text-slate-400 uppercase tracking-widest whitespace-nowrap">
                Kết quả: <span class="text-primary font-bold">{{ filteredContracts.length }}</span>
            </div>
        </div>

        <div v-if="isLoading" class="p-4 space-y-3">
          <div v-for="i in 3" :key="i"
               class="h-16 bg-slate-100 rounded-lg animate-pulse" />
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-left">
            <thead class="bg-slate-50 text-[8px] font-semibold uppercase tracking-widest text-slate-500 border-b border-slate-100">
              <tr>
                <th class="px-3 py-2 text-center w-12">STT</th>
                <th class="px-3 py-2">Hợp đồng</th>
                <th class="px-3 py-2">Doanh nghiệp</th>
                <th class="px-3 py-2 text-right">Giá trị</th>
                <th class="px-3 py-2 text-center">Dự kiến</th>
                <th class="px-3 py-2 text-center">Trạng thái</th>
                <th class="px-3 py-2 text-center">Tác vụ</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-50">
              <tr v-for="(c, index) in filteredContracts" :key="c.healthContractId"
                @click="viewSettlement(c)"
                class="text-[10px] hover:bg-sky-50/50 transition-all duration-200 group cursor-pointer border-b border-slate-50 last:border-0">
                <td class="px-3 py-1.5 text-center font-semibold text-slate-400 tabular-nums">
                  {{ String(index + 1).padStart(3, '0') }}
                </td>
                <td class="px-3 py-1.5">
                  <div class="flex items-center gap-2">
                    <div class="w-7 h-7 rounded-lg bg-primary/10 text-primary flex items-center justify-center shadow-sm group-hover:scale-110 transition-transform duration-300">
                      <FileText class="w-3.5 h-3.5" />
                    </div>
                    <div>
                      <div class="font-bold text-slate-800 uppercase italic leading-tight group-hover:text-primary transition-colors text-[10px]">{{ c.contractName }}</div>
                      <div class="text-[7px] text-slate-400 font-bold uppercase tracking-wider mt-0.5">{{ c.contractCode || 'HĐ-' + c.healthContractId }}</div>
                    </div>
                  </div>
                </td>
                <td class="px-3 py-1.5">
                  <div class="flex items-center gap-2 font-semibold text-slate-600">
                    <Building2 class="w-3 h-3 text-slate-300" />
                    {{ c.companyName }}
                  </div>
                </td>
                <td class="px-3 py-1.5 text-right font-black text-slate-700 tabular-nums">
                  {{ formatCurrency(c.totalValue) }}
                </td>
                <td class="px-3 py-1.5 text-center">
                  <span class="px-1.5 py-0.5 bg-slate-100 rounded-md font-bold text-slate-600 text-[9px] shadow-sm whitespace-nowrap">
                    {{ c.expectedQuantity }} người
                  </span>
                </td>
                <td class="px-3 py-1.5 text-center">
                  <span :class="getStatusBadgeClass(c.status)" class="px-1.5 py-0.5 rounded-full text-[7px] font-black uppercase tracking-widest shadow-sm">
                    {{ c.status === 'Completed' || c.status === 'Active' ? 'HOÀN TẤT' : 'ĐANG XỬ LÝ' }}
                  </span>
                </td>
                <td class="px-3 py-1.5 text-center">
                  <button @click.stop="viewSettlement(c)" 
                    class="w-7 h-7 bg-indigo-50 text-indigo-600 rounded-lg flex items-center justify-center hover:bg-indigo-600 hover:text-white transition-all shadow-sm"
                    title="Phân tích quyết toán">
                    <Activity class="w-4 h-4" />
                  </button>
                </td>
              </tr>
              <tr v-if="filteredContracts.length === 0">
                <td colspan="7" class="py-20 text-center">
                  <div class="flex flex-col items-center justify-center py-16 text-slate-400">
                    <component :is="Inbox" class="w-12 h-12 mb-3 opacity-40" />
                    <p class="font-bold text-sm">Chưa có dữ liệu</p>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <Teleport to="body">
        <div v-if="selectedSettlement" class="modal-overlay flex items-center justify-center p-4" @click.self="selectedSettlement = null">
          <div class="modal-box w-full max-w-xl overflow-hidden animate-scale-up !rounded-xl flex flex-col max-h-[90vh]">
            <div class="bg-gradient-to-br from-slate-800 to-slate-900 p-5 text-white relative overflow-hidden shrink-0">
              <div class="absolute -right-10 -top-10 w-48 h-48 bg-primary/20 rounded-full blur-[60px]"></div>
              <button @click="selectedSettlement = null" class="absolute top-4 right-4 w-7 h-7 rounded-full bg-white/10 hover:bg-white/20 flex items-center justify-center transition-all group z-10">
                <X class="w-3.5 h-3.5 text-white/70 group-hover:text-white group-hover:rotate-90 transition-all" />
              </button>
              
              <div class="relative z-10">
                <div class="flex items-center gap-2 mb-2">
                  <div class="w-9 h-9 bg-primary rounded-xl flex items-center justify-center shadow-lg shadow-primary/20">
                    <Calculator class="w-5 h-5" />
                  </div>
                  <div>
                    <h2 class="text-lg font-black tracking-tight italic uppercase">QUYẾT TOÁN</h2>
                    <p class="text-[7px] font-bold opacity-60 uppercase tracking-[0.2em] mt-0.5">Audit Report Analysis</p>
                  </div>
                </div>
                <p class="text-xs font-black text-white/90 uppercase tracking-widest leading-tight max-w-md">{{ selectedSettlement.contractName }}</p>
              </div>
            </div>
            
            <div class="p-4 overflow-y-auto custom-scrollbar flex-1">
             <!-- Revenue & Gross Profit -->
             <div class="grid grid-cols-2 gap-3 mb-4">
                <!-- Net Revenue -->
                <div class="bg-slate-900 p-4 rounded-xl shadow-md relative overflow-hidden group border-none">
                  <div class="absolute -right-3 -bottom-3 opacity-10 text-primary transform group-hover:scale-110 transition-transform">
                    <Calculator class="w-16 h-16" />
                  </div>
                  <p class="text-[8px] font-black text-slate-300 uppercase tracking-widest mb-1">Doanh Thu Thuần</p>
                  <h4 class="text-lg font-black text-white tabular-nums">{{ formatCurrency(selectedSettlement.netRevenue) }}</h4>
                  <div class="flex items-center gap-1.5 mt-1">
                    <span class="text-[8px] text-primary font-bold uppercase tracking-wide">{{ selectedSettlement.completedRecords }} ca COMPLETED</span>
                  </div>
                </div>
                
                <!-- Gross Profit -->
                <div :class="[selectedSettlement.profitMargin < 20 ? 'bg-rose-50 border-rose-100' : 'bg-white/80 border-white/50 backdrop-blur-md shadow-glass']" class="p-5 rounded-2xl border relative overflow-hidden group">
                  <div class="absolute -right-4 -bottom-4 opacity-5 transform group-hover:scale-110 transition-transform">
                    <Activity class="w-24 h-24 text-emerald-500" />
                  </div>
                  <p :class="selectedSettlement.profitMargin < 20 ? 'text-rose-400' : 'text-emerald-500'" class="text-[9px] font-black uppercase tracking-widest mb-1.5">
                    Lợi Nhuận Gộp
                  </p>
                  <h4 :class="selectedSettlement.profitMargin < 20 ? 'text-rose-700' : 'text-emerald-700'" class="text-xl font-black tabular-nums">
                    {{ formatCurrency(selectedSettlement.grossProfit) }}
                  </h4>
                  <div class="flex items-center gap-2 mt-1.5">
                    <span :class="selectedSettlement.profitMargin < 20 ? 'bg-rose-200 text-rose-700' : 'bg-emerald-100 text-emerald-700'" class="px-2 py-0.5 rounded-lg text-[10px] font-black shadow-sm">
                       {{ selectedSettlement.profitMargin?.toFixed(1) || 0 }}%
                    </span>
                    <span :class="selectedSettlement.profitMargin < 20 ? 'text-rose-600' : 'text-emerald-600'" class="text-[9px] font-black uppercase tracking-widest opacity-60">
                       Margin %
                    </span>
                  </div>
                </div>
             </div>

             <!-- Breakdown Costs -->
             <div class="mb-4">
                <h3 class="text-[10px] font-black text-slate-800 uppercase tracking-widest mb-3">Cấu trúc Chi Phí</h3>
                
                <div class="space-y-3 bg-slate-50/50 p-4 rounded-xl border border-slate-100">
                     <div class="flex justify-between items-center">
                       <div class="flex items-center gap-2">
                          <div class="w-7 h-7 rounded-lg bg-blue-50 text-blue-500 flex items-center justify-center shadow-sm"><Users class="w-4 h-4" /></div>
                          <span class="font-bold text-slate-600 italic text-[11px]">Lương nhân sự</span>
                       </div>
                       <span class="font-black text-slate-800 text-[11px]">{{ formatCurrency(selectedSettlement.laborCost) }}</span>
                    </div>

                    <div class="flex justify-between items-center">
                       <div class="flex items-center gap-2">
                          <div class="w-7 h-7 rounded-lg bg-amber-50 text-amber-500 flex items-center justify-center shadow-sm"><Package class="w-4 h-4" /></div>
                          <span class="font-bold text-slate-600 italic text-[11px]">Vật tư thực tế</span>
                       </div>
                       <span class="font-black text-slate-800 text-[11px]">{{ formatCurrency(selectedSettlement.materialCost) }}</span>
                    </div>

                    <div class="flex justify-between items-center">
                       <div class="flex items-center gap-2">
                          <div class="w-7 h-7 rounded-lg bg-purple-50 text-purple-500 flex items-center justify-center shadow-sm"><Clock class="w-4 h-4" /></div>
                          <span class="font-bold text-slate-600 italic text-[11px]">Chi phí chung</span>
                       </div>
                       <span class="font-black text-slate-800 text-[11px]">{{ formatCurrency(selectedSettlement.overheadCost) }}</span>
                    </div>

                    <div class="my-2 border-t border-slate-100"></div>

                    <div class="flex justify-between items-center px-1">
                       <span class="font-black text-slate-800 text-[10px] uppercase tracking-widest">Tổng Chi Phí</span>
                       <span class="font-black text-rose-500 text-lg">{{ formatCurrency(selectedSettlement.totalCost) }}</span>
                    </div>

                    <!-- Dịch vụ ngoài gói -->
                    <div class="my-2 border-t border-slate-100"></div>
                    <div class="p-3 bg-orange-50/50 rounded-xl border border-orange-100 mt-2">
                       <div class="flex justify-between items-center mb-2">
                           <span class="font-black text-orange-800 text-[10px] uppercase tracking-widest">Phát sinh ngoài gói</span>
                           <button v-if="!isEditingExtra && authStore.hasPermission('QuyetToan.Edit')" @click="startEditingExtra" class="text-[9px] font-bold text-orange-600 hover:text-orange-700 bg-orange-100 px-2 py-0.5 rounded-full uppercase">Sửa đổi</button>
                       </div>
                       
                       <div v-if="!isEditingExtra">
                           <div class="flex justify-between items-center mb-2">
                               <span class="text-[10px] text-orange-700 font-medium italic">Doanh thu ngoài gói</span>
                               <span class="font-black text-orange-600 text-sm">{{ formatCurrency(selectedSettlement.extraServiceRevenue) }}</span>
                           </div>
                           <div class="flex justify-between items-center mb-2">
                               <span class="text-[9px] text-orange-700 font-medium italic">Thuế VAT: {{ selectedSettlement.vatRate }}% ( {{ formatCurrency(selectedSettlement.vatAmount) }} )</span>
                           </div>
                           <ul class="text-[9px] text-orange-800 space-y-1" v-if="parsedExtraItems.length > 0">
                               <li v-for="(item, idx) in parsedExtraItems" :key="idx" class="flex justify-between border-b border-orange-200/30 pb-0.5">
                                  <span>{{ item.name }}</span>
                                  <span class="font-bold">{{ formatCurrency(item.amount) }}</span>
                               </li>
                           </ul>
                           <p v-else class="text-[9px] text-orange-600 italic">Không có chi tiết</p>
                       </div>
                       
                       <div v-else class="flex flex-col gap-1.5 mt-1.5">
                           <div class="flex gap-2 items-center mb-1">
                               <label class="text-[10px] font-bold text-orange-800 whitespace-nowrap">VAT (%):</label>
                               <input type="number" v-model.number="tempVatRate" class="w-16 bg-white border border-orange-200 rounded-lg px-2 py-1 text-slate-800 font-bold text-[10px] outline-none" />
                           </div>
                           
                           <div v-for="(item, idx) in tempExtraItems" :key="idx" class="flex gap-1.5 items-center">
                               <input type="text" v-model="item.name" class="flex-1 bg-white border border-orange-200 rounded-lg px-2 py-1.5 text-slate-800 text-[10px] outline-none" placeholder="Dịch vụ..." />
                               <input type="number" v-model.number="item.amount" class="w-24 bg-white border border-orange-200 rounded-lg px-2 py-1.5 text-slate-800 font-bold text-[10px] outline-none" placeholder="Tiền..." />
                               <button @click="removeExtraItem(idx)" class="text-rose-500 hover:bg-rose-50 p-1.5 rounded-lg transition">
                                  <Trash2 class="w-3.5 h-3.5" />
                                </button>
                           </div>
                           
                           <div class="flex gap-2 items-center mt-1.5">
                               <button @click="addExtraItem" class="bg-orange-100 text-orange-700 font-bold px-3 py-1.5 rounded-lg hover:bg-orange-200 transition text-[9px] flex-1 border border-orange-200 border-dashed">
                                   + Thêm Mục
                               </button>
                               <button @click="saveExtraService" :disabled="savingExtra" class="bg-orange-600 text-white font-bold px-3 py-1.5 rounded-lg hover:bg-orange-700 transition text-[9px]">
                                   {{ savingExtra ? 'Lưu...' : 'Lưu' }}
                               </button>
                               <button @click="isEditingExtra = false" class="bg-white border border-slate-200 text-slate-500 font-bold px-3 py-1.5 rounded-lg hover:bg-slate-50 transition text-[9px]">Hủy</button>
                           </div>
                       </div>
                    </div>
                </div>
             </div>

             <!-- Alerts -->
             <div v-if="selectedSettlement.profitMargin < 20" class="p-3 mb-4 bg-rose-50 border border-rose-100 rounded-xl flex gap-3 items-start">
                <div class="w-9 h-9 shrink-0 bg-rose-200/50 text-rose-600 rounded-xl flex items-center justify-center"><AlertTriangle class="w-5 h-5 animate-bounce-slow" /></div>
                <div>
                   <p class="font-black text-rose-800 text-[10px] uppercase tracking-wider mb-0.5">Biên lợi nhuận thấp (Margin &lt; 20%)</p>
                   <p class="text-[9px] text-rose-600 leading-tight font-bold">Cần rà soát vật tư và nhân lực.</p>
                </div>
             </div>

              <div class="flex gap-3">
                  <button class="flex-1 h-9 flex items-center justify-center gap-1.5 bg-slate-100 text-slate-600 rounded-xl font-bold text-[10px] uppercase hover:bg-slate-200 transition-all">
                    <Download class="w-3.5 h-3.5 text-emerald-500" /> Xuất Excel
                  </button>
                  <button v-if="authStore.hasPermission('QuyetToan.Finalize')" @click="settleContract(selectedSettlement.contractId)" class="flex-2 h-9 flex items-center justify-center gap-1.5 bg-emerald-600 text-white rounded-xl font-bold text-[10px] uppercase hover:bg-emerald-700 transition-all shadow-md shadow-emerald-100">
                    <CheckCircle class="w-3.5 h-3.5" /> Chốt Quyết Toán
                  </button>
                  <button @click="selectedSettlement = null" class="h-9 px-4 bg-white border border-slate-200 text-slate-500 rounded-xl font-bold text-[10px] uppercase hover:bg-slate-50 transition-all">
                    ĐÓNG
                  </button>
              </div>
            </div>
          </div>
        </div>
      </Teleport>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { RefreshCw, FileText, Building2, X, Calculator, Users, Clock, Download, CheckCircle, AlertTriangle, Activity, Package, ArrowRight, Trash2, Search, Inbox } from 'lucide-vue-next'
import apiClient from '../services/apiClient'
import { useToast } from '@/composables/useToast'
import { useAuthStore } from '@/stores/auth'
import { parseApiError } from '../services/errorHelper'

const route = useRoute()
const toast = useToast()
const authStore = useAuthStore()
const loading = ref(false)
const isLoading = ref(false)
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
        toast.error(parseApiError(e))
    } finally {
        savingExtra.value = false
    }
}

const loadContracts = async () => {
  isLoading.value = true
  loading.value = true
  try {
    const res = await apiClient.get('/api/Contracts')
    contracts.value = res.data
  } catch (e) {
    toast.error(parseApiError(e))
  } finally {
    isLoading.value = false
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
    toast.error(parseApiError(e))
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
    toast.error(parseApiError(e))
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
