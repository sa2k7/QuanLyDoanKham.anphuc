<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-emerald-600 text-white rounded-2xl flex items-center justify-center shadow-lg">
            <Package class="w-6 h-6" />
          </div>
          Quản trị Kho & Vật tư
        </h2>
        <p class="text-slate-400 font-bold uppercase tracking-widest text-[9px] mt-2">Nội bộ: Xuất/Nhập vật tư theo Phiếu Kho</p>
      </div>

      <div class="flex items-center gap-4">
        <button v-if="authStore.role === 'Admin' || authStore.role === 'WarehouseManager'" 
                @click="openVoucherModal" 
                class="btn-premium bg-slate-900 text-white px-8 py-3 rounded-xl shadow-lg">
            <ClipboardList class="w-5 h-5" />
            <span>TẠO PHIẾU KHO</span>
        </button>
        <button v-if="authStore.role === 'Admin'" 
                @click="showAddSupply = !showAddSupply" 
                class="p-3 bg-white border-2 border-slate-50 text-slate-400 rounded-xl hover:text-emerald-600 transition-all">
            <Plus class="w-5 h-5" />
        </button>
      </div>
    </div>

    <!-- Form Khai báo vật tư mới (Chỉ Admin) -->
    <div v-if="showAddSupply" class="premium-card p-6 bg-white border-4 border-emerald-50 animate-slide-up">
        <h3 class="text-sm font-black text-slate-800 mb-4 uppercase">Khai báo Danh mục Vật tư</h3>
        <form @submit.prevent="addSupply" class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <input v-model="newSupply.supplyName" placeholder="Tên vật tư..." class="input-premium" required />
            <input v-model="newSupply.unit" placeholder="Đơn vị tính (VD: Cái, Hộp...)" class="input-premium" required />
            <button type="submit" class="bg-emerald-600 text-white font-black text-xs uppercase rounded-xl">THÊM DANH MỤC</button>
        </form>
    </div>

    <!-- Tab Filter -->
    <div class="flex items-center gap-4 mb-6">
        <button @click="activeTab = 'inventory'" 
                :class="['px-6 py-3 rounded-xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'inventory' ? 'bg-emerald-600 text-white shadow-lg' : 'bg-white text-slate-400 border-2 border-slate-50']">
            Tồn kho hiện tại ({{ list.length }})
        </button>
        <button @click="activeTab = 'vouchers'" 
                :class="['px-6 py-3 rounded-xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'vouchers' ? 'bg-slate-800 text-white shadow-lg' : 'bg-white text-slate-400 border-2 border-slate-50']">
            Lịch sử Phiếu Kho ({{ vouchers.length }})
        </button>
    </div>

    <!-- Inventory Tab -->
    <div v-if="activeTab === 'inventory'" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div v-for="item in list" :key="item.supplyId" class="premium-card p-6 bg-white border border-slate-50 group">
            <div class="flex justify-between items-start mb-4">
                <div class="w-12 h-12 bg-emerald-50 text-emerald-600 rounded-xl flex items-center justify-center">
                    <Package class="w-6 h-6" />
                </div>
                <div class="text-right">
                    <p class="text-[9px] font-black text-slate-300 uppercase">Tồn kho</p>
                    <p class="text-2xl font-black text-slate-800 tracking-tighter">{{ item.totalStock }} <span class="text-[10px] text-slate-400">{{ item.unit }}</span></p>
                </div>
            </div>
            <h4 class="font-black text-slate-700 text-sm mb-4 leading-tight group-hover:text-emerald-600 transition-colors uppercase">{{ item.supplyName }}</h4>
            <div class="pt-4 border-t border-slate-50 flex justify-end">
                <button v-if="authStore.role === 'Admin'" @click="deleteSupply(item.supplyId)" class="text-slate-300 hover:text-rose-500 opacity-0 group-hover:opacity-100 transition-all"><Trash2 class="w-4 h-4" /></button>
            </div>
        </div>
    </div>

    <!-- Vouchers Tab -->
    <div v-if="activeTab === 'vouchers'" class="premium-card overflow-hidden bg-white border border-slate-100">
        <div class="overflow-x-auto">
            <table class="w-full text-left">
                <thead class="bg-slate-50 text-[10px] font-black uppercase text-slate-400">
                    <tr>
                        <th class="p-4">Mã Phiếu</th>
                        <th class="p-4">Ngày tạo</th>
                        <th class="p-4">Loại phiếu</th>
                        <th class="p-4">Đối tượng</th>
                        <th class="p-4">Người lập</th>
                        <th class="p-4 text-center">Tác vụ</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="v in vouchers" :key="v.voucherId" class="text-xs hover:bg-slate-50/50 transition-all">
                        <td class="p-4 font-black text-slate-800">#{{ v.voucherCode }}</td>
                        <td class="p-4 text-slate-500">{{ formatDate(v.createdAt) }}</td>
                        <td class="p-4">
                            <span :class="['px-3 py-1 rounded-full text-[9px] font-black uppercase', v.type === 'Import' ? 'bg-emerald-50 text-emerald-600' : 'bg-rose-50 text-rose-600']">
                                {{ v.type === 'Import' ? 'NHẬP KHO' : 'XUẤT ĐOÀN' }}
                            </span>
                        </td>
                        <td class="p-4 font-bold text-slate-600">{{ v.groupName || 'Hệ thống/NCC' }}</td>
                        <td class="p-4 text-slate-500">{{ v.createdBy }}</td>
                        <td class="p-4 text-center">
                            <button @click="viewVoucher(v)" class="text-indigo-600 hover:underline font-black">XEM CHI TIẾT</button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Voucher Creation Modal -->
    <div v-if="modals.voucher.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4">
        <div class="bg-white w-full max-w-2xl p-8 rounded-[2rem] shadow-2xl animate-fade-in-up">
            <h3 class="text-xl font-black text-slate-800 mb-6 uppercase">Lập Phiếu Kho Mới</h3>
            <form @submit.prevent="createVoucher" class="space-y-6">
                <div class="grid grid-cols-2 gap-4">
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Loại nghiệp vụ</label>
                        <select v-model="modals.voucher.data.type" required class="input-premium">
                            <option value="Import">Nhập kho từ NCC</option>
                            <option value="Export">Xuất kho đi Đoàn</option>
                        </select>
                    </div>
                    <div v-if="modals.voucher.data.type === 'Export'" class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Đoàn khám nhận</label>
                        <select v-model="modals.voucher.data.groupId" required class="input-premium">
                            <option v-for="g in openGroups" :key="g.groupId" :value="g.groupId">{{ g.groupName }}</option>
                        </select>
                    </div>
                </div>

                <!-- Item Selector -->
                <div class="space-y-4">
                    <label class="text-[10px] font-black uppercase text-slate-400">Danh sách vật tư</label>
                    <div class="space-y-2 max-h-60 overflow-y-auto pr-2">
                        <div v-for="(item, idx) in modals.voucher.data.details" :key="idx" class="flex gap-2 items-center">
                            <select v-model="item.supplyId" required class="flex-1 input-premium">
                                <option v-for="s in list" :key="s.supplyId" :value="s.supplyId">{{ s.supplyName }} (Tồn: {{ s.totalStock }})</option>
                            </select>
                            <input v-model="item.quantity" type="number" required placeholder="SL" class="w-24 input-premium text-center" />
                            <button type="button" @click="removeItem(idx)" class="text-rose-400"><X class="w-4 h-4" /></button>
                        </div>
                    </div>
                    <button type="button" @click="addItem" class="text-[10px] font-black text-emerald-600 uppercase">+ THÊM DÒNG</button>
                </div>

                <div class="flex gap-3 pt-6">
                    <button type="button" @click="modals.voucher.show = false" class="flex-1 py-3 text-slate-400 font-black text-xs uppercase underline">QUAY LẠI</button>
                    <button type="submit" class="flex-[3] bg-emerald-600 text-white py-3 rounded-xl font-black text-xs uppercase shadow-lg">XÁC NHẬN LẬP PHIẾU</button>
                </div>
            </form>
        </div>
    </div>

    <!-- Voucher Detail Modal -->
    <div v-if="viewingVoucher" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4">
        <div class="bg-white w-full max-w-lg p-10 rounded-[2rem] shadow-2xl relative">
            <button @click="viewingVoucher = null" class="absolute top-6 right-6 text-slate-300 hover:text-slate-600"><X class="w-6 h-6" /></button>
            <div class="text-center mb-8">
                <p class="text-[10px] font-black text-slate-300 uppercase">Chi tiết Phiếu Kho</p>
                <h3 class="text-2xl font-black text-slate-800">#{{ viewingVoucher.voucherCode }}</h3>
                <p class="text-xs font-bold text-emerald-600 uppercase mt-2">{{ viewingVoucher.type === 'Import' ? 'NHẬP KHO' : 'XUẤT ĐOÀN' }}</p>
            </div>
            
            <div class="space-y-6">
                <div class="grid grid-cols-2 gap-6 text-xs border-b border-slate-50 pb-6">
                    <div>
                        <p class="font-black text-slate-300 uppercase mb-1">Ngày tạo</p>
                        <p class="font-bold">{{ formatDate(viewingVoucher.createdAt) }}</p>
                    </div>
                    <div>
                        <p class="font-black text-slate-300 uppercase mb-1">Người lập</p>
                        <p class="font-bold">{{ viewingVoucher.createdBy }}</p>
                    </div>
                </div>

                <div class="space-y-3">
                    <p class="text-[9px] font-black text-slate-300 uppercase">Mặt hàng luân chuyển</p>
                    <div class="space-y-2">
                        <div v-for="d in viewingVoucher.details" :key="d.id" class="flex justify-between items-center bg-slate-50 p-3 rounded-xl">
                            <span class="font-bold text-slate-700">{{ d.supplyName }}</span>
                            <span class="font-black text-slate-800">{{ d.quantity }}</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { Package, Plus, ClipboardList, Trash2, X, FileText } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'

const authStore = useAuthStore()
const toast = useToast()
const list = ref([])
const vouchers = ref([])
const openGroups = ref([])
const activeTab = ref('inventory')
const showAddSupply = ref(false)
const viewingVoucher = ref(null)

const newSupply = ref({ supplyName: '', unit: '' })
const modals = ref({
    voucher: { show: false, data: { type: 'Import', groupId: null, details: [{ supplyId: null, quantity: 1 }] } }
})

const fetchData = async () => {
    try {
        const [sRes, vRes, gRes] = await Promise.all([
            axios.get('http://localhost:5283/api/Supplies'),
            axios.get('http://localhost:5283/api/Supplies/vouchers'),
            axios.get('http://localhost:5283/api/MedicalGroups')
        ]);
        list.value = sRes.data
        vouchers.value = vRes.data
        openGroups.value = gRes.data.filter(g => g.status === 'Open')
    } catch (e) { toast.error("Lỗi khi tải kho") }
}

const addSupply = async () => {
    try {
        await axios.post('http://localhost:5283/api/Supplies', newSupply.value)
        toast.success("Đã thêm vào danh mục!")
        showAddSupply.value = false
        newSupply.value = { supplyName: '', unit: '' }
        fetchData()
    } catch (e) { toast.error("Lỗi khi thêm danh mục") }
}

const deleteSupply = async (id) => {
    if (!confirm("Bạn có chắc chắn muốn xóa?")) return
    try {
        await axios.delete(`http://localhost:5283/api/Supplies/${id}`)
        toast.success("Đã xóa!")
        fetchData()
    } catch (e) { toast.error("Không thể xóa vật tư này") }
}

const openVoucherModal = () => {
    modals.value.voucher.data = { type: 'Import', groupId: null, details: [{ supplyId: list.value[0]?.supplyId || null, quantity: 1 }] }
    modals.value.voucher.show = true
}

const addItem = () => {
    modals.value.voucher.data.details.push({ supplyId: list.value[0]?.supplyId || null, quantity: 1 })
}

const removeItem = (idx) => {
    modals.value.voucher.data.details.splice(idx, 1)
}

const createVoucher = async () => {
    try {
        await axios.post('http://localhost:5283/api/Supplies/vouchers', modals.value.voucher.data)
        toast.success("Đã lập phiếu kho thành công!")
        modals.value.voucher.show = false
        fetchData()
    } catch (e) { toast.error(e.response?.data || "Lỗi khi lập phiếu") }
}

const viewVoucher = (v) => { viewingVoucher.value = v }
const formatDate = (d) => new Date(d).toLocaleDateString('vi-VN')
const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)

onMounted(fetchData)
</script>
