<template>
  <div class="space-y-6 animate-fade-in">
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
      <div>
        <h2 class="text-3xl font-bold tracking-tight flex items-center gap-3 text-gray-800">
          <div class="bg-primary p-2 rounded-xl">
            <Package class="w-6 h-6 text-white" />
          </div>
          Kho Vật tư Y tế
        </h2>
        <p class="text-gray-500 font-medium mt-1">Quản lý định mức tiêu hao và tài sản cố định</p>
      </div>
      <button v-if="authStore.role === 'Admin' || authStore.role === 'WarehouseManager'" 
              @click="showForm = !showForm" 
              class="px-6 py-3 bg-primary hover:bg-primary-dark text-white rounded-xl font-bold transition-all shadow-md shadow-primary/20 flex items-center gap-2">
        <Plus class="w-5 h-5" />
        <span>THÊM VẬT TƯ</span>
      </button>
    </div>

    <!-- Inventory Stats -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div class="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
        <p class="text-[10px] font-bold uppercase tracking-widest text-gray-400 mb-2">Giá trị tồn kho</p>
        <p class="text-2xl font-bold text-gray-900">{{ formatPrice(list.reduce((sum, item) => sum + (item.unitPrice * item.stockQuantity), 0)) }}</p>
      </div>
      <div class="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
        <p class="text-[10px] font-bold uppercase tracking-widest text-gray-400 mb-2">Vật tư tiêu hao</p>
        <p class="text-3xl font-bold text-gray-900">{{ list.filter(i => !i.isFixedAsset).length }} <span class="text-xs font-medium text-gray-400 ml-1">Loại</span></p>
      </div>
      <div class="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
        <p class="text-[10px] font-bold uppercase tracking-widest text-gray-400 mb-2">Tài sản cố định</p>
        <p class="text-3xl font-bold text-gray-900">{{ list.filter(i => i.isFixedAsset).length }} <span class="text-xs font-medium text-gray-400 ml-1">Loại</span></p>
      </div>
    </div>

    <!-- Tab Navigation -->
    <div class="flex space-x-8 border-b border-gray-100">
      <button @click="activeTab = 'inventory'" :class="['pb-4 font-bold text-sm transition-all relative uppercase tracking-widest', activeTab === 'inventory' ? 'text-primary' : 'text-gray-400 hover:text-gray-600']">
        <div class="flex items-center gap-2">
            <Package class="w-4 h-4" />
            <span>Tồn kho hiện tại</span>
        </div>
        <div v-if="activeTab === 'inventory'" class="absolute bottom-0 left-0 right-0 h-1 bg-primary rounded-t-full"></div>
      </button>
      <button @click="activeTab = 'history'" :class="['pb-4 font-bold text-sm transition-all relative uppercase tracking-widest', activeTab === 'history' ? 'text-primary' : 'text-gray-400 hover:text-gray-600']">
        <div class="flex items-center gap-2">
            <Clock class="w-4 h-4" />
            <span>Lịch sử luân chuyển</span>
        </div>
        <div v-if="activeTab === 'history'" class="absolute bottom-0 left-0 right-0 h-1 bg-primary rounded-t-full"></div>
      </button>
    </div>

    <!-- Inventory Tab -->
    <div v-show="activeTab === 'inventory'">
      <!-- Form -->
      <div v-if="showForm || editingSupply" class="bg-white p-10 rounded-[3rem] neo-shadow border border-white/20 mb-10 animate-scale-up">
          <h3 class="text-2xl font-black mb-10 text-slate-800 flex items-center gap-4">
              <div class="w-12 h-12 bg-indigo-50 text-indigo-600 rounded-2xl flex items-center justify-center">
                  <Package class="w-6 h-6" />
              </div>
              {{ editingSupply ? 'Hiệu chỉnh thông tin vật tư' : 'Khai báo vật tư/thiết bị mới' }}
          </h3>
          <form @submit.prevent="editingSupply ? updateSupply() : addSupply()" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
              <div class="space-y-3 lg:col-span-2">
                  <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Tên gọi vật tư/thiết bị</label>
                  <input v-model="currentSupply.supplyName" required :disabled="isLoading" class="input-premium" placeholder="Nhập tên vật tư..." />
              </div>
              <div class="space-y-3">
                  <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Phân loại tài sản</label>
                  <select v-model="currentSupply.isFixedAsset" :disabled="isLoading" class="input-premium">
                      <option :value="false">Vật tư tiêu hao (Một lần)</option>
                      <option :value="true">Tài sản cố định (Tái sử dụng)</option>
                  </select>
              </div>
              <div class="space-y-3">
                  <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">{{ editingSupply ? 'Tồn kho hiện tại' : 'Số lượng nhập kho' }}</label>
                  <input v-model="currentSupply.stockQuantity" type="number" required :disabled="isLoading" class="input-premium text-2xl font-black tracking-tighter" />
              </div>
              <div class="space-y-3 lg:col-span-2">
                  <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Đơn giá nhập (VNĐ)</label>
                  <div class="relative">
                      <input 
                        type="text" 
                        :value="currentSupply.unitPrice.toLocaleString('vi-VN')" 
                        :disabled="isLoading"
                        @input="e => {
                          const val = e.target.value.replace(/\D/g, '');
                          currentSupply.unitPrice = val ? parseInt(val) : 0;
                          e.target.value = currentSupply.unitPrice.toLocaleString('vi-VN');
                        }"
                        required 
                        class="input-premium pl-8 pr-16 text-2xl font-black text-indigo-600 tracking-tighter" 
                      />
                      <span class="absolute right-6 top-1/2 -translate-y-1/2 text-xs font-black text-slate-300">VNĐ</span>
                  </div>
              </div>
              <div class="lg:col-span-2 flex items-end justify-end gap-4">
                  <button type="button" @click="cancelEdit" :disabled="isLoading" class="px-8 py-4 text-slate-400 font-black hover:bg-slate-50 rounded-2xl transition-all uppercase tracking-widest text-[10px]">Hủy bỏ</button>
                  <button type="submit" :disabled="isLoading" class="btn-premium bg-indigo-600 text-white shadow-indigo-600/20">
                      <Loader v-if="isLoading" class="w-5 h-5 animate-spin" />
                      <span>{{ isLoading ? 'ĐANG XỬ LÝ...' : (editingSupply ? 'CẬP NHẬT' : 'KHỞI TẠO VẬT TƯ') }}</span>
                  </button>
              </div>
          </form>
      </div>

      <!-- Inventory Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
          <div v-for="item in list" :key="item.supplyId" 
               class="premium-card p-8 group overflow-hidden relative">
              
              <!-- Asset Status Ribbon -->
              <div v-if="item.isFixedAsset" class="absolute -right-12 top-6 rotate-45 bg-indigo-600 text-white text-[8px] font-black px-12 py-1 uppercase tracking-widest shadow-sm">
                  Tài sản
              </div>

              <div class="flex justify-between items-start mb-8">
                  <div :class="['w-14 h-14 rounded-2xl flex items-center justify-center shadow-inner transition-transform group-hover:scale-110 duration-500', item.isFixedAsset ? 'bg-indigo-50 text-indigo-600' : 'bg-emerald-50 text-emerald-600']">
                      <component :is="item.isFixedAsset ? HardDrive : Droplets" class="w-7 h-7" />
                  </div>
                  <div class="text-right">
                      <span class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1 block">Tồn kho</span>
                      <p :class="['font-black text-3xl tracking-tighter', item.stockQuantity < 10 ? 'text-rose-500 animate-pulse' : 'text-slate-900']">{{ item.stockQuantity }}</p>
                  </div>
              </div>

              <h4 class="font-black text-slate-800 text-lg mb-4 leading-tight min-h-[56px] group-hover:text-primary transition-colors tracking-tight">{{ item.supplyName }}</h4>
              
              <div class="pt-6 border-t-2 border-slate-50 flex justify-between items-center mb-8">
                  <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Đơn giá nhập</span>
                  <span class="font-black text-indigo-600">{{ formatPrice(item.unitPrice) }}</span>
              </div>

              <!-- Action Buttons -->
              <div v-if="authStore.role === 'Admin' || authStore.role === 'WarehouseManager'" class="flex items-center gap-2">
                  <button @click="showRestockModal(item)" class="flex-1 bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-3 rounded-xl text-[10px] font-black transition-all shadow-lg shadow-emerald-100 uppercase tracking-widest active:scale-95">
                      NHẬP KHO
                  </button>
                  <button @click="editSupply(item)" class="p-3 bg-slate-50 text-slate-400 rounded-xl hover:bg-indigo-50 hover:text-indigo-600 transition-all border border-transparent hover:border-indigo-100"><Edit2 class="w-4 h-4" /></button>
                  <button @click="confirmDelete(item)" class="p-3 bg-slate-50 text-slate-400 rounded-xl hover:bg-rose-50 hover:text-rose-600 transition-all border border-transparent hover:border-rose-100"><Trash2 class="w-4 h-4" /></button>
              </div>
          </div>
          
          <div v-if="list.length === 0" class="col-span-full py-40 flex flex-col items-center justify-center gap-6 bg-slate-50/20 rounded-[3rem] border-4 border-dashed border-slate-100">
              <div class="w-24 h-24 bg-white rounded-full flex items-center justify-center text-slate-100 shadow-sm">
                  <Package class="w-12 h-12" />
              </div>
              <p class="text-slate-300 font-black uppercase tracking-[0.3em] text-sm">Kho hàng trống</p>
          </div>
      </div>
    </div>

    <!-- History Tab -->
    <div v-show="activeTab === 'history'" class="premium-card overflow-hidden">
        <div class="p-8 border-b-2 border-slate-50 bg-slate-50/30">
            <h3 class="text-xl font-black text-slate-800 tracking-tight flex items-center gap-3">
                <Clock class="w-6 h-6 text-indigo-500" />
                LỊCH SỬ BIẾN ĐỘNG KHO
            </h3>
        </div>
        <div class="overflow-x-auto">
            <table class="professional-table">
                <thead>
                    <tr>
                        <th class="text-left border-r border-slate-100">Thời gian</th>
                        <th class="text-center border-r border-slate-100">Loại giao dịch</th>
                        <th class="text-left border-r border-slate-100">Vật tư & Nội dung</th>
                        <th class="text-right border-r border-slate-100">Số lượng</th>
                        <th class="text-left">Người xử lý</th>
                    </tr>
                </thead>
                <tbody class="divide-y-2 divide-slate-50">
                    <tr v-for="t in supplyHistory" :key="t.transactionId" class="group">
                        <td class="border-r border-slate-50">
                            <div class="font-black text-slate-600 text-sm">{{ formatDate(t.transactionDate) }}</div>
                            <div class="text-[10px] text-slate-400 font-bold uppercase tracking-tighter">{{ new Date(t.transactionDate).toLocaleTimeString('vi-VN') }}</div>
                        </td>
                        <td class="text-center border-r border-slate-50">
                            <span :class="['status-badge', t.type === 'IMPORT' ? 'bg-emerald-100 text-emerald-700' : 'bg-rose-100 text-rose-700']">
                                <ArrowDown v-if="t.type==='IMPORT'" class="w-3 h-3 rotate-180" />
                                <ArrowDown v-else class="w-3 h-3" />
                                {{ t.type === 'IMPORT' ? 'NHẬP KHO' : 'XUẤT KHO' }}
                            </span>
                        </td>
                        <td class="border-r border-slate-50">
                            <p class="font-black text-slate-800 group-hover:text-primary transition-colors text-base leading-tight">{{ t.supplyName }}</p>
                            <p class="text-[10px] text-slate-400 font-bold uppercase tracking-widest mt-0.5">{{ t.note || 'Không có ghi chú' }}</p>
                        </td>
                        <td class="text-right border-r border-slate-50 font-black text-xl tracking-tighter">
                            <span :class="t.type === 'IMPORT' ? 'text-emerald-600' : 'text-slate-900'">
                                {{ t.type === 'IMPORT' ? '+' : '-' }}{{ t.quantity }}
                            </span>
                        </td>
                        <td>
                            <div class="flex items-center gap-3">
                                <div class="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center text-[10px] font-black text-slate-500 border-2 border-white shadow-sm">
                                    {{ t.processedBy?.[0] || 'S' }}
                                </div>
                                <span class="text-sm font-bold text-slate-500">{{ t.processedBy || 'Hệ thống' }}</span>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        <div v-if="supplyHistory.length === 0" class="py-32 flex flex-col items-center justify-center gap-4 bg-slate-50/20">
            <Clock class="w-12 h-12 text-slate-200" />
            <p class="text-slate-400 font-black uppercase tracking-[0.2em] text-sm">Chưa có giao dịch kho</p>
        </div>
    </div>

    <!-- Restock Modal -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="showRestock" class="fixed inset-0 z-[9998] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4" @click.self="showRestock = false">
          <div class="bg-white max-w-md w-full p-8 rounded-[2.5rem] shadow-2xl">
            <h3 class="text-xl font-black mb-6 text-slate-800 flex items-center">
              <div class="w-2 h-6 bg-success rounded-full mr-3"></div>
              Nhập thêm hàng
            </h3>
            <p class="text-slate-600 font-bold mb-2">{{ restockItem?.supplyName }}</p>
            <p class="text-sm text-slate-400 mb-6">Tồn kho hiện tại: <span class="font-black text-slate-700">{{ restockItem?.stockQuantity }}</span></p>
            <input v-model="restockQty" type="number" min="1" placeholder="Số lượng nhập thêm" class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-emerald-500/20 outline-none transition-all font-black text-2xl text-emerald-600 mb-4" />
            <input v-model="restockNote" type="text" placeholder="Lý do nhập (Vd: Nhập mới, Hoàn trả...)" class="w-full px-5 py-4 rounded-xl bg-slate-50 border border-slate-100 outline-none transition-all font-bold text-sm mb-6" />
            <div class="flex space-x-4">
              <button @click="showRestock = false" class="flex-1 px-6 py-3 rounded-xl font-black text-slate-500 hover:bg-slate-100 transition-all">Hủy</button>
              <button @click="restockSupply" class="flex-1 px-6 py-3 rounded-xl font-black text-white bg-success hover:bg-success/90 transition-all">Xác nhận</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Confirm Delete Dialog -->
    <ConfirmDialog 
      v-model="showDeleteConfirm"
      title="Xóa vật tư?"
      :message="`Bạn có chắc muốn xóa &quot;${supplyToDelete?.supplyName}&quot;? Dữ liệu không thể khôi phục.`"
      confirmText="Xóa ngay"
      variant="danger"
      @confirm="deleteSupply"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'
import { Plus, ArrowDown, HardDrive, Droplets, Package, Edit2, Trash2, Loader, Clock } from 'lucide-vue-next'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import { useToast } from '../composables/useToast'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const toast = useToast()
const list = ref([])
const activeTab = ref('inventory')
const supplyHistory = ref([])
const showForm = ref(false)
const isLoading = ref(false)
const editingSupply = ref(null)
const showDeleteConfirm = ref(false)
const supplyToDelete = ref(null)
const showRestock = ref(false)
const restockItem = ref(null)
const restockQty = ref(0)
const restockNote = ref('')

const currentSupply = ref({
    supplyName: '',
    isFixedAsset: false,
    unitPrice: 0,
    stockQuantity: 0
})

const fetchList = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Supplies')
        list.value = res.data
    } catch (e) { 
        console.error(e)
        toast.error('Không thể tải danh sách vật tư')
    }
}

const fetchHistory = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Supplies/transactions')
        supplyHistory.value = res.data
    } catch (e) { 
        console.error(e)
        // toast.error('Không thể tải lịch sử giao dịch')
    }
}

const getHistoryTypeBadge = (type) => {
    if (type === 'IMPORT') return 'bg-emerald-50 text-emerald-600 border border-emerald-100'
    if (type === 'EXPORT') return 'bg-primary/10 text-primary border border-primary/20'
    return 'bg-blue-50 text-blue-600 border border-blue-100'
}

const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('vi-VN', { 
        year: 'numeric', 
        month: '2-digit', 
        day: '2-digit' 
    })
}

const addSupply = async () => {
    try {
        isLoading.value = true
        await axios.post('http://localhost:5283/api/Supplies', currentSupply.value)
        toast.success('Đã nhập vật tư mới thành công!')
        await fetchList()
        showForm.value = false
        resetForm()
    } catch (e) { 
        console.error(e)
        toast.error('Không thể thêm vật tư')
    } finally {
        isLoading.value = false
    }
}

const editSupply = (supply) => {
    editingSupply.value = supply
    currentSupply.value = { ...supply }
    showForm.value = false
}

const updateSupply = async () => {
    try {
        isLoading.value = true
        await axios.put(`http://localhost:5283/api/Supplies/${editingSupply.value.supplyId}`, currentSupply.value)
        toast.success('Cập nhật vật tư thành công!')
        await fetchList()
        cancelEdit()
    } catch (e) {
        console.error(e)
        toast.error('Không thể cập nhật vật tư')
    } finally {
        isLoading.value = false
    }
}

const confirmDelete = (supply) => {
    supplyToDelete.value = supply
    showDeleteConfirm.value = true
}

const deleteSupply = async () => {
    try {
        isLoading.value = true
        await axios.delete(`http://localhost:5283/api/Supplies/${supplyToDelete.value.supplyId}`)
        toast.success(`Đã xóa "${supplyToDelete.value.supplyName}"`)
        await fetchList()
        supplyToDelete.value = null
    } catch (e) {
        console.error(e)
        toast.error('Không thể xóa vật tư này')
    } finally {
        isLoading.value = false
    }
}

const showRestockModal = (item) => {
    restockItem.value = item
    restockQty.value = 0
    showRestock.value = true
}

const restockSupply = async () => {
    if (!restockQty.value || restockQty.value <= 0) {
        return toast.warning('Vui lòng nhập số lượng hợp lệ')
    }
    try {
        await axios.post(`http://localhost:5283/api/Supplies/import/${restockItem.value.supplyId}`, { 
            quantity: restockQty.value,
            note: restockNote.value
        })
        toast.success(`Đã nhập thêm ${restockQty.value} ${restockItem.value.supplyName}`)
        await fetchList()
        await fetchHistory()
        showRestock.value = false
        restockItem.value = null
        restockQty.value = 0
        restockNote.value = ''
    } catch (e) {
        console.error(e)
        toast.error('Không thể nhập thêm hàng')
    }
}

const cancelEdit = () => {
    editingSupply.value = null
    showForm.value = false
    resetForm()
}

const resetForm = () => {
    currentSupply.value = { supplyName: '', isFixedAsset: false, unitPrice: 0, stockQuantity: 0 }
}

const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)

onMounted(() => {
    fetchList()
    fetchHistory()
})
</script>
